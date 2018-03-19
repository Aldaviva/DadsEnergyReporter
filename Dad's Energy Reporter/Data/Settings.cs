using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Exceptions;
using MimeKit;

namespace DadsEnergyReporter.Data
{
    public class Settings : Validatable
    {
        internal static readonly JsonSettingsManager<Settings> SettingsManager = new JsonSettingsManager<Settings>
        {
            Filename = @"%localappdata%/Dad's Energy Reporter/settings.json"
        };

        public string SolarCityUsername { get; set; }
        public string SolarCityPassword { get; set; }
        public string OrangeRocklandUsername { get; set; }
        public string OrangeRocklandPassword { get; set; }
        public DateTime MostRecentReportBillingDate { get; set; } = DateTime.MinValue.ToUniversalTime();
        public string ReportSenderEmail { get; set; } = "dadsenergyreporter@aldaviva.com";
        public string SmtpHost { get; set; }
        public ushort SmtpPort { get; set; } = 25;
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public string HttpProxy { get; set; }
        public IList<string> ReportRecipientEmails { get; set; } = new List<string>();

        public static Settings Get()
        {
            return SettingsManager.Get();
        }

        public Settings Reload()
        {
            SettingsManager.Reload(this);
            return this;
        }

        public Settings Save()
        {
            SettingsManager.Save(this);
            return this;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(OrangeRocklandUsername))
            {
                throw new SettingsException("orangeRocklandUsername", OrangeRocklandUsername,
                    "must specify the username for logging into Orange & Rockland");
            }

            if (string.IsNullOrWhiteSpace(OrangeRocklandPassword))
            {
                throw new SettingsException("orangeRocklandPassword", OrangeRocklandPassword,
                    "must specify the password for logging into Orange & Rockland");
            }

            if (string.IsNullOrWhiteSpace(SolarCityUsername))
            {
                throw new SettingsException("solarCityUsername", SolarCityUsername,
                    "must specify the username for logging into SolarCity");
            }

            if (string.IsNullOrWhiteSpace(SolarCityPassword))
            {
                throw new SettingsException("solarCityPassword", SolarCityPassword,
                    "must specify the password for logging into SolarCity");
            }

            if (ReportRecipientEmails == null || ReportRecipientEmails.Count == 0)
            {
                throw new SettingsException("reportRecipientEmails", ReportRecipientEmails,
                    "must specify one or more email addresses to which energy reports will be sent");
            }

            for (int i = 0; i < ReportRecipientEmails.Count; i++)
            {
                ValidateEmailAddress($"reportRecipientEmails[{i}]", ReportRecipientEmails[i]);
            }

            ValidateEmailAddress("reportSenderEmail", ReportSenderEmail);

            if (string.IsNullOrWhiteSpace(SmtpUsername))
            {
                throw new SettingsException("smtpUsername", SmtpUsername,
                    "must specify the username for logging into the SMTP server and sending report emails");
            }

            if (string.IsNullOrWhiteSpace(SmtpPassword))
            {
                throw new SettingsException("smtpPassword", SmtpPassword,
                    "must specify the password for logging into the SMTP server and sending report emails");
            }

            if (SmtpPort <= 0)
            {
                throw new SettingsException("smtpPort", SmtpPort,
                    "must specify the port number (default 25) for logging into the SMTP server and sending report emails");
            }

            if (string.IsNullOrWhiteSpace(SmtpHost))
            {
                throw new SettingsException("smtpHost", SmtpHost,
                    "must specify the host (domain name or IP address) for logging into the SMTP server and sending report emails");
            }

            if (MostRecentReportBillingDate > DateTime.Now)
            {
                throw new SettingsException("mostRecentReportBillingDate", MostRecentReportBillingDate,
                    "date of most recent billing cycle end is in the future");
            }

            if (!string.IsNullOrWhiteSpace(HttpProxy))
            {
                try
                {
                    // ReSharper disable once ObjectCreationAsStatement - testing if constructor throws exception
                    new WebProxy(HttpProxy);
                }
                catch (UriFormatException)
                {
                    throw new SettingsException("httpProxy", HttpProxy,
                        "may specify the host (domain or IP) and optionally port number of an HTTP proxy server to use for outgoing connections (e.g. \"127.0.0.1\" or \"myserver.com:8080\"), " +
                        "or may be left blank (the default) to use a direct HTTP connection");
                }
            }
        }

        private static void ValidateEmailAddress(string settingsKey, string emailAddress)
        {
            if (emailAddress != null)
            {
                try
                {
                    var parserOptions = new ParserOptions
                    {
                        AddressParserComplianceMode = RfcComplianceMode.Strict,
                        AllowAddressesWithoutDomain = false,
                        ParameterComplianceMode = RfcComplianceMode.Strict,
                        Rfc2047ComplianceMode = RfcComplianceMode.Strict
                    };
                    MailboxAddress parsed = MailboxAddress.Parse(parserOptions, emailAddress);


                    string[] localAndDomainParts = parsed.Address.Split('@');

                    if (ContainsInMiddle(parsed.Address, "@", 1) && ContainsInMiddle(localAndDomainParts[1], "."))
                    {
                        // Valid
                        return;
                    }
                }
                catch (ParseException)
                {
                }
            }

            // Invalid
            throw new SettingsException(settingsKey, null,
                "must specify a valid email address");
        }

        private static bool ContainsInMiddle(string subject, string separator, int maxOccurrences = -1)
        {
            string[] split = subject.Split(new[] { separator }, default);
            return (maxOccurrences <= 0 ? split.Length > 1 : split.Length == maxOccurrences + 1) &&
                   !split.Any(string.IsNullOrWhiteSpace);
        }
    }
}