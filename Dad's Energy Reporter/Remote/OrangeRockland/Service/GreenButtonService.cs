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
        Task<GreenButtonData> FetchGreenButtonData();
    }

    public struct GreenButtonData
    {
        public MeterReading[] meterReadings;

        public struct MeterReading
        {
            public Interval billingInterval;
            public int costCents;
            public int energyConsumedKWh;
        }
    }

    [Component]
    internal class GreenButtonServiceImpl : GreenButtonService
    {
        private const string NS = "http://naesb.org/espi";

        private readonly OrangeRocklandClient client;

        public GreenButtonServiceImpl(OrangeRocklandClient client)
        {
            this.client = client;
        }

        public async Task<GreenButtonData> FetchGreenButtonData()
        {
            XDocument doc = await client.GreenButtonClient.FetchGreenButtonData();
            IEnumerable<XElement> intervalReadings = doc.Descendants(XName.Get("IntervalReading", NS));

            return new GreenButtonData
            {
                meterReadings = intervalReadings.Select(element =>
                {
                    Instant start =
                        Instant.FromUnixTimeSeconds(
                            long.Parse(element.Descendants(XName.Get("start", NS)).First().Value));
                    Instant end = start.Plus(Duration.FromSeconds(long.Parse(element
                        .Descendants(XName.Get("duration", NS))
                        .First().Value)));
                    return new GreenButtonData.MeterReading
                    {
                        energyConsumedKWh =
                            int.Parse(element.Element(XName.Get("value", NS))?.Value ??
                                      throw new OrangeRocklandException("IntervalReading has no value child element")),
                        billingInterval = new Interval(start, end),
                        costCents = int.Parse(element.Element(XName.Get("cost", NS))?.Value ??
                                              throw new OrangeRocklandException(
                                                  "IntervalReading has no cost child element")) / 1000
                    };
                }).ToArray()
            };
        }
    }
}