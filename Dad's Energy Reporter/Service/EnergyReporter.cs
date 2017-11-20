using System.Reflection;
using System.Threading.Tasks;
using DadsEnergyReporter.Data;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Properties;
using DadsEnergyReporter.Remote.OrangeRockland.Service;
using DadsEnergyReporter.Remote.PowerGuide.Service;
using NLog;
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
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        private readonly ReportGenerator reportGenerator;
        private readonly EmailSender emailSender;
        private readonly PowerGuideService powerGuideService;
        private readonly OrangeRocklandService orangeRocklandService;
        private readonly DateTimeZone reportTimeZone;
        private readonly Settings settings;

        public EnergyReporterImpl(ReportGenerator reportGenerator, EmailSender emailSender, PowerGuideService powerGuideService,
            OrangeRocklandService orangeRocklandService, DateTimeZone reportTimeZone, Settings settings)
        {
            this.reportGenerator = reportGenerator;
            this.emailSender = emailSender;
            this.powerGuideService = powerGuideService;
            this.orangeRocklandService = orangeRocklandService;
            this.reportTimeZone = reportTimeZone;
            this.settings = settings;
        }

        public async Task Start()
        {
            LOGGER.Info("Dad's Energy Reporter {0}", Assembly.GetExecutingAssembly().GetName().Version);

            LOGGER.Debug("Validating settings");
            ValidateSettings(settings);

            Instant mostRecentReportBillingDate = Instant.FromUnixTimeMilliseconds(settings.mostRecentReportBillingDate);

            if (HaveSentReportTooRecently(mostRecentReportBillingDate))
            {
                LOGGER.Info(
                    "Report was already created and sent for billing cycle ending on {0}, which is too recent. Not checking again now.",
                    mostRecentReportBillingDate.InZone(DateTimeZoneProviders.Tzdb.GetSystemDefault()).Date);
                return;
            }

            try
            {
                LOGGER.Info("Logging in");
                await LogIn();

                Report report = await reportGenerator.GenerateReport();

                if (HaveAlreadySentReport(mostRecentReportBillingDate, report))
                {
                    LOGGER.Info("Report has already been sent for billing cycle ending on {0}, not sending again.",
                        report.BillingDate);
                    return;
                }

                LOGGER.Info("Sending email report");
                await emailSender.SendEmail(report, settings.reportRecipientEmails);
                settings.mostRecentReportBillingDate =
                    report.BillingDate.AtStartOfDayInZone(reportTimeZone).ToInstant().ToUnixTimeMilliseconds();
                settings.Save();
            }
            finally
            {
                LOGGER.Info("Logging out");
                Task.WaitAll(
                    powerGuideService.Authentication.LogOut(),
                    orangeRocklandService.Authentication.LogOut());
                LOGGER.Info("Done");
            }
        }

        internal Task LogIn()
        {
            orangeRocklandService.Authentication.Username = settings.orangeRocklandUsername;
            orangeRocklandService.Authentication.Password = settings.orangeRocklandPassword;
            powerGuideService.Authentication.Username = settings.solarCityUsername;
            powerGuideService.Authentication.Password = settings.solarCityPassword;

            return Task.WhenAll(
                orangeRocklandService.Authentication.GetAuthToken(),
                powerGuideService.Authentication.GetAuthToken());
        }

        internal bool HaveSentReportTooRecently(Instant mostRecentReportBillingDate)
        {
            double daysSinceLastReportGenerated =
                new Interval(mostRecentReportBillingDate, SystemClock.Instance.GetCurrentInstant()).Duration.TotalDays;
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
                LOGGER.Error($"Invalid setting: {e.SettingsKey} = {e.InvalidValue}");
                LOGGER.Error(e);
                throw;
            }
        }
    }
}