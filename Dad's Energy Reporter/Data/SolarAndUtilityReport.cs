using NodaTime;

namespace DadsEnergyReporter.Data {

    public class SolarReport {

        public DateInterval billingInterval { get; }
        public double powerGenerated { get; }

        public SolarReport(DateInterval billingInterval, double powerGenerated) {
            this.billingInterval = billingInterval;
            this.powerGenerated = powerGenerated;
        }

    }

    public class SolarAndUtilityReport: SolarReport {

        public int powerBoughtOrSold { get; }
        public int powerCostCents { get; }

        public LocalDate billingDate => billingInterval.End;

        public SolarAndUtilityReport(DateInterval billingInterval, double powerGenerated, int powerBoughtOrSold, int powerCostCents):
            base(billingInterval, powerGenerated) {
            this.powerBoughtOrSold = powerBoughtOrSold;
            this.powerCostCents = powerCostCents;
        }

    }

}