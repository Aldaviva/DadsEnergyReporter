using System;
using System.Linq;
using System.Threading.Tasks;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Remote.Solar.PowerGuide.Client;
using NodaTime;
using Measurement = DadsEnergyReporter.Data.Measurement;

namespace DadsEnergyReporter.Remote.Solar.PowerGuide.Service {

    

    [Component]
    public class MeasurementServiceImpl: MeasurementService {

        private readonly PowerGuideClient client;
        private readonly InstallationService installationService;
        private readonly DateTimeZone reportTimeZone;

        public MeasurementServiceImpl(PowerGuideClient client, InstallationService installationService, DateTimeZone reportTimeZone) {
            this.client = client;
            this.installationService = installationService;
            this.reportTimeZone = reportTimeZone;
        }

        public async Task<Measurement> measure(DateInterval billingInterval) {
            Guid installationId = await installationService.fetchInstallationId();

            MeasurementsResponse measurements = await client.measurements.fetchMeasurements(installationId,
                billingInterval.Start.AtStartOfDayInZone(reportTimeZone),
                billingInterval.End.AtStartOfDayInZone(reportTimeZone));

            double cumulativeGeneratedBeforeInterval = measurements.measurements.Min(measurement => measurement.cumulativekWh);
            double cumulativeGeneratedAfterInterval = measurements.measurements.Max(measurement => measurement.cumulativekWh);

            return new Measurement {
                generatedKilowattHours = cumulativeGeneratedAfterInterval - cumulativeGeneratedBeforeInterval
            };
        }

    }

}