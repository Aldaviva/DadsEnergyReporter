using DadsEnergyReporter.Data;
using DadsEnergyReporter.Injection;
using MimeKit;
using NodaTime;
using NodaTime.Text;

namespace DadsEnergyReporter.Service
{
    public interface ReportFormatter
    {
        MimeMessage FormatReport(Report report);
    }

    [Component]
    internal class ReportFormatterImpl : ReportFormatter
    {
        private static readonly LocalDatePattern SHORT_DATE_PATTERN = LocalDatePattern.CreateWithCurrentCulture("M/d");

        public MimeMessage FormatReport(Report report)
        {
            return new MimeMessage
            {
                Subject = FormatSubject(report),
                Body = new BodyBuilder
                {
                    TextBody = FormatBodyPlainText(report),
                    HtmlBody = FormatBodyHtml(report)
                }.ToMessageBody()
            };
        }

        private static string FormatSubject(Report report)
        {
            return
                $"Electricity Usage Report for {ShortDate(report.BillingInterval.Start)}–{ShortDate(report.BillingInterval.End)}";
        }

        private static string FormatBodyPlainText(Report report)
        {
            return $@"Electricity generated: {report.PowerGenerated:N0} kWh
Electricity purchased: {report.PowerBought:N0} kWh for {report.PowerCostCents / 100.0:C}";
        }

        private static string FormatBodyHtml(Report report)
        {
            return $@"<b>Electricity generated:</b> {report.PowerGenerated:N0} kWh<br>
<b>Electricity purchased:</b> {report.PowerBought:N0} kWh for {report.PowerCostCents / 100.0:C}";
        }

        private static string ShortDate(LocalDate date)
        {
            return SHORT_DATE_PATTERN.Format(date);
        }
    }
}