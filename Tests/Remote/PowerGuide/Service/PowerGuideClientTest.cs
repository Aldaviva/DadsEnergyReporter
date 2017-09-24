using System;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Remote.Common;
using DadsEnergyReporter.Remote.PowerGuide.Client;
using FakeItEasy;

// ReSharper disable SuggestVarOrType_Elsewhere
// ReSharper disable SuggestVarOrType_SimpleTypes

namespace DadsEnergyReporter.Remote.PowerGuide.Service
{
    public class PowerGuideClientTest : IDisposable
    {
//        private readonly HttpClient httpClient;
//        private readonly CookieContainer cookieContainer;
//        private readonly FakeHttpMessageHandler httpMessageHandler;
        private readonly PowerGuideClientImpl client;
        private readonly ApiClient apiClient;
        private ContentHandlers contentHandlers;

        public PowerGuideClientTest()
        {
            JsonSerializerConfigurer.ConfigureDefault();
//            httpMessageHandler = A.Fake<FakeHttpMessageHandler>();
//            httpClient = new HttpClient(httpMessageHandler);
//            cookieContainer = A.Fake<CookieContainer>();
            apiClient = A.Fake<ApiClient>();
            contentHandlers = A.Fake<ContentHandlers>();
            A.CallTo(() => apiClient.ContentHandlers).Returns(contentHandlers);

            client = new PowerGuideClientImpl(apiClient);
        }

        public void Dispose()
        {
//            httpMessageHandler.Dispose();
//            httpClient.Dispose();
        }
        /* FIXME
        [Fact]
        public async void Measurements_FetchInstallationId()
        {
            var response = A.Fake<HttpResponseMessage>();
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
            var request = A.CallTo(() => httpMessageHandler.SendAsync(A<HttpRequestMessage>.That.Matches(message =>
                message.RequestUri == new Uri("https://mysolarcity.com:443/solarcity-api/powerguide/v1.0/installations")
                && message.Method == HttpMethod.Get)));
            request.Returns(response);

            A.CallTo(() => client.ContentHandlers.ReadContentJsonAs<InstallationsResponse>(response))
                .Returns(installationsResponse);

            Guid actual = await client.Installations.FetchInstallationId();

            actual.Should().Be(new Guid("80dfa10d-0e02-4993-aa9a-bd241b2dc7f9"));

            request.MustHaveHappened();
        }*/

        /* FIXME
        [Fact]
        public async void Measurements_FetchMeasurements()
        {
            var response = A.Fake<HttpResponseMessage>();
            using (StreamReader fileStream = File.OpenText(@"Data/measurements.json"))
            {
                var jsonSerializer = new JsonSerializer();
                jsonSerializer.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                var measurementsResponse = jsonSerializer.Deserialize<MeasurementsResponse>(new JsonTextReader(fileStream));

                var request = A.CallTo(() => httpMessageHandler.SendAsync(A<HttpRequestMessage>.That.Matches(message =>
                    message.RequestUri == new Uri("https://mysolarcity.com:443/solarcity-api/powerguide/v1.0/measurements/80dfa10d-0e02-4993-aa9a-bd241b2dc7f9?StartTime=2017-07-18T00:00:00&EndTime=2017-08-17T00:00:00&Period=Day")
                    && message.Method == HttpMethod.Get)));
                request.Returns(response);

                A.CallTo(() => client.ContentHandlers.ReadContentJsonAs<MeasurementsResponse>(response))
                    .Returns(measurementsResponse);

                DateTimeZone zone = DateTimeZoneProviders.Tzdb["America/New_York"];
                MeasurementsResponse actual = await client.Measurements.FetchMeasurements(new Guid("80dfa10d-0e02-4993-aa9a-bd241b2dc7f9"),
                    new LocalDate(2017, 07, 18).AtStartOfDayInZone(zone),
                    new LocalDate(2017, 08, 17).AtStartOfDayInZone(zone));

                actual.TotalEnergyInIntervalkWh.Should().Be(1361.32, "total");
                actual.Measurements.Count.Should().Be(31);
                var firstMeasurement = actual.Measurements[0];
                firstMeasurement.CumulativekWh.Should().Be(5979.32, "CumulativekWh");
                firstMeasurement.DataStatus.Should().Be(DataStatus.Validated);
                firstMeasurement.EnergyInIntervalkWh.Should().Be(47.38, "EnergyInIntervalkWh");
                LocalDateTime expectedTimestamp = LocalDateTimePattern.GeneralIso.Parse("2017-07-17T00:00:00").Value;
                firstMeasurement.Timestamp.Should().Be(expectedTimestamp, "Timestamp");
            }
        }*/
    }
}
