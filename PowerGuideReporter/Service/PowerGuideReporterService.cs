using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;
using PowerGuideReporter.Data;
using PowerGuideReporter.Injection;
using PowerGuideReporter.Service.Remote.Auth;

namespace PowerGuideReporter.Service
{
    public interface PowerGuideReporterService
    {
        Task Start();
    }

    [Component]
    internal class PowerGuideReporterServiceImpl : PowerGuideReporterService
    {
        private readonly ReportGenerator _reportGenerator;
        private readonly EmailSender _emailSender;
        private readonly AuthService authService;

        public PowerGuideReporterServiceImpl(ReportGenerator reportGenerator, EmailSender emailSender, AuthService authService)
        {
            _reportGenerator = reportGenerator;
            _emailSender = emailSender;
            this.authService = authService;
        }

        public async Task Start()
        {
            Properties.Settings settings = Properties.Settings.Default;
            if (settings.username.Length != 0 && settings.password.Length != 0)
            {
                authService.Username = settings.username;
                authService.Password = settings.password;
            }
            else
            {
                throw new InvalidCredentialException("Missing username or password setting.");
            }
            await authService.GetAuthToken();

            try
            {
                Report report = await _reportGenerator.GenerateReport();
                IEnumerable<string> recipients = new List<string>
                    {
                        "ben@aldaviva.com"
                    };

                await _emailSender.SendEmail(report, recipients);
            }
            finally
            {
                await authService.LogOut();
            }
        }
    }
}
