﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Parser.Html;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Remote.Common;
using FakeItEasy;
using FluentAssertions;
using Xunit;
// ReSharper disable SuggestVarOrType_SimpleTypes

namespace DadsEnergyReporter.Remote.OrangeRockland.Client
{
    public class OrangeRocklandAuthenticationClientTest
    {
        private readonly OrangeRocklandAuthenticationClientImpl client;
        private readonly ApiClient apiClient = A.Fake<ApiClient>();
        private readonly FakeHttpMessageHandler httpMessageHander = A.Fake<FakeHttpMessageHandler>();
        private readonly ContentHandlers contentHandlers = A.Fake<ContentHandlers>();

        public OrangeRocklandAuthenticationClientTest()
        {
            client = new OrangeRocklandAuthenticationClientImpl(new OrangeRocklandClientImpl(apiClient));

            A.CallTo(() => apiClient.HttpClient).Returns(new HttpClient(httpMessageHander));
            A.CallTo(() => apiClient.ContentHandlers).Returns(contentHandlers);
        }

        [Fact]
        public async void LogOut()
        {
            var response = A.Fake<HttpResponseMessage>();
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._)).Returns(response);

            await client.LogOut();

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

            Func<Task> thrower = async () => await client.LogOut();

            thrower.ShouldThrow<OrangeRocklandException>().WithMessage("Failed to log out");
        }

        [Fact]
        public async void FetchPreLogInData()
        {
            var response = A.Fake<HttpResponseMessage>();
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._)).Returns(response);

            var doc = new HtmlParser().Parse(File.Open("Data/oru-login.html", FileMode.Open));
            A.CallTo(() => contentHandlers.ReadContentAsHtml(response)).Returns(doc);

            IDictionary<string, string> actual = await client.FetchPreLogInData();

            actual.Should().HaveCount(6);
            actual.Should().Contain("__EVENTTARGET", "");
            actual.Should().Contain("__EVENTARGUMENT", "");
            actual.Should().Contain("__VIEWSTATEGENERATOR", "4B8780CA");
            actual.Should().Contain("__PREVIOUSPAGE",
                "P92a2vVV-IF1-sEagJhcgzXDysYrpkOSfCGX1p7FNa9HLFVKcNx4F7o8KY7h-pYceT0HBgyQ-O5WMSlRYDoR-agFhQ4SMOn3vbDPKbQlxnE1");
            actual.Should().Contain("__EVENTVALIDATION",
                "ZlKdECKyY3B752Uvz8n+TJQHIlMBxwWm+vVoaXcmxP69SqcoqsjU9PIj86q+tk8/FP4anucNKrXsLjH7hkywTwwwp82VcsILwxu/tStrp2gtTm2Tnm1Qr6APlsV5tH7puTBLGLKG5AJCHD8hfat1ZXDVTQhzUpEmv79UML5vsfWsQoYNuCKAzTZfvEUHOtrsAvNvapuvwR5scWsjunPJA4X9Byqh8tO30OIZ0U0FoaKcmjqCHUCzdTrHsFOK0q4zSVlV8T0ZQLQmSgdFmA+GO3KipxBpvQf6DVHNtlX8afk=");
            actual.Should().Contain("__VIEWSTATE",
                @"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");

            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>.That.Matches(message =>
                message.Method == HttpMethod.Get
                && message.RequestUri.ToString().Equals("https://apps.coned.com/ORMyAccount/Forms/login.aspx")
            ))).MustHaveHappened();
        }

        [Fact]
        public void FetchPreLogInDataFailure()
        {
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._))
                .ThrowsAsync(new HttpRequestException());

            Func<Task> thrower = async () => await client.FetchPreLogInData();

            thrower.ShouldThrow<OrangeRocklandException>().WithMessage("Auth Phase 1/2: Failed to fetch pre-log-in data");
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

            OrangeRocklandAuthToken actual = await client.SubmitCredentials("user", "pass",
                new Dictionary<string, string>
                {
                    ["hiddenKey1"] = "hiddenValue1",
                    ["hiddenKey2"] = "hiddenValue2"
                });

            actual.LogInCookie.Should().Be("hargle");

            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>.That.Matches(message =>
                message.Method == HttpMethod.Post
                && message.RequestUri.ToString().Equals("https://apps.coned.com/ORMyAccount/Forms/login.aspx")
            ))).MustHaveHappened();

            requestBody.Should().Be("txtUsername=user" +
                                    "&txtPassword=pass" +
                                    "&hiddenKey1=hiddenValue1" +
                                    "&hiddenKey2=hiddenValue2");
        }

        [Fact]
        public void SubmitCredentialsFailsNoCookie()
        {
            var response = A.Fake<HttpResponseMessage>();
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._)).Returns(response);
            A.CallTo(() => apiClient.Cookies).Returns(new CookieContainer());

            Func<Task> thrower = async () => await client.SubmitCredentials("user", "pass", new Dictionary<string, string>());

            thrower.ShouldThrow<OrangeRocklandException>().WithMessage(
                "Auth Phase 2/2: No LogCOOKPl95FnjAT cookie was set after submitting credentials, username or password may be incorrect.");
        }
        
        [Fact]
        public void SubmitCredentialsFailsBadResponse()
        {
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._))
                .ThrowsAsync(new HttpRequestException());

            Func<Task> thrower = async () => await client.SubmitCredentials("user", "pass", new Dictionary<string, string>());

            thrower.ShouldThrow<OrangeRocklandException>().WithMessage(
                "Auth Phase 2/2: Failed to log in with credentials, Orange and Rockland site may be unavailable.");
        }
    }
}