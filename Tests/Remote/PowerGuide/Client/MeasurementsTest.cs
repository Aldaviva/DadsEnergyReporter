using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Remote.Common;
using FakeItEasy;
using FluentAssertions;
using Newtonsoft.Json;
using NodaTime;
using Xunit;

namespace DadsEnergyReporter.Remote.PowerGuide.Client
{
    public class MeasurementsTest
    {
        private readonly MeasurementClientImpl client;
        private readonly ApiClient apiClient = A.Fake<ApiClient>();
        private readonly FakeHttpMessageHandler httpMessageHander = A.Fake<FakeHttpMessageHandler>();
        private readonly ContentHandlers contentHandlers = A.Fake<ContentHandlers>();

        public MeasurementsTest()
        {
            JsonSerializerConfigurer.ConfigureDefault();

            client = new MeasurementClientImpl(new PowerGuideClientImpl(apiClient));

            A.CallTo(() => apiClient.HttpClient).Returns(new HttpClient(httpMessageHander));
            A.CallTo(() => apiClient.ContentHandlers).Returns(contentHandlers);
        }

        [Fact]
        public async void FetchMeasurements()
        {
            var response = A.Fake<HttpResponseMessage>();
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._)).Returns(response);
            A.CallTo(() => contentHandlers.ReadContentAsJson<MeasurementsResponse>(response))
                .Returns(JsonConvert.DeserializeObject<MeasurementsResponse>(
                    File.ReadAllText("data/measurements.json")));

            DateTimeZone zone = DateTimeZoneProviders.Tzdb["America/New_York"];

            MeasurementsResponse actual = await client.FetchMeasurements(
                new Guid("80DFA10D-0E02-4993-AA9A-BD241B2DC7F9"),
                new ZonedDateTime(new LocalDateTime(2017, 7, 17, 0, 0, 0), zone, Offset.FromHours(-4)),
                new ZonedDateTime(new LocalDateTime(2017, 8, 16, 0, 0, 0), zone, Offset.FromHours(-4))
            );

            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>.That.Matches(message =>
                message.Method == HttpMethod.Get
                && message.RequestUri.ToString().Equals(
                    "https://mysolarcity.com/solarcity-api/powerguide/v1.0/measurements/80DFA10D-0E02-4993-AA9A-BD241B2DC7F9?StartTime=2017-07-17T00:00:00&EndTime=2017-08-16T00:00:00&Period=Day",
                    StringComparison.CurrentCultureIgnoreCase)
            ))).MustHaveHappened();

            actual.TotalEnergyInIntervalkWh.Should().Be(1361.32);
            actual.Measurements.Count.Should().Be(31);
            actual.Measurements[0].Timestamp.Should().Be(new LocalDateTime(2017, 7, 17, 0, 0, 0));
            actual.Measurements[0].CumulativekWh.Should().Be(5979.32);
            actual.Measurements[0].EnergyInIntervalkWh.Should().Be(47.38);
            actual.Measurements[0].DataStatus.Should().Be(DataStatus.Validated);
        }

        [Fact]
        public void FetchInstallationIdFailure()
        {
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._))
                .ThrowsAsync(new HttpRequestException());

            Func<Task> thrower = async () =>
                await client.FetchMeasurements(new Guid(), new ZonedDateTime(), new ZonedDateTime());

            thrower.ShouldThrow<PowerGuideException>()
                .WithMessage("Failed to get solar output measurements");
        }
    }
}