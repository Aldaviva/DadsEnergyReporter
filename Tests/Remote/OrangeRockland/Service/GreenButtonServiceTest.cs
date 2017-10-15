using System.Linq;
using System.Xml.Linq;
using DadsEnergyReporter.Remote.OrangeRockland.Client;
using FakeItEasy;
using FluentAssertions;
using NodaTime;
using Xunit;

namespace DadsEnergyReporter.Remote.OrangeRockland.Service
{
    public class GreenButtonServiceTest
    {
        private static readonly DateTimeZone ZONE = DateTimeZoneProviders.Tzdb["America/New_York"];
        private readonly GreenButtonServiceImpl greenButtonService;
        private readonly GreenButtonClient greenButtonClient = A.Fake<GreenButtonClient>();
        private readonly OrangeRocklandClient oruClient = A.Fake<OrangeRocklandClient>();

        public GreenButtonServiceTest()
        {
            greenButtonService = new GreenButtonServiceImpl(oruClient, ZONE);
            A.CallTo(() => oruClient.GreenButtonClient).Returns(greenButtonClient);
        }

        [Fact]
        public async void FetchGreenButtonData()
        {
            XDocument doc = XDocument.Load(@"Data/GreenButton_Electricity.xml");
            A.CallTo(() => greenButtonClient.FetchGreenButtonData()).Returns(doc);

            GreenButtonData greenButtonData = await greenButtonService.FetchGreenButtonData();

            greenButtonData.MeterReadings.Length.Should().Be(13, "number of ns:IntervalBlocks");
            GreenButtonData.MeterReading lastReading = greenButtonData.MeterReadings.Last();
            lastReading.EnergyConsumedKWh.Should().Be(0);
            lastReading.CostCents.Should().Be(2048);
            lastReading.BillingInterval.Start.Should().Be(new LocalDate(2017, 7, 17));
            lastReading.BillingInterval.End.Should().Be(new LocalDate(2017, 8, 16));
        }
    }
}