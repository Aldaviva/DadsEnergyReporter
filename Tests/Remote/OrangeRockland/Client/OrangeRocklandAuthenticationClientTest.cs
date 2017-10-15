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
        private readonly OrangeRocklandClientImpl client;

        public OrangeRocklandAuthenticationClientTest()
        {
            client = A.Fake<OrangeRocklandClientImpl>();
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

            thrower.ShouldThrow<OrangeRocklandException>().WithMessage("Failed to log out");
        }

        [Fact]
        public async void SubmitCredentials()
        {
            var response = A.Fake<HttpResponseMessage>();
            string requestBody = null;
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._)).ReturnsLazily(async call =>
            {
                requestBody = await call.Arguments[0].As<HttpRequestMessage>().Content.ReadAsStringAsync();
                return response;
            });

            var cookieContainer = new CookieContainer();
            cookieContainer.Add(new Uri("https://apps.coned.com/"), new Cookie("LogCOOKPl95FnjAT", "hargle"));
            A.CallTo(() => apiClient.Cookies).Returns(cookieContainer);

            A.CallTo(() => client.FetchHiddenFormData(A<Uri>._)).Returns(new Dictionary<string, string>
            {
                ["hiddenKey1"] = "hiddenValue1",
                ["hiddenKey2"] = "hiddenValue2"
            });

            OrangeRocklandAuthToken actual = await authClient.SubmitCredentials("user", "pass");

            actual.LogInCookie.Should().Be("hargle");

            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>.That.Matches(message =>
                message.Method == HttpMethod.Post
                && message.RequestUri.ToString().Equals("https://apps.coned.com/ORMyAccount/Forms/login.aspx")
            ))).MustHaveHappened();

            requestBody.Should().Be("txtUsername=user" +
                                    "&txtPassword=pass" +
                                    "&imgGo.x=1" +
                                    "&imgGo.y=1" +
                                    "&hiddenKey1=hiddenValue1" +
                                    "&hiddenKey2=hiddenValue2");
        }

        [Fact]
        public void SubmitCredentialsFailsNoCookie()
        {
            var response = A.Fake<HttpResponseMessage>();
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._)).Returns(response);
            A.CallTo(() => apiClient.Cookies).Returns(new CookieContainer());

            Func<Task> thrower = async () => await authClient.SubmitCredentials("user", "pass");

            thrower.ShouldThrow<OrangeRocklandException>().WithMessage(
                "Auth Phase 2/2: No LogCOOKPl95FnjAT cookie was set after submitting credentials, username or password may be incorrect.");
        }

        [Fact]
        public void SubmitCredentialsFailsBadResponse()
        {
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._))
                .ThrowsAsync(new HttpRequestException());

            Func<Task> thrower = async () => await authClient.SubmitCredentials("user", "pass");

            thrower.ShouldThrow<OrangeRocklandException>().WithMessage(
                "Auth Phase 2/2: Failed to log in with credentials, Orange and Rockland site may be unavailable.");
        }
    }
}