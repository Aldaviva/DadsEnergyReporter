using NodaTime;

namespace DadsEnergyReporter.Data
{
    public class SolarReport
    {
        public DateInterval BillingInterval { get; }
        public double PowerGenerated { get; }

        public SolarReport(DateInterval billingInterval, double powerGenerated)
        {
            BillingInterval = billingInterval;
            PowerGenerated = powerGenerated;
        }
    }

    public class SolarAndUtilityReport : SolarReport
    {
        public int PowerBoughtOrSold { get; }
        public int PowerCostCents { get; }

        public LocalDate BillingDate => BillingInterval.End;

        public SolarAndUtilityReport(DateInterval billingInterval, double powerGenerated, int powerBoughtOrSold, int powerCostCents) : base(billingInterval, powerGenerated)
        {
            PowerBoughtOrSold = powerBoughtOrSold;
            PowerCostCents = powerCostCents;
        }
    }
}