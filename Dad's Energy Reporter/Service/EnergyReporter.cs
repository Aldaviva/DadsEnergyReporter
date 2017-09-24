using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using DadsEnergyReporter.Data;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Properties;
using DadsEnergyReporter.Remote.OrangeRockland.Service;
using DadsEnergyReporter.Remote.PowerGuide.Service;
using NodaTime;
using NodaTime.Extensions;

namespace DadsEnergyReporter.Service
{
    public interface EnergyReporter
    {
        Task Start();
    }

    [Component]
    internal class EnergyReporterImpl : EnergyReporter
    {
        private readonly ReportGenerator reportGenerator;
        private readonly EmailSender emailSender;
        private readonly PowerGuideAuthenticationService powerGuideAuthenticationService;
        private readonly OrangeRocklandAuthenticationService orangeRocklandAuthenticationService;

        public EnergyReporterImpl(ReportGenerator reportGenerator, EmailSender emailSender,
            PowerGuideAuthenticationService powerGuideAuthenticationService)
        {
            JsonSerializerConfigurer.ConfigureDefault();
            this.reportGenerator = reportGenerator;
            this.emailSender = emailSender;
            this.powerGuideAuthenticationService = powerGuideAuthenticationService;
        }

        public async Task Start()
        {
            Settings settings = Settings.Default;
            ValidateSettings(settings);

            if (HaveSentReportTooRecently(settings.mostRecentReportBillingDate))
            {
                Console.WriteLine("Report was already sent recently, not checking again now.");
                return;
            }

            try
            {
                orangeRocklandAuthenticationService.Username = settings.orangeRocklandUsername;
                orangeRocklandAuthenticationService.Password = settings.orangeRocklandPassword;
                powerGuideAuthenticationService.Username = settings.solarCityUsername;
                powerGuideAuthenticationService.Password = settings.solarCityPassword;

                Task.WaitAll(
                    orangeRocklandAuthenticationService.GetAuthToken(),
                    powerGuideAuthenticationService.GetAuthToken());

                Report report = await reportGenerator.GenerateReport();

                if (HaveSentReportTooRecently(settings.mostRecentReportBillingDate, report))
                {
                    Console.WriteLine("Report has already been sent for billing cycle ending on " +
                                      $"{report.BillingDate}, not sending again.");
                    return;
                }

                await emailSender.SendEmail(report, settings.reportRecipientEmails);
                settings.mostRecentReportBillingDate = report.BillingDate.ToDateTimeUnspecified();
                settings.Save();
            }
            finally
            {
                Task.WaitAll(
                    powerGuideAuthenticationService.LogOut(),
                    orangeRocklandAuthenticationService.LogOut());
            }
        }

        internal static bool HaveSentReportTooRecently(DateTime mostRecentReportBillingDate)
        {
            double daysSinceLastReportGenerated =
                new Interval(mostRecentReportBillingDate.ToInstant(), new Instant()).Duration.TotalDays;
            return daysSinceLastReportGenerated < 28;
        }

        internal static bool HaveSentReportTooRecently(DateTime mostRecentReportBillingDate, Report report)
        {
            return report.BillingDate <= LocalDate.FromDateTime(mostRecentReportBillingDate);
        }

        private static void ValidateSettings(Settings settings)
        {
            if (settings.orangeRocklandUsername.Length == 0)
            {
                throw new InvalidCredentialException("Missing Orange & Rockland username setting.");
            }

            if (settings.orangeRocklandPassword.Length == 0)
            {
                throw new InvalidCredentialException("Missing Orange & Rockland password setting.");
            }

            if (settings.solarCityUsername.Length == 0)
            {
                throw new InvalidCredentialException("Missing SolarCity username setting.");
            }

            if (settings.solarCityPassword.Length == 0)
            {
                throw new InvalidCredentialException("Missing SolarCity password setting.");
            }

            if (settings.reportRecipientEmails.Count == 0)
            {
                throw new InvalidCredentialException("Missing list of report email recipients.");
            }
        }
    }
}