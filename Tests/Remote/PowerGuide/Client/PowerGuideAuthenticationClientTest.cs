using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Parser.Html;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Remote.Common;
using DadsEnergyReporter.Remote.OrangeRockland;
using FakeItEasy;
using FluentAssertions;
using Xunit;

// ReSharper disable SuggestVarOrType_SimpleTypes

namespace DadsEnergyReporter.Remote.PowerGuide.Client
{
    public class PowerGuideAuthenticationClientTest
    {
        private readonly PowerGuideAuthenticationClient client;
        private readonly ApiClient apiClient = A.Fake<ApiClient>();
        private readonly FakeHttpMessageHandler httpMessageHander = A.Fake<FakeHttpMessageHandler>();
        private readonly ContentHandlers contentHandlers = A.Fake<ContentHandlers>();

        public PowerGuideAuthenticationClientTest()
        {
            JsonSerializerConfigurer.ConfigureDefault();

            client = new PowerGuideAuthenticationClientImpl(new PowerGuideClientImpl(apiClient));

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
                && message.RequestUri.ToString() == "https://mysolarcity.com/Logout.aspx"
            ))).MustHaveHappened();
        }

        [Fact]
        public void LogOutFailure()
        {
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._))
                .ThrowsAsync(new HttpRequestException());

            Func<Task> thrower = async () => await client.LogOut();

            thrower.ShouldThrow<PowerGuideException>().WithMessage("Failed to log out");
        }

        [Fact]
        public async void FetchPreLogInData()
        {
            var response = A.Fake<HttpResponseMessage>();
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._)).Returns(response);

            var doc = new HtmlParser().Parse(File.Open("Data/solarcity-login.html", FileMode.Open));
            A.CallTo(() => contentHandlers.ReadContentAsHtml(response)).Returns(doc);

            PreLogInData actual = await client.FetchPreLogInData();

            actual.CsrfToken.Should().Be(
                "AAEAADs8_pWJ1-mMSxXSyne1GfJK4mpiFEvpzx0H4QuZcyOzvhUazxugwlou0Ay3rK8wNvDuo4Knpm3lGpH0ccZ8KE5coMqF238vBY-BR823zQmLHvD2hn--JpbdIq5isvqiTGS-DLf9IgNc-Pph_-K4Tz0HduJgLMI6EVBf4wBYfsQGlBZDkQ9HFk2yciyHHqnl9dzLMZTPj1cNQrLZRN4tNpRNzy06tijZHwYfnp89HiTYRdb0Pxe8OsZu9nch3osHNpsNyeamXqQ02qao4zEYI2ZFMyNzwAEpieMlFawDcdLLmL__C-BIkXdqmOYgBIaVKwrBHdfQdpGq628QiC-5oUwswJDMnzjEMnUhlo8yRJUmIa_vo1MbGSH1obG1AhiU5wABAACOtoMOY_r2dfuBF9Pc0tz-fi5cogml1fVaetrrXVQ0LElY2Ova0-vqSYQ9rC0ygjLOIzDt0IvqiN8VT18bIEd-4YARmH2LxxEMMw5lXc2NgV_0c5qj_kEO6bSOJyu1Qyq_DtN9DUFVdXa04ZY3X0CWOf7DLGDF-P_RLkHclHc-eeeo7NZwQyFooAAnytoDVZB01JqfMHyZAxcalNs7Z8UnOwIxwplY5pgb2T1v5tNvQ1yIk9_BO6MovmkCDM8V8ATUySXGcjyQHqJi0ogrK5NMoEGt8G0G5oJ2JolPGA8XW1iw5Ja10Mu19EPdqHiXZxjgtTMkmA62c-BlP_Wuk1U6IAAAAB_Fer7EeK4abWYA6T-DJzyzl36Mfntzc3_pNTEAB2Hl");
            actual.LogInUri.ToString().Should()
                .Be("https://login.solarcity.com/account/SignIn?signin=f1938d8ff30a3b6648ebd619e4278c46");

            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>.That.Matches(message =>
                message.Method == HttpMethod.Get
                && message.RequestUri.ToString().Equals("https://mysolarcity.com/")
            ))).MustHaveHappened();
        }

        [Fact]
        public void FetchPreLogInDataFailure()
        {
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._))
                .ThrowsAsync(new HttpRequestException());

            Func<Task> thrower = async () => await client.FetchPreLogInData();

            thrower.ShouldThrow<PowerGuideException>().WithMessage("Auth Phase 1/3: Failed to fetch pre-log-in data");
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

            var doc = new HtmlParser().Parse(File.Open("Data/solarcity-submit-credentials.html", FileMode.Open));
            A.CallTo(() => contentHandlers.ReadContentAsHtml(response)).Returns(doc);

            PreLogInData preLogInData = new PreLogInData
            {
                CsrfToken =
                    "AAEAADs8_pWJ1-mMSxXSyne1GfJK4mpiFEvpzx0H4QuZcyOzvhUazxugwlou0Ay3rK8wNvDuo4Knpm3lGpH0ccZ8KE5coMqF238vBY-BR823zQmLHvD2hn--JpbdIq5isvqiTGS-DLf9IgNc-Pph_-K4Tz0HduJgLMI6EVBf4wBYfsQGlBZDkQ9HFk2yciyHHqnl9dzLMZTPj1cNQrLZRN4tNpRNzy06tijZHwYfnp89HiTYRdb0Pxe8OsZu9nch3osHNpsNyeamXqQ02qao4zEYI2ZFMyNzwAEpieMlFawDcdLLmL__C-BIkXdqmOYgBIaVKwrBHdfQdpGq628QiC-5oUwswJDMnzjEMnUhlo8yRJUmIa_vo1MbGSH1obG1AhiU5wABAACOtoMOY_r2dfuBF9Pc0tz-fi5cogml1fVaetrrXVQ0LElY2Ova0-vqSYQ9rC0ygjLOIzDt0IvqiN8VT18bIEd-4YARmH2LxxEMMw5lXc2NgV_0c5qj_kEO6bSOJyu1Qyq_DtN9DUFVdXa04ZY3X0CWOf7DLGDF-P_RLkHclHc-eeeo7NZwQyFooAAnytoDVZB01JqfMHyZAxcalNs7Z8UnOwIxwplY5pgb2T1v5tNvQ1yIk9_BO6MovmkCDM8V8ATUySXGcjyQHqJi0ogrK5NMoEGt8G0G5oJ2JolPGA8XW1iw5Ja10Mu19EPdqHiXZxjgtTMkmA62c-BlP_Wuk1U6IAAAAB_Fer7EeK4abWYA6T-DJzyzl36Mfntzc3_pNTEAB2Hl",
                LogInUri = new Uri("https://login.solarcity.com/account/SignIn?signin=f1938d8ff30a3b6648ebd619e4278c46")
            };
            IDictionary<string, string>
                actual = await client.SubmitCredentials("user@domain.com", "pass", preLogInData);

            actual.Count.Should().Be(3);
            actual.Should().Contain("wa", "wsignin1.0");
            actual.Should().Contain("wresult",
                @"<trust:RequestSecurityTokenResponseCollection xmlns:trust=""http://docs.oasis-open.org/ws-sx/ws-trust/200512""><trust:RequestSecurityTokenResponse Context=""rm=0&amp;id=passive&amp;ru=%2f""><wsp:AppliesTo xmlns:wsp=""http://schemas.xmlsoap.org/ws/2004/09/policy""><wsa:EndpointReference xmlns:wsa=""http://www.w3.org/2005/08/addressing""><wsa:Address>https://mysolarcity.com/</wsa:Address></wsa:EndpointReference></wsp:AppliesTo><trust:RequestedSecurityToken><wsse:BinarySecurityToken ValueType=""urn:ietf:params:oauth:token-type:jwt"" EncodingType=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary"" xmlns:wsse=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"">ZXlKMGVYQWlPaUpLVjFRaUxDSmhiR2NpT2lKU1V6STFOaUlzSW5nMWRDSTZJbWRUUWtWU1pVSkRPVWQwUW1wTlFWcHFlak5ZWVdFMFMwbFpkeUo5LmV5Sm9kSFJ3T2k4dmMyOXNZWEpqYVhSNUxtTnZiUzkzY3k4eU1ERTBMekF5TDJsa1pXNTBhWFI1TDJOc1lXbHRjeTlqYjI1MFlXTjBMV2xrSWpvaU5ESXdPRFF6T0NJc0ltaDBkSEE2THk5emIyeGhjbU5wZEhrdVkyOXRMM2R6THpJd01UUXZNRFF2YVdSbGJuUnBkSGt2WTJ4aGFXMXpMMk52Ym5SaFkzUXRaM1ZwWkNJNkltSmpPVFkxWXpFekxUQXhZamN0TkROa01TMWlNalV6TFRBNE1UUTROMkZrTjJNMVppSXNJbVZ0WVdsc0lqb2laWFpoYmtCaGJHUmhkbWwyWVM1amIyMGlMQ0pvZEhSd09pOHZjMjlzWVhKamFYUjVMbU52YlM5M2N5OHlNREUwTHpBMEwybGtaVzUwYVhSNUwyTnNZV2x0Y3k5amIyNTBZV04wTFhSNWNHVWlPaUpEZFhOMGIyMWxjaUlzSW1GMWRHaHRaWFJvYjJRaU9pSm9kSFJ3T2k4dmMyTm9aVzFoY3k1dGFXTnliM052Wm5RdVkyOXRMM2R6THpJd01EZ3ZNRFl2YVdSbGJuUnBkSGt2WVhWMGFHVnVkR2xqWVhScGIyNXRaWFJvYjJRdmNHRnpjM2R2Y21RaUxDSmhkWFJvWDNScGJXVWlPaUl5TURFM0xUQTVMVEEwVkRBeU9qUXlPakkwTGpBNU5Gb2lMQ0pwYzNNaU9pSlRiMnhoY2tOcGRIbFRWRk1pTENKaGRXUWlPaUpvZEhSd2N6b3ZMMjE1YzI5c1lYSmphWFI1TG1OdmJTOGlMQ0psZUhBaU9qRTFNRFExTWpFM05EUXNJbTVpWmlJNk1UVXdORFE1TWprME5IMC5GNDF5a29GclZGX1dVN2EydEhLRzItOHE0Y1ZMZWxFQmRhLXViSzNiTVYtcVJ6QXM3am82RE1OUXFqaVJxdTBSenhqTHF3VC1WSEo4WWdfMXZ1WEtiX0hIUUJ2aE5uYnR1OTQtVnNIU2h3LUpEUUYtRHVNZXRBYjFMcHZXV1RXTlo2c2ZiMG45dWxaWmZUcExUdGc4ZHZRR3U3NVdlSDlyWXpvN2tfbzRTQUtBYTF6S09zWVo0RTBZYmRxTmsxRk1aWmxYNFFIb0lHTmc4cmJCUTFkeVhEVks0Y1pEUUgtbS1HS1pvaExFcnlzRTd2WjZfY292S0xkVGd1dWJJXzE4TjdOV3NEY05DUFAtUWNCX01qd0R4QV9JYmlzQWNpMzgxNTl0RGs4Y0x5ci05SGNPcDBNNTUzb3ByTFE2S1FTMlFPMzdVWmtqelcyRzZsUGFVRFdyY1E=</wsse:BinarySecurityToken></trust:RequestedSecurityToken></trust:RequestSecurityTokenResponse></trust:RequestSecurityTokenResponseCollection>");
            actual.Should().Contain("wctx", "rm=0&id=passive&ru=%2f");

            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>.That.Matches(message =>
                message.Method == HttpMethod.Post
                && message.RequestUri.ToString()
                    .Equals("https://login.solarcity.com/account/SignIn?signin=f1938d8ff30a3b6648ebd619e4278c46")
            ))).MustHaveHappened();

            requestBody.Should().Be(
                "idsrv.xsrf=AAEAADs8_pWJ1-mMSxXSyne1GfJK4mpiFEvpzx0H4QuZcyOzvhUazxugwlou0Ay3rK8wNvDuo4Knpm3lGpH0ccZ8KE5coMqF238vBY-BR823zQmLHvD2hn--JpbdIq5isvqiTGS-DLf9IgNc-Pph_-K4Tz0HduJgLMI6EVBf4wBYfsQGlBZDkQ9HFk2yciyHHqnl9dzLMZTPj1cNQrLZRN4tNpRNzy06tijZHwYfnp89HiTYRdb0Pxe8OsZu9nch3osHNpsNyeamXqQ02qao4zEYI2ZFMyNzwAEpieMlFawDcdLLmL__C-BIkXdqmOYgBIaVKwrBHdfQdpGq628QiC-5oUwswJDMnzjEMnUhlo8yRJUmIa_vo1MbGSH1obG1AhiU5wABAACOtoMOY_r2dfuBF9Pc0tz-fi5cogml1fVaetrrXVQ0LElY2Ova0-vqSYQ9rC0ygjLOIzDt0IvqiN8VT18bIEd-4YARmH2LxxEMMw5lXc2NgV_0c5qj_kEO6bSOJyu1Qyq_DtN9DUFVdXa04ZY3X0CWOf7DLGDF-P_RLkHclHc-eeeo7NZwQyFooAAnytoDVZB01JqfMHyZAxcalNs7Z8UnOwIxwplY5pgb2T1v5tNvQ1yIk9_BO6MovmkCDM8V8ATUySXGcjyQHqJi0ogrK5NMoEGt8G0G5oJ2JolPGA8XW1iw5Ja10Mu19EPdqHiXZxjgtTMkmA62c-BlP_Wuk1U6IAAAAB_Fer7EeK4abWYA6T-DJzyzl36Mfntzc3_pNTEAB2Hl" +
                "&username=user%40domain.com" +
                "&password=pass" +
                "&RecaptchaResponse=");
        }

        [Fact]
        public void SubmitCredentialsFailure()
        {
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._))
                .ThrowsAsync(new HttpRequestException());

            Func<Task> thrower = async () =>
            {
                var preLogInData = new PreLogInData
                {
                    LogInUri = new Uri(
                        "https://login.solarcity.com/account/SignIn?signin=f1938d8ff30a3b6648ebd619e4278c46")
                };
                await client.SubmitCredentials("user@domain.com", "pass", preLogInData);
            };

            thrower.ShouldThrow<PowerGuideException>()
                .WithMessage(
                    "Auth Phase 2/3: Failed to log in with credentials, username or password may be incorrect.");
        }

        [Fact]
        public async void FetchAuthToken()
        {
            var response = A.Fake<HttpResponseMessage>();
            string requestBody = null;
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._)).ReturnsLazily(async call =>
            {
                requestBody = await call.Arguments[0].As<HttpRequestMessage>().Content.ReadAsStringAsync();
                return response;
            });

            var cookieContainer = new CookieContainer();
            cookieContainer.Add(new Uri("https://mysolarcity.com/"), new Cookie("FedAuth", "hargle"));
            A.CallTo(() => apiClient.Cookies).Returns(cookieContainer);

            var credentialResponse = new Dictionary<string, string>
            {
                ["wa"] = "wsignin1.0",
                ["wresult"] = "some XML bullshit",
                ["wctx"] = "rm=0&id=passive&ru=%2f"
            };

            PowerGuideAuthToken actual = await client.FetchAuthToken(credentialResponse);

            actual.FedAuth.Should().Be("hargle");

            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>.That.Matches(message =>
                message.Method == HttpMethod.Post
                && message.RequestUri.ToString().Equals("https://mysolarcity.com/")
            ))).MustHaveHappened();

            requestBody.Should().Be("wa=wsignin1.0" +
                                    "&wresult=some+XML+bullshit" +
                                    "&wctx=rm%3D0%26id%3Dpassive%26ru%3D%252f");
        }

        [Fact]
        public void FetchAuthTokenFailsNoCookie()
        {
            var response = A.Fake<HttpResponseMessage>();
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._)).Returns(response);
            A.CallTo(() => apiClient.Cookies).Returns(new CookieContainer());

            Func<Task> thrower = async () => await client.FetchAuthToken(new Dictionary<string, string>());

            thrower.ShouldThrow<PowerGuideException>().WithMessage(
                "Auth Phase 3/3: No FedAuth cookie was set while fetching auth token from https://mysolarcity.com/");
        }

        [Fact]
        public void FetchAuthTokenFailsBadResponse()
        {
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._))
                .ThrowsAsync(new HttpRequestException());

            Func<Task> thrower = async () => await client.FetchAuthToken(new Dictionary<string, string>());

            thrower.ShouldThrow<PowerGuideException>()
                .WithMessage("Auth Phase 3/3: Failed to fetch auth token based on credential response");
        }
    }
}