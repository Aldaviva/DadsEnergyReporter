using System.Threading.Tasks;
using DadsEnergyReporter.Data;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Remote.PowerGuide.Service;
using NodaTime;

namespace DadsEnergyReporter.Service
{
    internal interface ReportGenerator
    {
        Task<Report> GenerateReport();
    }

    [Component]
    public class ReportGeneratorImpl : ReportGenerator
    {
        private readonly MeasurementService measurementService;

        public ReportGeneratorImpl(MeasurementService measurementService)
        {
            this.measurementService = measurementService;
        }

        public async Task<Report> GenerateReport()
        {
            LocalDate mostRecentBillingEndDate = CalculateMostRecentBillingEndDate(new LocalDate());
            DateInterval billingInterval = CalculateBillingInterval(mostRecentBillingEndDate);
            Measurement measurement = await measurementService.Measure(billingInterval);

            return new Report(measurement.GeneratedKilowattHours, billingInterval);
        }

        public static LocalDate CalculateMostRecentBillingEndDate(LocalDate now)
        {
            LocalDate billingEndDate = now.With(DateAdjusters.DayOfMonth(17));
            if (billingEndDate >= now)
            {
                billingEndDate = billingEndDate.PlusMonths(-1);
            }
            return billingEndDate;
        }

        public static DateInterval CalculateBillingInterval(LocalDate lastBillingDay)
        {
            LocalDate start = lastBillingDay.PlusMonths(-1).PlusDays(1);
            return new DateInterval(start, lastBillingDay);
        }
    }
}