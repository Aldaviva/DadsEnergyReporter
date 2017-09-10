using Newtonsoft.Json;
using NodaTime;
using NodaTime.Serialization.JsonNet;

namespace PowerGuideReporter.Data.Marshal
{
    internal class JsonSerializerConfigurer
    {
        private static readonly IDateTimeZoneProvider DATE_TIME_ZONE_PROVIDER = DateTimeZoneProviders.Tzdb;

        private JsonSerializerConfigurer()
        {
        }

        public static void ConfigureDefault()
        {
            JsonConvert.DefaultSettings = () =>
                new JsonSerializerSettings()
                    .ConfigureForNodaTime(DATE_TIME_ZONE_PROVIDER);
        }

        public static void Configure(JsonSerializer jsonSerializer)
        {
            jsonSerializer.ConfigureForNodaTime(DATE_TIME_ZONE_PROVIDER);
        }
    }
}
