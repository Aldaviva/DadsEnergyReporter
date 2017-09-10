using System;
using System.Threading.Tasks;
using NodaTime;
using PowerGuideReporter.Data.Marshal;
using PowerGuideReporter.Injection;

namespace PowerGuideReporter.Service.Remote.Measurement
{
    public interface MeasurementService
    {
        Task<Measurement> Measure(DateInterval billingInterval);
    }

    
    [Component]
    public class MeasurementServiceImpl : MeasurementService
    {
        private readonly PowerGuideClient client;

        private static readonly DateTimeZone REPORT_TIME_ZONE = DateTimeZoneProviders.Tzdb["America/New_York"];

        public MeasurementServiceImpl(PowerGuideClient client)
        {
            this.client = client;
        }

        public async Task<Measurement> Measure(DateInterval billingInterval)
        {
            Guid installationId = await FetchInstallationId();

            MeasurementsResponse measurementsResponse = await client.Measurements.FetchMeasurements(installationId, billingInterval.Start.AtStartOfDayInZone(REPORT_TIME_ZONE), billingInterval.End.AtStartOfDayInZone(REPORT_TIME_ZONE));
            return new Measurement
            {
                GeneratedKilowattHours = measurementsResponse.TotalEnergyInIntervalkWh
            };
        }

        public async Task<Guid> FetchInstallationId()
        {
            return await client.Measurements.FetchInstallationId();
        }
    }
}