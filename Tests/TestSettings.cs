using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Properties;
using FluentAssertions;
using Xunit;

namespace DadsEnergyReporter
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class TestSettings
    {
        public static IEnumerable<object[]> ValidSettings => new[]
        {
            new object[] { nameof(Settings.orangeRocklandUsername), "foo" },
            new object[] { nameof(Settings.orangeRocklandPassword), "foo" },
            new object[] { nameof(Settings.solarCityUsername), "foo" },
            new object[] { nameof(Settings.solarCityPassword), "foo" },
            new object[] { nameof(Settings.smtpUsername), "foo" },
            new object[] { nameof(Settings.smtpPassword), "foo" },
            new object[] { nameof(Settings.smtpHost), "foo.com" },
            new object[] { nameof(Settings.smtpPort), (ushort) 996 },
            new object[] { nameof(Settings.reportRecipientEmails), new List<string> { "foo@bar.com" } },
            new object[] { nameof(Settings.reportSenderEmail), "foo@bar.com" },
            new object[] { nameof(Settings.mostRecentReportBillingDate), 0L },
            new object[] { nameof(Settings.mostRecentReportBillingDate), 1507189000194L }
        };

        public static IEnumerable<object[]> InvalidSettings => new[]
        {
            new object[] { nameof(Settings.orangeRocklandUsername), "" },
            new object[] { nameof(Settings.orangeRocklandUsername), " " },
            new object[] { nameof(Settings.orangeRocklandPassword), " " },
            new object[] { nameof(Settings.orangeRocklandPassword), "" },
            new object[] { nameof(Settings.solarCityUsername), "" },
            new object[] { nameof(Settings.solarCityUsername), " " },
            new object[] { nameof(Settings.solarCityPassword), "" },
            new object[] { nameof(Settings.solarCityPassword), " " },
            new object[] { nameof(Settings.smtpUsername), "" },
            new object[] { nameof(Settings.smtpUsername), " " },
            new object[] { nameof(Settings.smtpPassword), "" },
            new object[] { nameof(Settings.smtpPassword), " " },
            new object[] { nameof(Settings.smtpHost), " " },
            new object[] { nameof(Settings.smtpHost), "" },
            new object[] { nameof(Settings.smtpPort), (ushort) 0 },
            new object[] { nameof(Settings.reportRecipientEmails), null },
            new object[] { nameof(Settings.reportRecipientEmails), new List<string>() },
            new object[] { nameof(Settings.reportRecipientEmails), new List<string> { "" } },
            new object[] { nameof(Settings.reportRecipientEmails), new List<string> { " " } },
            new object[] { nameof(Settings.reportRecipientEmails), new List<string> { null } },
            new object[] { nameof(Settings.reportRecipientEmails), new List<string> { "foo" } },
            new object[] { nameof(Settings.reportRecipientEmails), new List<string> { "foo@" } },
            new object[] { nameof(Settings.reportRecipientEmails), new List<string> { "foo@bar" } },
            new object[] { nameof(Settings.reportRecipientEmails), new List<string> { "@bar" } },
            new object[] { nameof(Settings.reportRecipientEmails), new List<string> { "a@b.com c@d.com"  } },
            new object[] { nameof(Settings.reportRecipientEmails), new List<string> { "a@b.com,c@d.com"  } },
            new object[] { nameof(Settings.reportRecipientEmails), new List<string> { "a@b.com;c@d.com"  } },
            new object[] { nameof(Settings.reportRecipientEmails), new List<string> { "a@b.com, c@d.com" } },
            new object[] { nameof(Settings.reportRecipientEmails), new List<string> { "a@b.com; c@d.com" } },
            new object[] { nameof(Settings.reportSenderEmail), "foo" },
            new object[] { nameof(Settings.reportSenderEmail), "foo@" },
            new object[] { nameof(Settings.reportSenderEmail), "foo@bar" },
            new object[] { nameof(Settings.reportSenderEmail), "@bar.com" },
            new object[] { nameof(Settings.reportSenderEmail), "a@b.com c@d.com" },
            new object[] { nameof(Settings.reportSenderEmail), "a@b.com,c@d.com" },
            new object[] { nameof(Settings.reportSenderEmail), "a@b.com;c@d.com" },
            new object[] { nameof(Settings.reportSenderEmail), "a@b.com, c@d.com" },
            new object[] { nameof(Settings.reportSenderEmail), "a@b.com; c@d.com" },
            new object[] { nameof(Settings.mostRecentReportBillingDate), -1L }
        };

        [Fact]
        public void AllSettingsCovered()
        {
            foreach (SettingsProperty settingsProperty in new Settings().Properties)
            {
                string name = settingsProperty.Name;
                ValidSettings.Should().Contain(theories => theories[0].Equals(name), $"ValidSettings must test the {name} setting");
                InvalidSettings.Should().Contain(theories => theories[0].Equals(name),
                    $"InvalidSettings must test the {name} setting");
            }
        }

        [Theory, MemberData(nameof(ValidSettings))]
        public void ValidateShouldnotThrowOnValidSettings(string settingsKey, object settingsValue)
        {
            TestSettingsValidation(settingsKey, settingsValue, true);
        }

        [Theory, MemberData(nameof(InvalidSettings))]
        public void ValidateShouldThrowOnInvalidSettings(string settingsKey, object settingsValue)
        {
            TestSettingsValidation(settingsKey, settingsValue, false);
        }

        [Fact]
        public void ValidateEmailAddress()
        {
            Action thrower = () => Settings.ValidateEmailAddress("ValidateEmailAddress", "foo");
            thrower.ShouldThrow<SettingsException>();
        }

        private static void TestSettingsValidation(string settingsKey, object settingsValue, bool isValid)
        {
            Settings settings = CreateValidSettings();
            settings[settingsKey] = settingsValue;
            Action validateMethod = settings.Validate;
            if (isValid)
            {
                validateMethod.ShouldNotThrow<SettingsException>($"{settingsKey} = {settingsValue} is valid");
            }
            else
            {
                validateMethod.ShouldThrow<SettingsException>($"{settingsKey} = {settingsValue} is invalid");
            }
        }

        private static Settings CreateValidSettings()
        {
            var settings = new Settings
            {
                reportRecipientEmails = new List<string> { "ben@aldaviva.com" },
                orangeRocklandUsername = "a",
                orangeRocklandPassword = "b",
                solarCityUsername = "c",
                solarCityPassword = "d",
                mostRecentReportBillingDate = 0,
                reportSenderEmail = "dadsenergyreporter@aldaviva.com",
                smtpHost = "domain.com",
                smtpPort = 465,
                smtpUsername = "e",
                smtpPassword = "f"
            };
            settings.Validate();
            return settings;
        }
    }
}