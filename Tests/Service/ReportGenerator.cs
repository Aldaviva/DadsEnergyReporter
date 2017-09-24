using FluentAssertions;
using NodaTime;
using Xunit;

namespace DadsEnergyReporter.Service
{
    public class ReportGeneratorTest
    {
        [Fact]
        public void CalculateMostRecentBillingEndDate()
        {
            var fakeNow = new LocalDate(2017, 9, 9);
            var expected = new LocalDate(2017, 8, 17);
            LocalDate actual = ReportGeneratorImpl.CalculateMostRecentBillingEndDate(fakeNow);
            
            actual.Should().Be(expected);
            fakeNow.Should().Be(new LocalDate(2017, 9, 9), "CalculateMostRecentBillingEndDate should not have mutated its argument");
        }

        [Fact]
        public void CalculateBillingInterval()
        {
            var lastBillingDay = new LocalDate(2017, 08, 17);
            DateInterval actual = ReportGeneratorImpl.CalculateBillingInterval(lastBillingDay);

            actual.Start.Should().Be(new LocalDate(2017, 07, 18), "start");
            actual.End.Should().Be(lastBillingDay, "end");
        }
    }
}
