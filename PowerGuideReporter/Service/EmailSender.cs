using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PowerGuideReporter.Data;
using PowerGuideReporter.Injection;

namespace PowerGuideReporter.Service
{
    internal interface EmailSender
    {
        Task SendEmail(Report report, IEnumerable<string> recipients);
    }

    [Component]
    internal class EmailSenderImpl : EmailSender
    {
        public async Task SendEmail(Report report, IEnumerable<string> recipients)
        {
            foreach (string recipient in recipients)
            {
                Console.WriteLine($"sending {report.Subject} to {recipient} with body \"{report.Body}\"");
            }
        }
    }
}