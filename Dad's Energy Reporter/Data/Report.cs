using NodaTime;

namespace DadsEnergyReporter.Data
{
    public class Report
    {
        public DateInterval BillingInterval { get; }
        public double PowerGenerated { get; }
        public int PowerBoughtOrSold { get; }
        public int PowerCostCents { get; }

        public LocalDate BillingDate => BillingInterval.End;

        public Report(DateInterval billingInterval, double powerGenerated, int powerBoughtOrSold, int powerCostCents)
        {
            PowerGenerated = powerGenerated;
            PowerBoughtOrSold = powerBoughtOrSold;
            PowerCostCents = powerCostCents;
            BillingInterval = billingInterval;
        }
    }
}