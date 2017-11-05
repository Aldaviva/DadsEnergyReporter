using DadsEnergyReporter.Data;
using FluentAssertions;
using MimeKit;
using NodaTime;
using Xunit;

namespace DadsEnergyReporter.Service
{
    public class ReportFormatterTest
    {
        private readonly ReportFormatterImpl reportFormatter = new ReportFormatterImpl();

        [Fact]
        public void FormatReport()
        {
            var report = new Report(new DateInterval(new LocalDate(2017, 07, 17), new LocalDate(2017, 08, 16)), 1234.56789, 9999,
                6780);
            MimeMessage actual = reportFormatter.FormatReport(report);
            actual.Subject.Should().Be("Electricity Usage Report for 7/17–8/16");
            actual.TextBody.Should().Be(@"Electricity generated: 1,235 kWh
Electricity purchased: 9,999 kWh for $67.80");
            actual.HtmlBody.Should().Be(@"<b>Electricity generated:</b> 1,235 kWh<br>
<b>Electricity purchased:</b> 9,999 kWh for $67.80");
        }

        [Fact]
        public void Zeroes()
        {
            var report = new Report(new DateInterval(new LocalDate(2017, 11, 1), new LocalDate(2017, 12, 2)), 0, 0, 2000);
            MimeMessage actual = reportFormatter.FormatReport(report);
            actual.Subject.Should().Be("Electricity Usage Report for 11/1–12/2");
            actual.TextBody.Should().Be(@"Electricity generated: 0 kWh
Electricity purchased: 0 kWh for $20.00");
            actual.HtmlBody.Should().Be(@"<b>Electricity generated:</b> 0 kWh<br>
<b>Electricity purchased:</b> 0 kWh for $20.00");
        }
    }
}