using NodaTime;

namespace PowerGuideReporter.Data
{
    public class Report
    {
        private readonly double _powerGenerated;
        private readonly DateInterval _billingInterval;

        public string Subject => "monthly kwh report";
        public string Body => $"you generated {_powerGenerated} kWh between {_billingInterval.Start} and {_billingInterval.End}.";

        public Report(double powerGenerated, DateInterval billingInterval)
        {
            _powerGenerated = powerGenerated;
            _billingInterval = billingInterval;
        }
    }
}