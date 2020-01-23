using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DadsEnergyReporter.Data;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Injection;
using MailKit;
using MailKit.Security;
using MimeKit;

namespace DadsEnergyReporter.Service {

    public interface EmailSender {

        Task sendEmail(SolarAndUtilityReport report, IEnumerable<string> recipients);

    }

    [Component]
    internal class EmailSenderImpl: EmailSender {

        private readonly IMailTransport smtpClient;
        private readonly Settings settings;
        private readonly ReportFormatter reportFormatter;

        public EmailSenderImpl(IMailTransport smtpClient, ReportFormatter reportFormatter, Settings settings) {
            this.smtpClient = smtpClient;
            this.reportFormatter = reportFormatter;
            this.settings = settings;
        }

        public async Task sendEmail(SolarAndUtilityReport report, IEnumerable<string> recipients) {
            MimeMessage message = reportFormatter.formatReport(report);
            message.From.Add(new MailboxAddress("Dad's Energy Reporter", settings.reportSenderEmail));
            message.To.AddRange(recipients.Select(recipient => new MailboxAddress(recipient)));

            try {
                await smtpClient.ConnectAsync(settings.smtpHost, settings.smtpPort, SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(settings.smtpUsername, settings.smtpPassword);
                await smtpClient.SendAsync(message);
                await smtpClient.DisconnectAsync(true);
            } catch (IOException e) {
                throw new EmailException("Failed to send email message", e);
            }
        }

    }

}