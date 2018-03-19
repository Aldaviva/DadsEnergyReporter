using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using DadsEnergyReporter.Exceptions;
using FluentAssertions;
using NodaTime;
using Xunit;

namespace DadsEnergyReporter.Data
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class TestSettings
    {
        public static IEnumerable<object[]> ValidSettings => new[]
        {
            new object[] { nameof(Settings.OrangeRocklandUsername), "foo" },
            new object[] { nameof(Settings.OrangeRocklandPassword), "foo" },
            new object[] { nameof(Settings.SolarCityUsername), "foo" },
            new object[] { nameof(Settings.SolarCityPassword), "foo" },
            new object[] { nameof(Settings.SmtpUsername), "foo" },
            new object[] { nameof(Settings.SmtpPassword), "foo" },
            new object[] { nameof(Settings.SmtpHost), "foo.com" },
            new object[] { nameof(Settings.SmtpPort), (ushort) 996 },
            new object[] { nameof(Settings.ReportRecipientEmails), new List<string> { "foo@bar.com" } },
            new object[] { nameof(Settings.ReportSenderEmail), "foo@bar.com" },
            new object[] { nameof(Settings.MostRecentReportBillingDate), DateTime.MinValue },
            new object[] { nameof(Settings.MostRecentReportBillingDate), Instant.FromUnixTimeMilliseconds(1507189000194L).ToDateTimeUtc() },
            new object[] { nameof(Settings.HttpProxy), "127.0.0.1:9998" },
            new object[] { nameof(Settings.HttpProxy), " " },
            new object[] { nameof(Settings.HttpProxy), "" },
            new object[] { nameof(Settings.HttpProxy), null }
        };

        public static IEnumerable<object[]> InvalidSettings => new[]
        {
            new object[] { nameof(Settings.OrangeRocklandUsername), "" },
            new object[] { nameof(Settings.OrangeRocklandUsername), " " },
            new object[] { nameof(Settings.OrangeRocklandPassword), " " },
            new object[] { nameof(Settings.OrangeRocklandPassword), "" },
            new object[] { nameof(Settings.SolarCityUsername), "" },
            new object[] { nameof(Settings.SolarCityUsername), " " },
            new object[] { nameof(Settings.SolarCityPassword), "" },
            new object[] { nameof(Settings.SolarCityPassword), " " },
            new object[] { nameof(Settings.SmtpUsername), "" },
            new object[] { nameof(Settings.SmtpUsername), " " },
            new object[] { nameof(Settings.SmtpPassword), "" },
            new object[] { nameof(Settings.SmtpPassword), " " },
            new object[] { nameof(Settings.SmtpHost), " " },
            new object[] { nameof(Settings.SmtpHost), "" },
            new object[] { nameof(Settings.SmtpPort), (ushort) 0 },
            new object[] { nameof(Settings.ReportRecipientEmails), null },
            new object[] { nameof(Settings.ReportRecipientEmails), new List<string>() },
            new object[] { nameof(Settings.ReportRecipientEmails), new List<string> { "" } },
            new object[] { nameof(Settings.ReportRecipientEmails), new List<string> { " " } },
            new object[] { nameof(Settings.ReportRecipientEmails), new List<string> { null } },
            new object[] { nameof(Settings.ReportRecipientEmails), new List<string> { "foo" } },
            new object[] { nameof(Settings.ReportRecipientEmails), new List<string> { "foo@" } },
            new object[] { nameof(Settings.ReportRecipientEmails), new List<string> { "foo@bar" } },
            new object[] { nameof(Settings.ReportRecipientEmails), new List<string> { "@bar" } },
            new object[] { nameof(Settings.ReportRecipientEmails), new List<string> { "a@b.com c@d.com"  } },
            new object[] { nameof(Settings.ReportRecipientEmails), new List<string> { "a@b.com,c@d.com"  } },
            new object[] { nameof(Settings.ReportRecipientEmails), new List<string> { "a@b.com;c@d.com"  } },
            new object[] { nameof(Settings.ReportRecipientEmails), new List<string> { "a@b.com, c@d.com" } },
            new object[] { nameof(Settings.ReportRecipientEmails), new List<string> { "a@b.com; c@d.com" } },
            new object[] { nameof(Settings.ReportSenderEmail), "foo" },
            new object[] { nameof(Settings.ReportSenderEmail), "foo@" },
            new object[] { nameof(Settings.ReportSenderEmail), "foo@bar" },
            new object[] { nameof(Settings.ReportSenderEmail), "@bar.com" },
            new object[] { nameof(Settings.ReportSenderEmail), "a@b.com c@d.com" },
            new object[] { nameof(Settings.ReportSenderEmail), "a@b.com,c@d.com" },
            new object[] { nameof(Settings.ReportSenderEmail), "a@b.com;c@d.com" },
            new object[] { nameof(Settings.ReportSenderEmail), "a@b.com, c@d.com" },
            new object[] { nameof(Settings.ReportSenderEmail), "a@b.com; c@d.com" },
            new object[] { nameof(Settings.MostRecentReportBillingDate), DateTime.MaxValue },
            new object[] { nameof(Settings.HttpProxy), ":9998" }
        };

        [Fact]
        public void AllSettingsCovered()
        {
            foreach (PropertyInfo settingsProperty in typeof(Settings).GetProperties())
            {
                string name = settingsProperty.Name;
                ValidSettings.Should().Contain(theories => theories[0].Equals(name), $"ValidSettings must test the {name} setting");
                InvalidSettings.Should().Contain(theories => theories[0].Equals(name),
                    $"InvalidSettings must test the {name} setting");
            }
        }

        [Theory, MemberData(nameof(ValidSettings))]
        public void ValidateShouldNotThrowOnValidSettings(string settingsKey, object settingsValue)
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
            Settings settings = CreateValidSettings();
            Action thrower = () => settings.Validate();
            thrower.ShouldNotThrow<SettingsException>();

            settings.ReportSenderEmail = "foo";
            thrower.ShouldThrow<SettingsException>();
        }

        [Fact]
        public void DefaultValues()
        {
            var settings = new Settings();
            settings.SmtpPort.Should().Be(25);
            settings.ReportRecipientEmails.Should().BeEmpty();
            settings.ReportSenderEmail.Should().Be("dadsenergyreporter@aldaviva.com");
        }

        private static void TestSettingsValidation(string settingsKey, object settingsValue, bool isValid)
        {
            Settings settings = CreateValidSettings();
            PropertyInfo settingProperty = typeof(Settings).GetProperty(settingsKey);
            settingProperty.Should().NotBeNull();
            settingProperty?.SetValue(settings, settingsValue);
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
                ReportRecipientEmails = new List<string> { "recipient@aldaviva.com" },
                OrangeRocklandUsername = "a",
                OrangeRocklandPassword = "b",
                SolarCityUsername = "c",
                SolarCityPassword = "d",
                MostRecentReportBillingDate = DateTime.Now,
                ReportSenderEmail = "sender@aldaviva.com",
                SmtpHost = "domain.com",
                SmtpPort = 465,
                SmtpUsername = "e",
                SmtpPassword = "f"
            };
            settings.Validate();
            return settings;
        }
    }
}