using System;
using System.Collections.Generic;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Remote.PowerGuide.Client;
using FakeItEasy;
using FluentAssertions;
using NodaTime;
using Xunit;
using Measurement = DadsEnergyReporter.Data.Measurement;

namespace DadsEnergyReporter.Remote.PowerGuide.Service
{
    public class MeasurementServiceTest
    {
        private readonly MeasurementServiceImpl measurementService;
        private readonly PowerGuideClient powerGuideClient = A.Fake<PowerGuideClient>();
        private readonly InstallationService installationService = A.Fake<InstallationService>();
        private readonly MeasurementClient measurementClient = A.Fake<MeasurementClient>();

        public MeasurementServiceTest()
        {
            DateTimeZone zone = DateTimeZoneProviders.Tzdb["America/New_York"];
            measurementService = new MeasurementServiceImpl(powerGuideClient, installationService, zone);

            A.CallTo(() => powerGuideClient.Measurements).Returns(measurementClient);
        }

        [Fact]
        public async void Measure()
        {
            var installationGuid = new Guid("45c0c40e-a9ad-11e7-abc4-cec278b6b50a");
            A.CallTo(() => installationService.FetchInstallationId()).Returns(installationGuid);

            var measurementResponse = new MeasurementsResponse()
            {
                Measurements = new List<Data.Marshal.Measurement>
                {
                    new Data.Marshal.Measurement()
                    {
                        CumulativekWh = 1,
                        DataStatus = DataStatus.Validated,
                        EnergyInIntervalkWh = 100,
                        Timestamp = new LocalDateTime(2017, 08, 16, 0, 0)
                    }
                },
                TotalEnergyInIntervalkWh = 100
            };
            A.CallTo(() => measurementClient.FetchMeasurements(A<Guid>._, A<ZonedDateTime>._, A<ZonedDateTime>._))
                .Returns(measurementResponse);

            var interval = new DateInterval(new LocalDate(2017, 7, 17), new LocalDate(2017, 8, 16));
            Measurement measurement = await measurementService.Measure(interval);

            measurement.GeneratedKilowattHours.Should().Be(100);
        }
    }
}