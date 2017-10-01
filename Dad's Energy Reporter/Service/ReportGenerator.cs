using System.Linq;
using System.Threading.Tasks;
using DadsEnergyReporter.Data;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Remote.OrangeRockland.Service;
using DadsEnergyReporter.Remote.PowerGuide.Service;
using NodaTime;

namespace DadsEnergyReporter.Service
{
    public interface ReportGenerator
    {
        Task<Report> GenerateReport();
    }

    [Component]
    public class ReportGeneratorImpl : ReportGenerator
    {
        private readonly PowerGuideService powerGuideService;
        private readonly OrangeRocklandService orangeRocklandService;

        public ReportGeneratorImpl(PowerGuideService powerGuideService, OrangeRocklandService orangeRocklandService)
        {
            this.powerGuideService = powerGuideService;
            this.orangeRocklandService = orangeRocklandService;
        }

        public async Task<Report> GenerateReport()
        {
            GreenButtonData greenButtonData = await orangeRocklandService.GreenButton.FetchGreenButtonData();
            GreenButtonData.MeterReading mostRecentOrangeRocklandBill = greenButtonData.meterReadings.Last();
            DateInterval billingInterval = mostRecentOrangeRocklandBill.billingInterval;
            Measurement measurement = await powerGuideService.Measurement.Measure(billingInterval);

            return new Report(measurement.GeneratedKilowattHours, billingInterval);
        }
    }
}