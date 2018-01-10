using System;
using System.Collections.Generic;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Remote.PowerGuide.Client;
using FakeItEasy;
using FluentAssertions;
using NodaTime;
using Xunit;

namespace DadsEnergyReporter.Remote.PowerGuide.Service
{
    public class MeasurementServiceTest
    {
        private readonly MeasurementServiceImpl measurementService;
        private readonly PowerGuideClient powerGuideClient = A.Fake<PowerGuideClient>();
        private readonly InstallationService installationService = A.Fake<InstallationService>();
        private readonly MeasurementClient measurementClient = A.Fake<MeasurementClient>();
        private readonly DateTimeZone zone;

        public MeasurementServiceTest()
        {
            zone = DateTimeZoneProviders.Tzdb["America/New_York"];
            measurementService = new MeasurementServiceImpl(powerGuideClient, installationService, zone);

            A.CallTo(() => powerGuideClient.Measurements).Returns(measurementClient);
        }

        [Fact]
        public async void Measure()
        {
            var installationGuid = new Guid("45c0c40e-a9ad-11e7-abc4-cec278b6b50a");
            A.CallTo(() => installationService.FetchInstallationId()).Returns(installationGuid);

            var measurementResponse = new MeasurementsResponse
            {
                Measurements = new List<Measurement>
                {
                    new Measurement
                    {
                        CumulativekWh = 0,
                        DataStatus = DataStatus.Validated,
                        Timestamp = new LocalDateTime(2017, 07, 17, 0, 0)
                    },
                    new Measurement
                    {
                        CumulativekWh = 100,
                        DataStatus = DataStatus.Validated,
                        Timestamp = new LocalDateTime(2017, 08, 16, 0, 0)
                    }
                },
                TotalEnergyInIntervalkWh = 100
            };
            A.CallTo(() => measurementClient.FetchMeasurements(A<Guid>._, A<ZonedDateTime>._, A<ZonedDateTime>._))
                .Returns(measurementResponse);

            var interval = new DateInterval(new LocalDate(2017, 7, 17), new LocalDate(2017, 8, 16));
            Data.Measurement measurement = await measurementService.Measure(interval);

            measurement.GeneratedKilowattHours.Should().Be(100);

            A.CallTo(() => measurementClient.FetchMeasurements(A<Guid>._,
                new LocalDateTime(2017, 07, 17, 0, 0).InZoneStrictly(zone),
                new LocalDateTime(2017, 08, 16, 0, 0).InZoneStrictly(zone))).MustHaveHappened();
        }
    }
}