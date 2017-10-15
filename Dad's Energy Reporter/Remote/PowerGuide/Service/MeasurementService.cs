using System;
using System.Linq;
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

            //start one day earlier because we need the cumulative kWh at the end of the last billing cycle for our subtraction
            //if we didn't start one day early, we would be omitting the first day's worth of generation, since cumulative kWh is measured at the end of the day
            MeasurementsResponse measurements = await client.Measurements.FetchMeasurements(installationId,
                billingInterval.Start.Minus(Period.FromDays(1)).AtStartOfDayInZone(reportTimeZone),
                billingInterval.End.AtStartOfDayInZone(reportTimeZone));

            double cumulativeGeneratedBeforeInterval = measurements.Measurements.Min(measurement => measurement.CumulativekWh);
            double cumulativeGeneratedAfterInterval = measurements.Measurements.Max(measurement => measurement.CumulativekWh);

            return new Measurement
            {
                GeneratedKilowattHours = cumulativeGeneratedAfterInterval - cumulativeGeneratedBeforeInterval
            };
        }
    }
}