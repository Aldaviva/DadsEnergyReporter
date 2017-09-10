using System;
using System.Net;
using System.Net.Http;
using FakeItEasy;
using FluentAssertions;
using Newtonsoft.Json;
using PowerGuideReporter.Data.Marshal;
using Xunit;

namespace PowerGuideReporter.Service.Remote.Measurement
{
    public class PowerGuideClientTest : IDisposable
    {
        private readonly HttpClient httpClient;
        private readonly CookieContainer cookieContainer;
        private readonly FakeHttpMessageHandler httpMessageHandler;

        public PowerGuideClientTest()
        {
            httpMessageHandler = A.Fake<FakeHttpMessageHandler>();
            httpClient = new HttpClient(httpMessageHandler);
            cookieContainer = A.Fake<CookieContainer>();
        }

        public void Dispose()
        {
            httpMessageHandler.Dispose();
            httpClient.Dispose();
        }

        [Fact]
        public async void Measurements_FetchInstallationId()
        {
            var client = new PowerGuideClientImpl(httpClient, cookieContainer)
            {
                ResponseReaders = A.Fake<PowerGuideClientImpl.IResponseReaders>()
            };

            var response = A.Fake<HttpResponseMessage>();
            response.StatusCode = HttpStatusCode.OK;
            var installationsResponse = JsonConvert.DeserializeObject<InstallationsResponse>(@"
{
    ""ResultTotal"": 1,
    ""Data"": [
        {
            ""GUID"": ""80dfa10d-0e02-4993-aa9a-bd241b2dc7f9"",
            ""SystemSize"": 12.600,
            ""JobID"": ""JB-1093759-00""
        }
    ]
}
");
            var request = A.CallTo(() => httpMessageHandler.FakeSendAsync(A<HttpRequestMessage>.That.Matches(message =>
                message.RequestUri == new Uri("https://mysolarcity.com:443/solarcity-api/powerguide/v1.0/installations")
                && message.Method == HttpMethod.Get)));
            request.Returns(response);

            A.CallTo(() => client.ResponseReaders.ReadContentJsonAs<InstallationsResponse>(response))
                .Returns(installationsResponse);

            Guid actual = await client.Measurements.FetchInstallationId();

            actual.Should().Be(new Guid("80dfa10d-0e02-4993-aa9a-bd241b2dc7f9"));

            request.MustHaveHappened();
        }

        [Fact]
        public void Measurements_FetchMeasurements()
        {
            
        }
    }
}
