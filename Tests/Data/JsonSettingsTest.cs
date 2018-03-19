using System;
using System.IO;
using FluentAssertions;
using Xunit;

namespace DadsEnergyReporter.Data
{
    public class JsonSettingsTest : IDisposable
    {
        private const string FILENAME = @"%localappdata%/Dad's Energy Reporter/test-settings.json";

        public JsonSettingsTest()
        {
            Settings.SettingsManager.Filename = FILENAME;
        }

        public void Dispose()
        {
            File.Delete(Environment.ExpandEnvironmentVariables(FILENAME));
        }

        [Fact]
        public void SavesToDisk()
        {
            DateTime now = DateTime.Now;

            var writingSettings = new Settings
            {
                HttpProxy = "127.0.0.1:9998",
                MostRecentReportBillingDate = now,
                SmtpPort = 587,
                ReportRecipientEmails = new[]{ "ben@aldaviva.com" }
            };
            writingSettings.Save();

            var readingSettings = new Settings();
            readingSettings.Reload();

            readingSettings.HttpProxy.Should().Be("127.0.0.1:9998");
            readingSettings.MostRecentReportBillingDate.Should().Be(now);
            readingSettings.SmtpPort.Should().Be(587);
            readingSettings.ReportRecipientEmails.Should().BeEquivalentTo("ben@aldaviva.com");
        }
    }
}