using System;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Remote.Common;
using NodaTime;
using NodaTime.Text;

namespace DadsEnergyReporter.Remote.PowerGuide.Client
{
    public interface PowerGuideClient
    {
        MeasurementClient Measurements { get; }
        PowerGuideAuthenticationClient Authentication { get; }
        InstallationClient Installations { get; }
    }

    [Component]
    internal class PowerGuideClientImpl : PowerGuideClient
    {
        internal readonly ApiClient ApiClient;

        public MeasurementClient Measurements { get; }
        public InstallationClient Installations { get; }
        public PowerGuideAuthenticationClient Authentication { get; }

        private static readonly ZonedDateTimePattern ISO8601_NO_ZONE_NO_MILLIS_PATTERN =
            ZonedDateTimePattern.CreateWithCurrentCulture("uuuu-MM-ddTHH:mm:ss", null);

        internal static UriBuilder ApiRoot => new UriBuilder()
            .UseHttps(true)
            .WithHost("mysolarcity.com")
            .WithPathSegment("solarcity-api")
            .WithPathSegment("powerguide")
            .WithPathSegment("v1.0");

        public PowerGuideClientImpl(ApiClient apiClient)
        {
            ApiClient = apiClient;
            Measurements = new MeasurementClientImpl(this);
            Installations = new InstallationClientImpl(this);
            Authentication = new PowerGuideAuthenticationClientImpl(this);
        }

        internal static string FormatDate(ZonedDateTime date)
        {
            return ISO8601_NO_ZONE_NO_MILLIS_PATTERN.Format(date);
        }
    }
}