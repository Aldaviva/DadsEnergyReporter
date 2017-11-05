using System.Linq;
using System.Threading.Tasks;
using DadsEnergyReporter.Data;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Remote.OrangeRockland.Service;
using DadsEnergyReporter.Remote.PowerGuide.Service;
using NLog;

namespace DadsEnergyReporter.Service
{
    public interface ReportGenerator
    {
        Task<Report> GenerateReport();
    }

    [Component]
    public class ReportGeneratorImpl : ReportGenerator
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        private readonly PowerGuideService powerGuideService;
        private readonly OrangeRocklandService orangeRocklandService;

        public ReportGeneratorImpl(PowerGuideService powerGuideService, OrangeRocklandService orangeRocklandService)
        {
            this.powerGuideService = powerGuideService;
            this.orangeRocklandService = orangeRocklandService;
        }

        public async Task<Report> GenerateReport()
        {
            LOGGER.Info("Downloading Green Button Data from Orange & Rockland");
            GreenButtonData greenButtonData = await orangeRocklandService.GreenButton.FetchGreenButtonData();
            GreenButtonData.MeterReading mostRecentOrangeRocklandBill = greenButtonData.MeterReadings.Last();
            LOGGER.Debug("Paid ${0} to use {1} kWh of electricity between {2} and {3}", mostRecentOrangeRocklandBill.CostCents / 100,
                mostRecentOrangeRocklandBill.EnergyConsumedKWh, mostRecentOrangeRocklandBill.BillingInterval.Start,
                mostRecentOrangeRocklandBill.BillingInterval.End);

            LOGGER.Info("Downloading PowerGuide solar panel report from SolarCity");
            Measurement measurement = await powerGuideService.Measurement.Measure(mostRecentOrangeRocklandBill.BillingInterval);
            LOGGER.Debug("Generated {0} kWh of electricity between {1} and {2}", measurement.GeneratedKilowattHours, mostRecentOrangeRocklandBill.BillingInterval.Start,
                mostRecentOrangeRocklandBill.BillingInterval.End);

            return new Report(mostRecentOrangeRocklandBill.BillingInterval, measurement.GeneratedKilowattHours,
                mostRecentOrangeRocklandBill.EnergyConsumedKWh, mostRecentOrangeRocklandBill.CostCents);
        }
    }
}