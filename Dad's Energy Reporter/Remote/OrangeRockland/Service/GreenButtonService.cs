using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Remote.OrangeRockland.Client;
using NodaTime;

namespace DadsEnergyReporter.Remote.OrangeRockland.Service
{
    public interface GreenButtonService
    {
        /// <summary>
        /// Download the ESPI Green Button data from Orange &amp; Rockland.
        /// <para>It includes the 13 most recent months of electricity usage, in ascending date order.</para>
        /// </summary>
        /// <remarks>See https://www.naesb.org/ESPI_Standards.asp </remarks>
        Task<GreenButtonData> FetchGreenButtonData();
    }

    public struct GreenButtonData
    {
        public MeterReading[] MeterReadings;

        public struct MeterReading
        {
            public DateInterval BillingInterval;
            public int CostCents;
            public int EnergyConsumedKWh;
        }
    }

    [Component]
    internal class GreenButtonServiceImpl : GreenButtonService
    {
        private const string NS = "http://naesb.org/espi";

        private readonly OrangeRocklandClient client;
        private readonly DateTimeZone zone;

        public GreenButtonServiceImpl(OrangeRocklandClient client, DateTimeZone zone)
        {
            this.client = client;
            this.zone = zone;
        }

        public async Task<GreenButtonData> FetchGreenButtonData()
        {
            XDocument doc = await client.GreenButtonClient.FetchGreenButtonData();
            IEnumerable<XElement> intervalReadings = doc.Descendants(XName.Get("IntervalReading", NS));

            return new GreenButtonData
            {
                MeterReadings = intervalReadings
                    .Select(element =>
                    {
                        Instant start =
                            Instant.FromUnixTimeSeconds(long.Parse(element.Descendants(XName.Get("start", NS)).First().Value));
                        Instant end = start.Plus(
                            Duration.FromSeconds(long.Parse(element.Descendants(XName.Get("duration", NS)).First().Value)));

                        return new GreenButtonData.MeterReading
                        {
                            EnergyConsumedKWh =
                                int.Parse(element.Element(XName.Get("value", NS))?.Value ??
                                          throw new OrangeRocklandException("IntervalReading has no value child element")),
                            BillingInterval = new DateInterval(start.InZone(zone).Date, end.InZone(zone).Date),
                            CostCents = int.Parse(element.Element(XName.Get("cost", NS))?.Value ??
                                                  throw new OrangeRocklandException("IntervalReading has no cost child element")) /
                                        1000
                        };
                    })
                    .OrderBy(reading => reading.BillingInterval.End)
                    .ToArray()
            };
        }
    }
}