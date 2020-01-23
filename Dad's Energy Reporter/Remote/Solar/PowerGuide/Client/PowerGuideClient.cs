using System;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Remote.Common;
using NodaTime;
using NodaTime.Text;

namespace DadsEnergyReporter.Remote.Solar.PowerGuide.Client {

    public interface PowerGuideClient {

        MeasurementClient measurements { get; }
        PowerGuideAuthenticationClient authentication { get; }
        InstallationClient installations { get; }

    }

    [Component]
    internal class PowerGuideClientImpl: PowerGuideClient {

        internal readonly ApiClient apiClient;

        public MeasurementClient measurements { get; }
        public InstallationClient installations { get; }
        public PowerGuideAuthenticationClient authentication { get; }

        private static readonly ZonedDateTimePattern ISO8601_NO_ZONE_NO_MILLIS_PATTERN =
            ZonedDateTimePattern.CreateWithCurrentCulture("uuuu-MM-ddTHH:mm:ss", null);

        internal static UriBuilder apiRoot => new UriBuilder()
            .UseHttps()
            .WithHost("mysolarcity.com")
            .WithPathSegment("solarcity-api")
            .WithPathSegment("powerguide")
            .WithPathSegment("v1.0");

        public PowerGuideClientImpl(ApiClient apiClient) {
            this.apiClient = apiClient;
            measurements = new MeasurementClientImpl(this);
            installations = new InstallationClientImpl(this);
            authentication = new PowerGuideAuthenticationClientImpl(this);
        }

        internal static string formatDate(ZonedDateTime date) {
            return ISO8601_NO_ZONE_NO_MILLIS_PATTERN.Format(date);
        }

    }

}