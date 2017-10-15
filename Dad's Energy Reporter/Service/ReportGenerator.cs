using System;
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
            Console.WriteLine("Downloading Green Button Data from Orange & Rockland");
            GreenButtonData greenButtonData = await orangeRocklandService.GreenButton.FetchGreenButtonData();
            GreenButtonData.MeterReading mostRecentOrangeRocklandBill = greenButtonData.MeterReadings.Last();
            DateInterval billingInterval = mostRecentOrangeRocklandBill.BillingInterval;
            
            Console.WriteLine("Downloading PowerGuide solar panel report from SolarCity");
            Measurement measurement = await powerGuideService.Measurement.Measure(billingInterval);

            return new Report(measurement.GeneratedKilowattHours, billingInterval);
        }
    }
}