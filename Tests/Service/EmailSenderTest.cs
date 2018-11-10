using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DadsEnergyReporter.Data;
using DadsEnergyReporter.Exceptions;
using FakeItEasy;
using FluentAssertions;
using MailKit;
using MailKit.Security;
using MimeKit;
using NodaTime;
using Xunit;

namespace DadsEnergyReporter.Service
{
    public class EmailSenderTest
    {
        private readonly EmailSenderImpl emailSender;
        private readonly IMailTransport smtpClient = A.Fake<IMailTransport>();
        private readonly ReportFormatter reportFormatter = A.Fake<ReportFormatter>();
        private readonly Settings settings = new Settings()
        {
            SmtpHost = "aldaviva.com",
            SmtpPort = 25,
            SmtpUsername = "user",
            SmtpPassword = "pass",
            ReportSenderEmail = "reportsender@aldaviva.com",
            ReportRecipientEmails = new List<string> { "ben@aldaviva.com" }
        };

        public EmailSenderTest()
        {
            emailSender = new EmailSenderImpl(smtpClient, reportFormatter, settings);
        }

        [Fact]
        public async void SendEmail()
        {
            var recipients = new List<string> { "ben@aldaviva.com" };
            var report = new SolarAndUtilityReport(new DateInterval(new LocalDate(2017, 07, 17), new LocalDate(2017, 08, 16)), 100, 0, 2000);

            var expectedMessage = new MimeMessage
            {
                Subject = "expected message"
            };
            A.CallTo(() => reportFormatter.FormatReport(A<SolarAndUtilityReport>._)).Returns(expectedMessage);

            await emailSender.SendEmail(report, recipients);

            CancellationToken cancellationToken = default;
            A.CallTo(() => smtpClient.ConnectAsync("aldaviva.com", 25, SecureSocketOptions.StartTls, cancellationToken))
                .MustHaveHappened()
                .Then(A.CallTo(() => smtpClient.AuthenticateAsync("user", "pass", cancellationToken)).MustHaveHappened())
                .Then(A.CallTo(() => smtpClient.SendAsync(A<MimeMessage>.That.Matches(message =>
                    message.Subject == "expected message"), cancellationToken, default)).MustHaveHappened())
                .Then(A.CallTo(() => smtpClient.DisconnectAsync(true, cancellationToken)).MustHaveHappened());
        }

        [Fact]
        public void SendEmailFailure()
        {
            A.CallTo(() => smtpClient.ConnectAsync(A<string>._, A<int>._, A<SecureSocketOptions>._, default))
                .ThrowsAsync(new IOException());

            var recipients = new List<string> { "ben@aldaviva.com" };
            var report = new SolarAndUtilityReport(new DateInterval(new LocalDate(2017, 07, 17), new LocalDate(2017, 08, 16)), 100, 0, 2000);

            Func<Task> thrower = async () => await emailSender.SendEmail(report, recipients);

            thrower.Should().Throw<EmailException>().WithMessage("Failed to send email message");
        }
    }
}