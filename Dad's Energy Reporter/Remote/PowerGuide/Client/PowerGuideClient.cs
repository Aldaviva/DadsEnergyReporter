using System;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Remote.Common;
using NodaTime;
using NodaTime.Text;

namespace DadsEnergyReporter.Remote.PowerGuide.Client
{
    public interface PowerGuideClient
    {
        Measurements Measurements { get; }
        Authentication Authentication { get; }
        Installations Installations { get; }
    }

    [Component]
    internal class PowerGuideClientImpl : PowerGuideClient
    {
        internal readonly ApiClient ApiClient;
        
        public Measurements Measurements { get; }
        public Installations Installations { get; }
        public Authentication Authentication { get; }

        private static readonly ZonedDateTimePattern ISO8601_NO_ZONE_NO_MILLIS_PATTERN = ZonedDateTimePattern.CreateWithCurrentCulture("uuuu-MM-ddTHH:mm:ss", null);
        internal static UriBuilder ApiRoot => new UriBuilder("https", "mysolarcity.com", -1, "/solarcity-api/powerguide/v1.0/");

        public PowerGuideClientImpl(ApiClient apiClient)
        {
            ApiClient = apiClient;
            Measurements = new MeasurementsImpl(this);
            Installations = new InstallationsImpl(this);
            Authentication = new AuthenticationImpl(this);
        }

        internal static string FormatDate(ZonedDateTime date)
        {
            return ISO8601_NO_ZONE_NO_MILLIS_PATTERN.Format(date);
        }
    }
}
