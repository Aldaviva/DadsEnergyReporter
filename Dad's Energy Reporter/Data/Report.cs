using NodaTime;

namespace DadsEnergyReporter.Data
{
    public class Report
    {
        public DateInterval BillingInterval { get; }
        public double PowerGenerated { get; }
        public int PowerBought { get; }
        public int PowerCostCents { get; }

        public LocalDate BillingDate => BillingInterval.End;

        public Report(DateInterval billingInterval, double powerGenerated, int powerBought, int powerCostCents)
        {
            PowerGenerated = powerGenerated;
            PowerBought = powerBought;
            PowerCostCents = powerCostCents;
            BillingInterval = billingInterval;
        }
    }
}