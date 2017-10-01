using System;
using System.Threading.Tasks;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Remote.PowerGuide.Client;
using NodaTime;
using Measurement = DadsEnergyReporter.Data.Measurement;

namespace DadsEnergyReporter.Remote.PowerGuide.Service
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
        private readonly DateTimeZone reportTimeZone;

        public MeasurementServiceImpl(PowerGuideClient client, InstallationService installationService, DateTimeZone reportTimeZone)
        {
            this.client = client;
            this.installationService = installationService;
            this.reportTimeZone = reportTimeZone;
        }

        public async Task<Measurement> Measure(DateInterval billingInterval)
        {
            Guid installationId = await installationService.FetchInstallationId();

            MeasurementsResponse measurements = await client.Measurements.FetchMeasurements(installationId, billingInterval.Start.AtStartOfDayInZone(reportTimeZone),
                billingInterval.End.AtStartOfDayInZone(reportTimeZone));
            return new Measurement
            {
                GeneratedKilowattHours = measurements.TotalEnergyInIntervalkWh
            };
        }
    }
}