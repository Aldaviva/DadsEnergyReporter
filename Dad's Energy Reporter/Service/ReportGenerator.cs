using System.Linq;
using System.Threading.Tasks;
using DadsEnergyReporter.Data;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Remote.OrangeRockland.Service;
using DadsEnergyReporter.Remote.PowerGuide.Service;
using NLog;
using NodaTime;

namespace DadsEnergyReporter.Service
{
    public interface ReportGenerator
    {
        Task<SolarAndUtilityReport> GenerateReport();
        Task<SolarReport> GenerateSolarReport(DateInterval billingInterval);
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

        public async Task<SolarAndUtilityReport> GenerateReport()
        {
            LOGGER.Info("Downloading Green Button Data from Orange & Rockland");
            GreenButtonData greenButtonData = await orangeRocklandService.GreenButton.FetchGreenButtonData();
            GreenButtonData.MeterReading mostRecentOrangeRocklandBill = greenButtonData.MeterReadings.Last();
            LOGGER.Info("Downloading bill from Orange & Rockland");
            int energyPurchasedOrSold = await orangeRocklandService.BillDocuments.FetchEnergyPurchasedOrSoldKWh(
                mostRecentOrangeRocklandBill
                    .BillingInterval.End);
            LOGGER.Debug("Paid ${0} to use {1} kWh of electricity between {2} and {3}",
                mostRecentOrangeRocklandBill.CostCents / 100,
                energyPurchasedOrSold, mostRecentOrangeRocklandBill.BillingInterval.Start,
                mostRecentOrangeRocklandBill.BillingInterval.End);

            SolarReport solarReport = await GenerateSolarReport(mostRecentOrangeRocklandBill.BillingInterval);

            return new SolarAndUtilityReport(solarReport.BillingInterval, solarReport.PowerGenerated,
                energyPurchasedOrSold, mostRecentOrangeRocklandBill.CostCents);
        }

        public async Task<SolarReport> GenerateSolarReport(DateInterval billingInterval)
        {
            LOGGER.Info("Downloading PowerGuide solar panel report from SolarCity");
            Measurement measurement = await powerGuideService.Measurement.Measure(billingInterval);
            LOGGER.Debug("Generated {0} kWh of electricity between {1} and {2}", measurement.GeneratedKilowattHours,
                billingInterval.Start, billingInterval.End);

            return new SolarReport(billingInterval, measurement.GeneratedKilowattHours);
        }
    }
}