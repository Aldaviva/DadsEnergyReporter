using System;
using System.Reflection;
using System.Threading.Tasks;
using DadsEnergyReporter.Data;
using DadsEnergyReporter.Entry;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Remote.OrangeRockland.Service;
using DadsEnergyReporter.Remote.Solar.PowerGuide.Service;
using DadsEnergyReporter.Remote.Solar.Tesla.Service;
using NLog;
using NodaTime;

namespace DadsEnergyReporter.Service {

    public interface EnergyReporter {

        Task<int> sendSolarReport();
        Task<int> sendSolarAndUtilityReport();

    }

    [Component]
    internal class EnergyReporterImpl: EnergyReporter {

        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        private readonly ReportGenerator reportGenerator;
        private readonly EmailSender emailSender;
        private readonly OwnerApiService ownerApiService;
        private readonly OrangeRocklandService orangeRocklandService;
        private readonly DateTimeZone reportTimeZone;
        private readonly Settings settings;
        private readonly Options options;

        public EnergyReporterImpl(ReportGenerator reportGenerator, EmailSender emailSender, OwnerApiService ownerApiService,
            OrangeRocklandService orangeRocklandService, DateTimeZone reportTimeZone, Settings settings, Options options) {
            this.reportGenerator = reportGenerator;
            this.emailSender = emailSender;
            this.ownerApiService = ownerApiService;
            this.orangeRocklandService = orangeRocklandService;
            this.reportTimeZone = reportTimeZone;
            this.settings = settings;
            this.options = options;
        }

        public async Task<int> sendSolarReport() {
            LOGGER.Info("Dad's Energy Reporter {0}", Assembly.GetExecutingAssembly().GetName().Version);

            LOGGER.Debug("Validating options");
            try {
                validateOptions(options);
            } catch (Exception) {
                return 1;
            }

            LOGGER.Debug("Validating settings");
            try {
                validateSettings(settings);
            } catch (SettingsException) {
                return 1;
            }

            try {
                LOGGER.Info("Logging in...");
                await logIn();
                LOGGER.Info("Logged in.");

                SolarReport report =
                    await reportGenerator.generateSolarReport(new DateInterval(options.startDate, options.endDate));

                Console.WriteLine($"{report.powerGenerated:N2}");

                return 0;
            } catch (Exception e) {
                LOGGER.Error(e, "Aborted report generation due to exception " + e);
                return 1;
            } finally {
                LOGGER.Info("Logging out");
                await ownerApiService.authentication.logOut();
                LOGGER.Info("Done");
            }
        }

        public async Task<int> sendSolarAndUtilityReport() {
            LOGGER.Info("Dad's Energy Reporter {0}", Assembly.GetExecutingAssembly().GetName().Version);

            LOGGER.Debug("Validating settings");
            try {
                validateSettings(settings);
            } catch (SettingsException) {
                return 1;
            }

            Instant mostRecentReportBillingDate = Instant.FromDateTimeUtc(settings.mostRecentReportBillingDate);

            if (haveSentReportTooRecently(mostRecentReportBillingDate)) {
                LOGGER.Info(
                    "Report was already created and sent for billing cycle ending on {0}, which is too recent. Not checking again now.",
                    mostRecentReportBillingDate.InZone(DateTimeZoneProviders.Tzdb.GetSystemDefault()).Date);
                return 0;
            }

            try {
                LOGGER.Info("Logging in...");
                await logIn();
                LOGGER.Info("Logged in.");

                SolarAndUtilityReport report = await reportGenerator.generateReport();

                if (haveAlreadySentReport(mostRecentReportBillingDate, report)) {
                    LOGGER.Info("Report has already been sent for billing cycle ending on {0}, not sending again.",
                        report.billingDate);
                    return 0;
                }

                LOGGER.Info("Sending email report");
                await emailSender.sendEmail(report, settings.reportRecipientEmails);
                settings.mostRecentReportBillingDate =
                    report.billingDate.AtStartOfDayInZone(reportTimeZone).ToInstant().ToDateTimeUtc();
                settings.save();
                return 0;
            } catch (Exception e) {
                LOGGER.Error(e, "Aborted report generation due to exception");
                return 1;
            } finally {
                LOGGER.Info("Logging out");
                Task.WaitAll(
                    ownerApiService.authentication.logOut(),
                    options.skipUtility ? Task.CompletedTask : orangeRocklandService.authentication.logOut());
                LOGGER.Info("Done");
            }
        }

        internal Task logIn() {
            orangeRocklandService.authentication.username = settings.orangeRocklandUsername;
            orangeRocklandService.authentication.password = settings.orangeRocklandPassword;
            ownerApiService.authentication.username = settings.solarCityUsername;
            ownerApiService.authentication.password = settings.solarCityPassword;

            return Task.WhenAll(
                options.skipUtility ? Task.CompletedTask : orangeRocklandService.authentication.getAuthToken(),
                ownerApiService.authentication.getAuthToken());
        }

        internal bool haveSentReportTooRecently(Instant mostRecentReportBillingDate) {
            double daysSinceLastReportGenerated =
                new Interval(mostRecentReportBillingDate, SystemClock.Instance.GetCurrentInstant()).Duration.TotalDays;
            return daysSinceLastReportGenerated < 28;
        }

        internal bool haveAlreadySentReport(Instant mostRecentReportBillingDate, SolarAndUtilityReport report) {
            return !(mostRecentReportBillingDate.InZone(reportTimeZone).Date < report.billingDate);
        }

        private static void validateSettings(Settings settings) {
            try {
                settings.validate();
            } catch (SettingsException e) {
                LOGGER.Error($"Invalid setting: {e.settingsKey} = {e.invalidValue}, {e.Message}.");
                throw;
            }
        }

        private void validateOptions(Options opts) {
            try {
                if (!opts.skipUtility) {
                    return;
                } else if (opts.startDate == default) {
                    throw new Exception("--start-date option must have a value specified when --skip-utility is specified");
                } else if (opts.endDate == default) {
                    throw new Exception("--end-date option must have a value specified when --skip-utility is specified");
                } else if (opts.endDate <= opts.startDate) {
                    throw new Exception("Value of --end-date must be aftert value of --start-date");
                }
            } catch (Exception e) {
                LOGGER.Error(e, "Invalid option passed on the command line");
                throw;
            }
        }

    }

}