using DadsEnergyReporter.Data;
using DadsEnergyReporter.Remote.OrangeRockland.Service;
using DadsEnergyReporter.Remote.PowerGuide.Service;
using FakeItEasy;
using FluentAssertions;
using NodaTime;
using Xunit;

namespace DadsEnergyReporter.Service
{
    public class ReportGeneratorTest
    {
        private readonly ReportGeneratorImpl reportGenerator;
        private readonly PowerGuideService powerGuideService = A.Fake<PowerGuideService>();
        private readonly OrangeRocklandService orangeRocklandService = A.Fake<OrangeRocklandService>();
        private readonly MeasurementService measurementService = A.Fake<MeasurementService>();
        private readonly GreenButtonService greenButtonService = A.Fake<GreenButtonService>();

        public ReportGeneratorTest()
        {
            reportGenerator = new ReportGeneratorImpl(powerGuideService, orangeRocklandService);
            A.CallTo(() => orangeRocklandService.GreenButton).Returns(greenButtonService);
            A.CallTo(() => powerGuideService.Measurement).Returns(measurementService);
        }

        [Fact]
        public async void GenerateReport()
        {
            var greenButtonData = new GreenButtonData
            {
                MeterReadings = new[]
                {
                    new GreenButtonData.MeterReading
                    {
                        CostCents = 0,
                        EnergyConsumedKWh = 0,
                        BillingInterval = new DateInterval(new LocalDate(2017, 6, 15), new LocalDate(2017, 7, 17))
                    },
                    new GreenButtonData.MeterReading
                    {
                        CostCents = 100,
                        EnergyConsumedKWh = 200,
                        BillingInterval = new DateInterval(new LocalDate(2017, 7, 17), new LocalDate(2017, 8, 16))
                    }
                }
            };
            A.CallTo(() => orangeRocklandService.GreenButton.FetchGreenButtonData()).Returns(greenButtonData);

            var measurement = new Measurement
            {
                GeneratedKilowattHours = 3000
            };
            A.CallTo(() => powerGuideService.Measurement.Measure(A<DateInterval>._)).Returns(measurement);

            Report report = await reportGenerator.GenerateReport();

            report.Subject.Should().Be("monthly kwh report");
            report.Body.Should().Be("you generated 3000 kWh between Monday, July 17, 2017 and Wednesday, August 16, 2017.");
            report.BillingDate.Should().Be(new LocalDate(2017, 8, 16));

            A.CallTo(() => orangeRocklandService.GreenButton.FetchGreenButtonData()).MustHaveHappened();
            A.CallTo(() =>
                    powerGuideService.Measurement.Measure(new DateInterval(new LocalDate(2017, 7, 17), new LocalDate(2017, 8, 16))))
                .MustHaveHappened();
        }
    }
}