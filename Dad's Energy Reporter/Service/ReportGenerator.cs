using System.Linq;
using System.Threading.Tasks;
using DadsEnergyReporter.Data;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Remote.OrangeRockland.Service;
using DadsEnergyReporter.Remote.Solar.Tesla.Service;
using NLog;
using NodaTime;

namespace DadsEnergyReporter.Service {

    public interface ReportGenerator {

        Task<SolarAndUtilityReport> generateReport();
        Task<SolarReport> generateSolarReport(DateInterval billingInterval);

    }

    [Component]
    public class ReportGeneratorImpl: ReportGenerator {

        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        private readonly OwnerApiService ownerApiService;
        private readonly OrangeRocklandService orangeRocklandService;

        public ReportGeneratorImpl(OwnerApiService ownerApiService, OrangeRocklandService orangeRocklandService) {
            this.ownerApiService = ownerApiService;
            this.orangeRocklandService = orangeRocklandService;
        }

        public async Task<SolarAndUtilityReport> generateReport() {
            LOGGER.Info("Downloading Green Button Data from Orange & Rockland");
            GreenButtonData greenButtonData = await orangeRocklandService.greenButton.fetchGreenButtonData();
            GreenButtonData.MeterReading mostRecentOrangeRocklandBill = greenButtonData.meterReadings.Last();
            LOGGER.Info("Downloading bill from Orange & Rockland");
            int energyPurchasedOrSold = await orangeRocklandService.billDocuments.fetchEnergyPurchasedOrSoldKWh(
                mostRecentOrangeRocklandBill
                    .billingInterval.End);
            LOGGER.Debug("Paid ${0} to use {1} kWh of electricity between {2} and {3}",
                mostRecentOrangeRocklandBill.costCents / 100,
                energyPurchasedOrSold, mostRecentOrangeRocklandBill.billingInterval.Start,
                mostRecentOrangeRocklandBill.billingInterval.End);

            SolarReport solarReport = await generateSolarReport(mostRecentOrangeRocklandBill.billingInterval);

            return new SolarAndUtilityReport(solarReport.billingInterval, solarReport.powerGenerated,
                energyPurchasedOrSold, mostRecentOrangeRocklandBill.costCents);
        }

        public async Task<SolarReport> generateSolarReport(DateInterval billingInterval) {
            LOGGER.Info("Downloading Owner API solar panel report from Tesla");
            Measurement measurement = await ownerApiService.measurement.measure(billingInterval);
            LOGGER.Debug("Generated {0} kWh of electricity between {1} and {2}", measurement.generatedKilowattHours,
                billingInterval.Start, billingInterval.End);

            return new SolarReport(billingInterval, measurement.generatedKilowattHours);
        }

    }

}