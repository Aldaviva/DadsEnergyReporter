using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Remote.Common;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace DadsEnergyReporter.Remote.OrangeRockland.Client
{
    public class GreenButtonClientTest
    {
        private readonly GreenButtonClient greenButtonClient;
        private readonly ApiClient apiClient = A.Fake<ApiClient>();
        private readonly FakeHttpMessageHandler httpMessageHander = A.Fake<FakeHttpMessageHandler>();
        private readonly ContentHandlers contentHandlers = A.Fake<ContentHandlers>();
        private readonly OrangeRocklandClientImpl client;

        public GreenButtonClientTest()
        {
            client = A.Fake<OrangeRocklandClientImpl>();
            A.CallTo(() => client.ApiClient).Returns(apiClient);

            greenButtonClient = new GreenButtonClientImpl(client);
            A.CallTo(() => apiClient.HttpClient).Returns(new HttpClient(httpMessageHander));
            A.CallTo(() => apiClient.ContentHandlers).Returns(contentHandlers);
        }

        [Fact]
        public async void FetchGreenButtonData()
        {
            A.CallTo(() => client.FetchHiddenFormData(A<Uri>._)).Returns(new Dictionary<string, string>
            {
                ["hiddenKey1"] = "hiddenValue1",
                ["hiddenKey2"] = "hiddenValue2"
            });

            var response = A.Fake<HttpResponseMessage>();
            var doc = new XDocument();
            string requestBody = null;
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._)).ReturnsLazily(async call =>
            {
                requestBody = await call.Arguments[0].As<HttpRequestMessage>().Content.ReadAsStringAsync();
                return response;
            });
            A.CallTo(() => contentHandlers.ReadContentAsXml(A<HttpResponseMessage>._)).Returns(doc);

            XDocument actual = await greenButtonClient.FetchGreenButtonData();

            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>.That.Matches(message =>
                    message.Method == HttpMethod.Post
                    && message.RequestUri.ToString()
                        .Equals("https://apps.coned.com/ORMyAccount/Forms/Billing/GreenButtonData.aspx")
                )))
                .MustHaveHappened();

            requestBody.Should().Be("OptEnergy=E" +
                                    "&optFileFormat=XML" +
                                    "&imgGreenButton.x=1" +
                                    "&imgGreenButton.y=1" +
                                    "&hiddenKey1=hiddenValue1" +
                                    "&hiddenKey2=hiddenValue2");

            actual.Should().BeSameAs(doc);
        }

        [Fact]
        public void FetchGreenButtonDataFailure()
        {
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._))
                .ThrowsAsync(new HttpRequestException());

            Func<Task> thrower = async () => await greenButtonClient.FetchGreenButtonData();
            thrower.Should().Throw<OrangeRocklandException>()
                .WithMessage("Failed to download Green Button data: XML request failed");
        }
    }
}