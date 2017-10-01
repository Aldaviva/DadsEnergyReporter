using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Remote.Common;
using FakeItEasy;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace DadsEnergyReporter.Remote.PowerGuide.Client
{
    public class InstallationsTest
    {
        private readonly InstallationClientImpl client;
        private readonly ApiClient apiClient = A.Fake<ApiClient>();
        private readonly FakeHttpMessageHandler httpMessageHander = A.Fake<FakeHttpMessageHandler>();
        private readonly ContentHandlers contentHandlers = A.Fake<ContentHandlers>();

        public InstallationsTest()
        {
            client = new InstallationClientImpl(new PowerGuideClientImpl(apiClient));

            A.CallTo(() => apiClient.HttpClient).Returns(new HttpClient(httpMessageHander));
            A.CallTo(() => apiClient.ContentHandlers).Returns(contentHandlers);
        }

        [Fact]
        public async void FetchInstallationId()
        {
            var response = A.Fake<HttpResponseMessage>();
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._)).Returns(response);
            A.CallTo(() => contentHandlers.ReadContentJsonAs<InstallationsResponse>(response))
                .Returns(JsonConvert.DeserializeObject<InstallationsResponse>(
                    File.ReadAllText("data/installations.json")));

            Installation actual = (await client.FetchInstallations()).First();

            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>.That.Matches(message =>
                message.Method == HttpMethod.Get
                && message.RequestUri.ToString() == "https://mysolarcity.com/solarcity-api/powerguide/v1.0/installations"
            ))).MustHaveHappened();

            actual.Guid.Should().Be(new Guid("80dfb10d-0e02-4993-aa9a-bd241b2df7f9"));
        }

        [Fact]
        public void FetchInstallationIdFailure()
        {
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._))
                .ThrowsAsync(new HttpRequestException());

            Func<Task> thrower = async () => await client.FetchInstallations();

            thrower.ShouldThrow<PowerGuideException>()
                .WithMessage("Failed to fetch installation ID of the solar panels at the house");
        }
    }
}