using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Exceptions;
using MimeKit;

namespace DadsEnergyReporter.Data {

    public class Settings: Validatable {

        internal static readonly JsonSettingsManager<Settings> SETTINGS_MANAGER = new JsonSettingsManager<Settings> {
            filename = @"%localappdata%/Dad's Energy Reporter/settings.json"
        };

        public string solarCityUsername { get; set; }
        public string solarCityPassword { get; set; }
        public string orangeRocklandUsername { get; set; }
        public string orangeRocklandPassword { get; set; }
        public DateTime mostRecentReportBillingDate { get; set; } = DateTime.MinValue.ToUniversalTime();
        public string reportSenderEmail { get; set; } = "dadsenergyreporter@aldaviva.com";
        public string smtpHost { get; set; }
        public ushort smtpPort { get; set; } = 25;
        public string smtpUsername { get; set; }
        public string smtpPassword { get; set; }
        public string httpProxy { get; set; }
        public IList<string> reportRecipientEmails { get; set; } = new List<string>();

        public static Settings get() {
            return SETTINGS_MANAGER.get();
        }

        public Settings reload() {
            SETTINGS_MANAGER.reload(this);
            return this;
        }

        public Settings save() {
            SETTINGS_MANAGER.save(this);
            return this;
        }

        public void validate() {
            if (string.IsNullOrWhiteSpace(orangeRocklandUsername)) {
                throw new SettingsException("orangeRocklandUsername", orangeRocklandUsername,
                    "must specify the username for logging into Orange & Rockland");
            }

            if (string.IsNullOrWhiteSpace(orangeRocklandPassword)) {
                throw new SettingsException("orangeRocklandPassword", orangeRocklandPassword,
                    "must specify the password for logging into Orange & Rockland");
            }

            if (string.IsNullOrWhiteSpace(solarCityUsername)) {
                throw new SettingsException("solarCityUsername", solarCityUsername,
                    "must specify the username for logging into SolarCity");
            }

            if (string.IsNullOrWhiteSpace(solarCityPassword)) {
                throw new SettingsException("solarCityPassword", solarCityPassword,
                    "must specify the password for logging into SolarCity");
            }

            if (reportRecipientEmails == null || reportRecipientEmails.Count == 0) {
                throw new SettingsException("reportRecipientEmails", reportRecipientEmails,
                    "must specify one or more email addresses to which energy reports will be sent");
            }

            for (int i = 0; i < reportRecipientEmails.Count; i++) {
                validateEmailAddress($"reportRecipientEmails[{i}]", reportRecipientEmails[i]);
            }

            validateEmailAddress("reportSenderEmail", reportSenderEmail);

            if (string.IsNullOrWhiteSpace(smtpUsername)) {
                throw new SettingsException("smtpUsername", smtpUsername,
                    "must specify the username for logging into the SMTP server and sending report emails");
            }

            if (string.IsNullOrWhiteSpace(smtpPassword)) {
                throw new SettingsException("smtpPassword", smtpPassword,
                    "must specify the password for logging into the SMTP server and sending report emails");
            }

            if (smtpPort <= 0) {
                throw new SettingsException("smtpPort", smtpPort,
                    "must specify the port number (default 25) for logging into the SMTP server and sending report emails");
            }

            if (string.IsNullOrWhiteSpace(smtpHost)) {
                throw new SettingsException("smtpHost", smtpHost,
                    "must specify the host (domain name or IP address) for logging into the SMTP server and sending report emails");
            }

            if (mostRecentReportBillingDate > DateTime.Now) {
                throw new SettingsException("mostRecentReportBillingDate", mostRecentReportBillingDate,
                    "date of most recent billing cycle end is in the future");
            }

            if (!string.IsNullOrWhiteSpace(httpProxy)) {
                try {
                    // ReSharper disable once ObjectCreationAsStatement - testing if constructor throws exception
                    new WebProxy(httpProxy);
                } catch (UriFormatException) {
                    throw new SettingsException("httpProxy", httpProxy,
                        "may specify the host (domain or IP) and optionally port number of an HTTP proxy server to use for outgoing connections (e.g. \"127.0.0.1\" or \"myserver.com:8080\"), " +
                        "or may be left blank (the default) to use a direct HTTP connection");
                }
            }
        }

        private static void validateEmailAddress(string settingsKey, string emailAddress) {
            if (emailAddress != null) {
                try {
                    var parserOptions = new ParserOptions {
                        AddressParserComplianceMode = RfcComplianceMode.Strict,
                        AllowAddressesWithoutDomain = false,
                        ParameterComplianceMode = RfcComplianceMode.Strict,
                        Rfc2047ComplianceMode = RfcComplianceMode.Strict
                    };
                    MailboxAddress parsed = MailboxAddress.Parse(parserOptions, emailAddress);

                    string[] localAndDomainParts = parsed.Address.Split('@');

                    if (containsInMiddle(parsed.Address, "@", 1) && containsInMiddle(localAndDomainParts[1], ".")) {
                        // Valid
                        return;
                    }
                } catch (ParseException) { }
            }

            // Invalid
            throw new SettingsException(settingsKey, null,
                "must specify a valid email address");
        }

        private static bool containsInMiddle(string subject, string separator, int maxOccurrences = -1) {
            string[] split = subject.Split(new[] { separator }, default);
            return (maxOccurrences <= 0 ? split.Length > 1 : split.Length == maxOccurrences + 1) &&
                   !split.Any(string.IsNullOrWhiteSpace);
        }

    }

}