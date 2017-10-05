using System;
using System.Threading.Tasks;
using DadsEnergyReporter.Data;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Properties;
using DadsEnergyReporter.Remote.OrangeRockland.Service;
using DadsEnergyReporter.Remote.PowerGuide.Service;
using NodaTime;

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
        private readonly DateTimeZone reportTimeZone;

        public EnergyReporterImpl(ReportGenerator reportGenerator, EmailSender emailSender, PowerGuideAuthenticationService powerGuideAuthenticationService,
            OrangeRocklandAuthenticationService orangeRocklandAuthenticationService, DateTimeZone reportTimeZone)
        {
            JsonSerializerConfigurer.ConfigureDefault();
            this.reportGenerator = reportGenerator;
            this.emailSender = emailSender;
            this.powerGuideAuthenticationService = powerGuideAuthenticationService;
            this.orangeRocklandAuthenticationService = orangeRocklandAuthenticationService;
            this.reportTimeZone = reportTimeZone;
        }

        public async Task Start()
        {
            Settings settings = Settings.Default;
            ValidateSettings(settings);

            Instant mostRecentReportBillingDate = Instant.FromUnixTimeMilliseconds(settings.mostRecentReportBillingDate);

            if (HaveSentReportTooRecently(mostRecentReportBillingDate))
            {
                Console.WriteLine("Report was already sent recently, not checking again now.");
                return;
            }

            try
            {
                await LogIn();

                Report report = await reportGenerator.GenerateReport();

                if (HaveAlreadySentReport(mostRecentReportBillingDate, report))
                {
                    Console.WriteLine("Report has already been sent for billing cycle ending on " +
                                      $"{report.BillingDate}, not sending again.");
                    return;
                }

                await emailSender.SendEmail(report, settings.reportRecipientEmails);
                settings.mostRecentReportBillingDate = report.BillingDate.AtStartOfDayInZone(reportTimeZone).ToInstant().ToUnixTimeMilliseconds();
                settings.Save();
            }
            finally
            {
                Task.WaitAll(
                    powerGuideAuthenticationService.LogOut(),
                    orangeRocklandAuthenticationService.LogOut());
            }
        }

        internal Task LogIn()
        {
            Settings settings = Settings.Default;

            orangeRocklandAuthenticationService.Username = settings.orangeRocklandUsername;
            orangeRocklandAuthenticationService.Password = settings.orangeRocklandPassword;
            powerGuideAuthenticationService.Username = settings.solarCityUsername;
            powerGuideAuthenticationService.Password = settings.solarCityPassword;

            return Task.WhenAll(
                orangeRocklandAuthenticationService.GetAuthToken(),
                powerGuideAuthenticationService.GetAuthToken());
        }

        internal bool HaveSentReportTooRecently(Instant mostRecentReportBillingDate)
        {
            double daysSinceLastReportGenerated = new Interval(mostRecentReportBillingDate, SystemClock.Instance.GetCurrentInstant()).Duration.TotalDays;
            return daysSinceLastReportGenerated < 28;
        }

        internal bool HaveAlreadySentReport(Instant mostRecentReportBillingDate, Report report)
        {
            return !(mostRecentReportBillingDate.InZone(reportTimeZone).Date < report.BillingDate);
        }

        private static void ValidateSettings(Settings settings)
        {
            try
            {
                settings.Validate();
            }
            catch (SettingsException e)
            {
                Console.WriteLine($"Invalid setting: {e.SettingsKey} = {e.InvalidValue}");
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }
}