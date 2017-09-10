using System;
using System.Threading.Tasks;
using NodaTime;
using PowerGuideReporter.Data.Marshal;
using PowerGuideReporter.Injection;
using PowerGuideReporter.Remote.PowerGuide.Client;
using Measurement = PowerGuideReporter.Data.Measurement;

namespace PowerGuideReporter.Remote.PowerGuide.Service
{
    public interface MeasurementService
    {
        Task<Measurement> Measure(DateInterval billingInterval);
    }
    
    [Component]
    public class MeasurementServiceImpl : MeasurementService
    {
        private readonly PowerGuideClient client;
        private readonly InstallationService installationService;

        private static readonly DateTimeZone REPORT_TIME_ZONE = DateTimeZoneProviders.Tzdb["America/New_York"];

        public MeasurementServiceImpl(PowerGuideClient client, InstallationService installationService)
        {
            this.client = client;
            this.installationService = installationService;
        }

        public async Task<Measurement> Measure(DateInterval billingInterval)
        {
            Guid installationId = await installationService.FetchInstallationId();

            MeasurementsResponse measurementsResponse = await client.Measurements.FetchMeasurements(installationId, billingInterval.Start.AtStartOfDayInZone(REPORT_TIME_ZONE), billingInterval.End.AtStartOfDayInZone(REPORT_TIME_ZONE));
            return new Measurement
            {
                GeneratedKilowattHours = measurementsResponse.TotalEnergyInIntervalkWh
            };
        }
    }
}