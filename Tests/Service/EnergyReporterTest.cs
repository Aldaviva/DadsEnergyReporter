using System.Collections.Generic;
using DadsEnergyReporter.Data;
using DadsEnergyReporter.Properties;
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
        private readonly PowerGuideAuthenticationService powerGuideAuthenticationService = A.Fake<PowerGuideAuthenticationService>();
        private readonly OrangeRocklandAuthenticationService orangeRocklandAuthenticationService = A.Fake<OrangeRocklandAuthenticationService>();
        private readonly Settings settings = new Settings
        {
            orangeRocklandUsername = "oruUser",
            orangeRocklandPassword = "oruPass",
            solarCityUsername = "solarcityUser",
            solarCityPassword = "solarcityPass",
            mostRecentReportBillingDate = 0,
            reportRecipientEmails = new List<string> { "ben@aldaviva.com" },
            reportSenderEmail = "dadsenergyreporter@aldaviva.com",
            smtpUsername = "hargle",
            smtpPassword = "blargle",
            smtpHost = "aldaviva.com"
        };

        private static readonly DateTimeZone ZONE = DateTimeZoneProviders.Tzdb["America/New_York"];

        public EnergyReporterTest()
        {
            var powerGuideService = new PowerGuideServiceImpl(powerGuideAuthenticationService, null, null);
            var orangeRocklandService = new OrangeRocklandServiceImpl(orangeRocklandAuthenticationService, null, null);
            energyReporter = new EnergyReporterImpl(reportGenerator, emailSender, powerGuideService, orangeRocklandService, ZONE, settings);
        }

        [Fact]
        public async void Normal()
        {
            var report = new Report(new DateInterval(new LocalDate(2017, 07, 17), new LocalDate(2017, 08, 16)), 100, 2, 2000);
            A.CallTo(() => reportGenerator.GenerateReport()).Returns(report);

            await energyReporter.Start();

            A.CallTo(() => powerGuideAuthenticationService.GetAuthToken()).MustHaveHappened();
            A.CallTo(() => orangeRocklandAuthenticationService.GetAuthToken()).MustHaveHappened();
            A.CallTo(() => reportGenerator.GenerateReport()).MustHaveHappened();
            A.CallTo(() => emailSender.SendEmail(report, A<IEnumerable<string>>.That.IsSameSequenceAs(new List<string> { "ben@aldaviva.com" }))).MustHaveHappened();

            A.CallTo(() => powerGuideAuthenticationService.LogOut()).MustHaveHappened();
            A.CallTo(() => orangeRocklandAuthenticationService.LogOut()).MustHaveHappened();

            settings.mostRecentReportBillingDate.Should().Be(report.BillingDate.AtStartOfDayInZone(ZONE).ToInstant().ToUnixTimeMilliseconds());
        }

        [Fact]
        public async void SkipsIfTooFewDaysSinceLastReport()
        {
            settings.mostRecentReportBillingDate = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(27)).ToUnixTimeMilliseconds();

            await energyReporter.Start();

            A.CallTo(() => powerGuideAuthenticationService.GetAuthToken()).MustNotHaveHappened();
            A.CallTo(() => orangeRocklandAuthenticationService.GetAuthToken()).MustNotHaveHappened();
            A.CallTo(() => reportGenerator.GenerateReport()).MustNotHaveHappened();
            A.CallTo(() => emailSender.SendEmail(A<Report>._, A<IEnumerable<string>>._)).MustNotHaveHappened();
        }
        
        [Fact]
        public async void SkipsIfAlreadySentReport()
        {
            settings.mostRecentReportBillingDate = new LocalDate(2017, 08, 16).AtStartOfDayInZone(ZONE).ToInstant().ToUnixTimeMilliseconds();

            var report = new Report(new DateInterval(new LocalDate(2017, 07, 17), new LocalDate(2017, 08, 16)), 100, 20, 3000);
            A.CallTo(() => reportGenerator.GenerateReport()).Returns(report);
            
            await energyReporter.Start();

            A.CallTo(() => powerGuideAuthenticationService.GetAuthToken()).MustHaveHappened();
            A.CallTo(() => orangeRocklandAuthenticationService.GetAuthToken()).MustHaveHappened();
            A.CallTo(() => reportGenerator.GenerateReport()).MustHaveHappened();
            
            A.CallTo(() => emailSender.SendEmail(A<Report>._, A<IEnumerable<string>>._)).MustNotHaveHappened();
        }
    }
}