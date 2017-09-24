using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using DadsEnergyReporter.Remote.Common;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace DadsEnergyReporter.Remote.OrangeRockland.Client
{
    public class GreenButtonClientTest
    {
        private readonly GreenButtonClient client;
        private readonly ApiClient apiClient = A.Fake<ApiClient>();
        private readonly FakeHttpMessageHandler httpMessageHander = A.Fake<FakeHttpMessageHandler>();
        private readonly ContentHandlers contentHandlers = A.Fake<ContentHandlers>();

        public GreenButtonClientTest()
        {
            client = new GreenButtonClientImpl(new OrangeRocklandClientImpl(apiClient));

            A.CallTo(() => apiClient.HttpClient).Returns(new HttpClient(httpMessageHander));
            A.CallTo(() => apiClient.ContentHandlers).Returns(contentHandlers);
        }

        [Fact]
        public async void FetchGreenButtonData()
        {
            var response = A.Fake<HttpResponseMessage>();
            var doc = new XDocument();
            string requestBody = null;
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._)).ReturnsLazily(async call =>
            {
                requestBody = await call.Arguments[0].As<HttpRequestMessage>().Content.ReadAsStringAsync();
                return response;
            });
            A.CallTo(() => contentHandlers.ReadContentAsXml(A<HttpResponseMessage>._)).Returns(doc);

            XDocument actual = await client.FetchGreenButtonData();

            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>.That.Matches(message =>
                    message.Method == HttpMethod.Post
                    && message.RequestUri.ToString()
                        .Equals("https://apps.coned.com/ORMyAccount/Forms/Billing/GreenButtonData.aspx")
                )))
                .MustHaveHappened();

            requestBody.Should().Be("OptEnergy=E&optFileFormat=XML");

            actual.Should().BeSameAs(doc);
        }

        [Fact]
        public void Failure()
        {
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._))
                .ThrowsAsync(new HttpRequestException());

            Func<Task> act = async () => await client.FetchGreenButtonData();

            act.ShouldThrow<OrangeRocklandException>().WithMessage("Failed to download Green Button data.");
        }
    }
}