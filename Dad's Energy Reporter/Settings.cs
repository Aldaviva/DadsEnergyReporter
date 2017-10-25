using System;
using System.Linq;
using System.Net;
using DadsEnergyReporter.Exceptions;
using MimeKit;

// ReSharper disable once CheckNamespace Visual Studio just does it this way, trying to fix it will break compilation
namespace DadsEnergyReporter.Properties
{
    internal sealed partial class Settings
    {
        internal void Validate()
        {
            if (string.IsNullOrWhiteSpace(orangeRocklandUsername))
            {
                throw new SettingsException("orangeRocklandUsername", orangeRocklandUsername,
                    "must specify the username for logging into Orange & Rockland");
            }

            if (string.IsNullOrWhiteSpace(orangeRocklandPassword))
            {
                throw new SettingsException("orangeRocklandPassword", orangeRocklandPassword,
                    "must specify the password for logging into Orange & Rockland");
            }

            if (string.IsNullOrWhiteSpace(solarCityUsername))
            {
                throw new SettingsException("solarCityUsername", solarCityUsername,
                    "must specify the username for logging into SolarCity");
            }

            if (string.IsNullOrWhiteSpace(solarCityPassword))
            {
                throw new SettingsException("solarCityPassword", solarCityPassword,
                    "must specify the password for logging into SolarCity");
            }

            if (reportRecipientEmails == null || reportRecipientEmails.Count == 0)
            {
                throw new SettingsException("reportRecipientEmails", reportRecipientEmails,
                    "must specify one or more email addresses to which energy reports will be sent");
            }

            for (int i = 0; i < reportRecipientEmails.Count; i++)
            {
                ValidateEmailAddress($"reportRecipientEmails[{i}]", reportRecipientEmails[i]);
            }

            ValidateEmailAddress("reportSenderEmail", reportSenderEmail);

            if (string.IsNullOrWhiteSpace(smtpUsername))
            {
                throw new SettingsException("smtpUsername", smtpUsername,
                    "must specify the username for logging into the SMTP server and sending report emails");
            }

            if (string.IsNullOrWhiteSpace(smtpPassword))
            {
                throw new SettingsException("smtpPassword", smtpPassword,
                    "must specify the password for logging into the SMTP server and sending report emails");
            }

            if (smtpPort <= 0)
            {
                throw new SettingsException("smtpPort", smtpPort,
                    "must specify the port number (default 25) for logging into the SMTP server and sending report emails");
            }

            if (string.IsNullOrWhiteSpace(smtpHost))
            {
                throw new SettingsException("smtpHost", smtpHost,
                    "must specify the host (domain name or IP address) for logging into the SMTP server and sending report emails");
            }

            if (mostRecentReportBillingDate < 0)
            {
                throw new SettingsException("mostRecentReportBillingDate", mostRecentReportBillingDate,
                    "malformed date of most recent billing cycle end");
            }

            if (!string.IsNullOrWhiteSpace(httpProxy))
            {
                try
                {
                    // ReSharper disable once ObjectCreationAsStatement - testing if constructor throws exception
                    new WebProxy(httpProxy);
                }
                catch (UriFormatException)
                {
                    throw new SettingsException("httpProxy", httpProxy,
                        "may specify the host (domain or IP) and optionally port number of an HTTP proxy server to use for outgoing connections (e.g. \"127.0.0.1\" or \"myserver.com:8080\"), " +
                        "or may be left blank (the default) to use a direct HTTP connection");
                }
            }
        }

        internal static void ValidateEmailAddress(string settingsKey, string emailAddress)
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