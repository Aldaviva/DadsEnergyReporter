using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Remote.Common;
using FakeItEasy;
using FluentAssertions;
using Xunit;

// ReSharper disable SuggestVarOrType_SimpleTypes

namespace DadsEnergyReporter.Remote.OrangeRockland.Client
{
    public class OrangeRocklandAuthenticationClientTest
    {
        private readonly OrangeRocklandAuthenticationClientImpl authClient;
        private readonly ApiClient apiClient = A.Fake<ApiClient>();
        private readonly FakeHttpMessageHandler httpMessageHander = A.Fake<FakeHttpMessageHandler>();
        private readonly ContentHandlers contentHandlers = A.Fake<ContentHandlers>();

        public OrangeRocklandAuthenticationClientTest()
        {
            var client = A.Fake<OrangeRocklandClientImpl>();
            A.CallTo(() => client.ApiClient).Returns(apiClient);

            authClient = new OrangeRocklandAuthenticationClientImpl(client);
            A.CallTo(() => apiClient.HttpClient).Returns(new HttpClient(httpMessageHander));
            A.CallTo(() => apiClient.ContentHandlers).Returns(contentHandlers);
        }

        [Fact]
        public async void LogOut()
        {
            var response = A.Fake<HttpResponseMessage>();
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._)).Returns(response);

            await authClient.LogOut();

            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>.That.Matches(message =>
                message.Method == HttpMethod.Get
                && message.RequestUri.ToString().Equals("https://apps.coned.com/ORMyAccount/Forms/logoff.aspx")
            ))).MustHaveHappened();
        }

        [Fact]
        public void LogOutFailure()
        {
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._))
                .ThrowsAsync(new HttpRequestException());

            Func<Task> thrower = async () => await authClient.LogOut();

            thrower.Should().Throw<OrangeRocklandException>().WithMessage("Failed to log out");
        }

        [Fact]
        public async void SubmitCredentials()
        {
            var tokenExchangeUriResponse = A.Fake<HttpResponseMessage>();
            var authTokenResponse = A.Fake<HttpResponseMessage>();
            var sessionActivationResponse = A.Fake<HttpResponseMessage>();

            string credentialsRequestBody = null;
            var tokenExchangeUriResponeBody = new Dictionary<string, object>
            {
                { "login", true },
                { "newDevice", false },
                { "authRedirectUrl", "https://apps.coned.com/ORMyAccount/Forms/DcxLogin.aspx?params=***REMOVED***" },
                { "noMfa", false },
                { "legacyLockout", false }
            };

            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._)).ReturnsLazily(async call =>
                {
                    credentialsRequestBody = await call.Arguments[0].As<HttpRequestMessage>().Content.ReadAsStringAsync();
                    return tokenExchangeUriResponse;
                }).Once().Then
                .Returns(authTokenResponse).Once().Then
                .Returns(sessionActivationResponse).Once();

            A.CallTo(() => contentHandlers.ReadContentAsJson<IDictionary<string, object>>(tokenExchangeUriResponse))
                .Returns(tokenExchangeUriResponeBody);

            var cookieContainer = new CookieContainer();
            cookieContainer.Add(new Uri("https://apps.coned.com/"), new Cookie("LogCOOKPl95FnjAT", "hargle"));
            A.CallTo(() => apiClient.Cookies).Returns(cookieContainer);

            OrangeRocklandAuthToken actual = await authClient.SubmitCredentials("user", "pass");

            actual.LogInCookie.Should().Be("hargle");

            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>.That.Matches(message =>
                message.Method == HttpMethod.Post
                && message.RequestUri.ToString().Equals("https://www.oru.com/sitecore/api/ssc/ConEd-Cms-Services-Controllers-Okta/User/0/Login")
            ))).MustHaveHappened();
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>.That.Matches(message =>
                message.Method == HttpMethod.Get
                && message.RequestUri.ToString().Equals("https://apps.coned.com/ORMyAccount/Forms/DcxLogin.aspx?params=***REMOVED***")
            ))).MustHaveHappened();
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>.That.Matches(message =>
                message.Method == HttpMethod.Get
                && message.RequestUri.ToString().Equals("https://apps.coned.com/ORMyAccount/Forms/System/accountStatus.aspx")
            ))).MustHaveHappened();

            credentialsRequestBody.Should().Be("{" +
                                               "\"LoginEmail\":\"user\"," +
                                               "\"LoginPassword\":\"pass\"," +
                                               "\"LoginRememberMe\":false," +
                                               "\"ReturnUrl\":\"\"" +
                                               "}");
        }

        [Fact]
        public void SubmitCredentialsFailsNoCookie()
        {
            A.CallTo(() => contentHandlers.ReadContentAsJson<IDictionary<string, object>>(A<HttpResponseMessage>._))
                .Returns(new Dictionary<string, object>
            {
                { "login", true },
                { "newDevice", false },
                { "authRedirectUrl", "https://apps.coned.com/ORMyAccount/Forms/DcxLogin.aspx?params=***REMOVED***" },
                { "noMfa", false },
                { "legacyLockout", false }
            });

            var response = A.Fake<HttpResponseMessage>();
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._)).Returns(response);
            A.CallTo(() => apiClient.Cookies).Returns(new CookieContainer());

            Func<Task> thrower = async () => await authClient.SubmitCredentials("user", "pass");

            thrower.Should().Throw<OrangeRocklandException>().WithMessage(
                "Auth Phase 2/3: No LogCOOKPl95FnjAT cookie was set after submitting credentials, username or password may be incorrect.");
        }

        [Fact]
        public void SubmitCredentialsFailsBadResponse()
        {
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._))
                .ThrowsAsync(new HttpRequestException());

            Func<Task> thrower = async () => await authClient.SubmitCredentials("user", "pass");

            thrower.Should().Throw<OrangeRocklandException>().WithMessage(
                "Auth Phase 1/3: Failed to log in with credentials, Orange and Rockland site may be unavailable.");
        }
    }
}