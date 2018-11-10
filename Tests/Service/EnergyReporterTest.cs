using System;
using System.Collections.Generic;
using DadsEnergyReporter.Data;
using DadsEnergyReporter.Entry;
using DadsEnergyReporter.Remote.OrangeRockland.Service;
using DadsEnergyReporter.Remote.PowerGuide.Service;
using FakeItEasy;
using FluentAssertions;
using NodaTime;
using Xunit;

namespace DadsEnergyReporter.Service
{
    public class EnergyReporterTest
    {
        private readonly EnergyReporterImpl energyReporter;
        private readonly ReportGenerator reportGenerator = A.Fake<ReportGenerator>();
        private readonly EmailSender emailSender = A.Fake<EmailSender>();

        private readonly PowerGuideAuthenticationService
            powerGuideAuthenticationService = A.Fake<PowerGuideAuthenticationService>();

        private readonly OrangeRocklandAuthenticationService orangeRocklandAuthenticationService =
            A.Fake<OrangeRocklandAuthenticationService>();

        private readonly Settings settings = new Settings
        {
            OrangeRocklandUsername = "oruUser",
            OrangeRocklandPassword = "oruPass",
            SolarCityUsername = "solarcityUser",
            SolarCityPassword = "solarcityPass",
            MostRecentReportBillingDate = DateTime.MinValue.ToUniversalTime(),
            ReportRecipientEmails = new List<string> { "ben@aldaviva.com" },
            ReportSenderEmail = "dadsenergyreporter@aldaviva.com",
            SmtpUsername = "hargle",
            SmtpPassword = "blargle",
            SmtpHost = "aldaviva.com"
        };

        private readonly Options options = new Options
        {
            SkipUtility = false
        };

        private static readonly DateTimeZone ZONE = DateTimeZoneProviders.Tzdb["America/New_York"];

        public EnergyReporterTest()
        {
            var powerGuideService = new PowerGuideServiceImpl(powerGuideAuthenticationService, null, null);
            var orangeRocklandService = new OrangeRocklandServiceImpl(orangeRocklandAuthenticationService, null, null);
            energyReporter = new EnergyReporterImpl(reportGenerator, emailSender, powerGuideService, orangeRocklandService, ZONE,
                settings, options);
        }

        [Fact]
        public async void Normal()
        {
            var report = new SolarAndUtilityReport(new DateInterval(new LocalDate(2017, 07, 17), new LocalDate(2017, 08, 16)), 100,
                2, 2000);
            A.CallTo(() => reportGenerator.GenerateReport()).Returns(report);

            await energyReporter.SendSolarAndUtilityReport();

            A.CallTo(() => powerGuideAuthenticationService.GetAuthToken()).MustHaveHappened();
            A.CallTo(() => orangeRocklandAuthenticationService.GetAuthToken()).MustHaveHappened();
            A.CallTo(() => reportGenerator.GenerateReport()).MustHaveHappened();
            A.CallTo(() =>
                emailSender.SendEmail(report,
                    A<IEnumerable<string>>.That.IsSameSequenceAs(new List<string> { "ben@aldaviva.com" }))).MustHaveHappened();

            A.CallTo(() => powerGuideAuthenticationService.LogOut()).MustHaveHappened();
            A.CallTo(() => orangeRocklandAuthenticationService.LogOut()).MustHaveHappened();

            settings.MostRecentReportBillingDate.Should()
                .Be(report.BillingDate.AtStartOfDayInZone(ZONE).ToInstant().ToDateTimeUtc());
        }

        [Fact]
        public async void SkipsIfTooFewDaysSinceLastReport()
        {
            settings.MostRecentReportBillingDate =
                SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(27)).ToDateTimeUtc();

            await energyReporter.SendSolarAndUtilityReport();

            A.CallTo(() => powerGuideAuthenticationService.GetAuthToken()).MustNotHaveHappened();
            A.CallTo(() => orangeRocklandAuthenticationService.GetAuthToken()).MustNotHaveHappened();
            A.CallTo(() => reportGenerator.GenerateReport()).MustNotHaveHappened();
            A.CallTo(() => emailSender.SendEmail(A<SolarAndUtilityReport>._, A<IEnumerable<string>>._)).MustNotHaveHappened();
        }

        [Fact]
        public async void SkipsIfAlreadySentReport()
        {
            settings.MostRecentReportBillingDate = new LocalDate(2017, 08, 16).AtStartOfDayInZone(ZONE).ToInstant().ToDateTimeUtc();

            var report = new SolarAndUtilityReport(new DateInterval(new LocalDate(2017, 07, 17), new LocalDate(2017, 08, 16)), 100,
                20, 3000);
            A.CallTo(() => reportGenerator.GenerateReport()).Returns(report);

            await energyReporter.SendSolarAndUtilityReport();

            A.CallTo(() => powerGuideAuthenticationService.GetAuthToken()).MustHaveHappened();
            A.CallTo(() => orangeRocklandAuthenticationService.GetAuthToken()).MustHaveHappened();
            A.CallTo(() => reportGenerator.GenerateReport()).MustHaveHappened();

            A.CallTo(() => emailSender.SendEmail(A<SolarAndUtilityReport>._, A<IEnumerable<string>>._)).MustNotHaveHappened();
        }
    }
}