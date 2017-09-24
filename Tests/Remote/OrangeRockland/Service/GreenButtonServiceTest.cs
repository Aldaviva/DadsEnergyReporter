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
            greenButtonService = new GreenButtonServiceImpl(oruClient);
            A.CallTo(() => oruClient.GreenButtonClient).Returns(greenButtonClient);
        }

        [Fact]
        public async void FetchGreenButtonData()
        {
            XDocument doc = XDocument.Load(@"Data/GreenButton_Electricity.xml");
            A.CallTo(() => greenButtonClient.FetchGreenButtonData()).Returns(doc);

            GreenButtonData greenButtonData = await greenButtonService.FetchGreenButtonData();

            greenButtonData.meterReadings.Length.Should().Be(13, "number of ns:IntervalBlocks");
            GreenButtonData.MeterReading firstReading = greenButtonData.meterReadings[0];
            firstReading.energyConsumedKWh.Should().Be(0);
            firstReading.costCents.Should().Be(2048);
            firstReading.billingInterval.Start.Should()
                .Be(new LocalDateTime(2017, 7, 17, 0, 0).InZoneStrictly(ZONE).ToInstant());
            firstReading.billingInterval.Duration.TotalSeconds.Should().Be(2592000, "duration (sec)");
        }
    }
}