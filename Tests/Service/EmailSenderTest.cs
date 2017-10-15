using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DadsEnergyReporter.Data;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Properties;
using FakeItEasy;
using FluentAssertions;
using MailKit;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using NodaTime;
using Xunit;

namespace DadsEnergyReporter.Service
{
    public class EmailSenderTest
    {
        private readonly EmailSenderImpl emailSender;
        private readonly IMailTransport smtpClient = A.Fake<IMailTransport>();

        public EmailSenderTest()
        {
            emailSender = new EmailSenderImpl(smtpClient);
        }

        [Fact]
        public async void SendEmail()
        {
            Settings settings = Settings.Default;
            settings.smtpHost = "aldaviva.com";
            settings.smtpPort = 25;
            settings.smtpUsername = "user";
            settings.smtpPassword = "pass";
            settings.reportSenderEmail = "reportsender@aldaviva.com";
            settings.reportRecipientEmails = new List<string> { "ben@aldaviva.com" };

            var recipients = new List<string> { "ben@aldaviva.com" };
            var report = new Report(100, new DateInterval(new LocalDate(2017, 07, 17), new LocalDate(2017, 08, 16)));

            await emailSender.SendEmail(report, recipients);

            CancellationToken cancellationToken = default;
            A.CallTo(() => smtpClient.ConnectAsync("aldaviva.com", 25, SecureSocketOptions.StartTls, cancellationToken))
                .MustHaveHappened()
                .Then(A.CallTo(() => smtpClient.AuthenticateAsync("user", "pass", cancellationToken))
                    .MustHaveHappened())
                .Then(A.CallTo(() => smtpClient.SendAsync(A<MimeMessage>.That.Matches(message =>
                    message.From.Equals(new InternetAddressList(new List<InternetAddress> { new MailboxAddress("Dad's Energy Reporter", "reportsender@aldaviva.com") }))
                        && message.To.Equals(new InternetAddressList(new List<InternetAddress> { new MailboxAddress("ben@aldaviva.com") }))
                        && message.Subject == "monthly kwh report"
                        && message.Body.ContentType.MimeType.Equals(new TextPart(TextFormat.Plain).ContentType.MimeType)
                        && message.TextBody == "you generated 100 kWh between Monday, July 17, 2017 and Wednesday, August 16, 2017."
                        ),
                    cancellationToken, default)).MustHaveHappened())
                .Then(A.CallTo(() => smtpClient.DisconnectAsync(true, cancellationToken)).MustHaveHappened());
        }

        [Fact]
        public void SendEmailFailure()
        {
            A.CallTo(() => smtpClient.ConnectAsync(A<string>._, A<int>._, A<SecureSocketOptions>._, default(CancellationToken))).ThrowsAsync(new IOException());
            
            Settings settings = Settings.Default;
            settings.smtpHost = "aldaviva.com";
            settings.smtpPort = 25;

            var recipients = new List<string> { "ben@aldaviva.com" };
            var report = new Report(100, new DateInterval(new LocalDate(2017, 07, 17), new LocalDate(2017, 08, 16)));
            
            Func<Task> thrower = async () => await emailSender.SendEmail(report, recipients);

            thrower.ShouldThrow<EmailException>().WithMessage("Failed to send email message");
        }
    }
}