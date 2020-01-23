using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NodaTime;

namespace DadsEnergyReporter.Data.Marshal {

    public struct EnergySiteProduct
    {

        [JsonProperty("energy_site_id")]
        public long energySiteId { get; set; }

        public Guid id { get; set; }

    }

    public class ProductsResponse
    {

        public IList<EnergySiteProduct> response { get; set; }

    }

    public class HistoricalCalendarData
    {

        public HistoricalCalendarDataResponse response { get; set; }

    }

    public class HistoricalCalendarDataResponse
    {

        [JsonProperty("time_series")]
        public IEnumerable<EnergyTimeSeriesEntry> timeSeries { get; set; }

    }

    public class EnergyTimeSeriesEntry
    {

        public ZonedDateTime timestamp { get; set; }

        /// <summary>
        /// watt-hours
        /// </summary>
        [JsonProperty("solar_energy_exported")]
        public double solarEnergyExported { get; set; }

    }

}