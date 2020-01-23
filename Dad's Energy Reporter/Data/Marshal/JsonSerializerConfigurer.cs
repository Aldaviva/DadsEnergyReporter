using Newtonsoft.Json;
using NodaTime;
using NodaTime.Serialization.JsonNet;

namespace DadsEnergyReporter.Data.Marshal {

    internal static class JsonSerializerConfigurer {

        private static readonly IDateTimeZoneProvider DATE_TIME_ZONE_PROVIDER = DateTimeZoneProviders.Tzdb;

        public static void configureDefault() {
            JsonConvert.DefaultSettings = () =>
                new JsonSerializerSettings()
                    .ConfigureForNodaTime(DATE_TIME_ZONE_PROVIDER);
        }

        /*public static void Configure(JsonSerializer jsonSerializer)
        {
            jsonSerializer.ConfigureForNodaTime(DATE_TIME_ZONE_PROVIDER);
        }*/

    }

}