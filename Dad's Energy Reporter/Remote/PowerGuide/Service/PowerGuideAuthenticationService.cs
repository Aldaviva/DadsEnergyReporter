using System.Collections.Generic;
using System.Threading.Tasks;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Remote.PowerGuide.Client;
using NLog;

namespace DadsEnergyReporter.Remote.PowerGuide.Service
{
    public interface PowerGuideAuthenticationService
    {
        string Username { get; set; }
        string Password { set; }
        
        Task<PowerGuideAuthToken> GetAuthToken();
        Task LogOut();
    }

    [Component]
    internal class PowerGuideAuthenticationServiceImpl : PowerGuideAuthenticationService
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        
        private readonly PowerGuideClient client;

        public string Username { get; set; }
        public string Password { private get; set; }

        private PowerGuideAuthToken _authToken;

        public PowerGuideAuthenticationServiceImpl(PowerGuideClient client)
        {
            this.client = client;
        }

        public async Task<PowerGuideAuthToken> GetAuthToken()
        {
            return _authToken ?? (_authToken = await LogIn());
        }

        private async Task<PowerGuideAuthToken> LogIn()
        {
            LOGGER.Debug("Logging in to MySolarCity as {0}", Username);
            PreLogInData preLogInData = await client.Authentication.FetchPreLogInData();
            IDictionary<string, string> credentialResponseParams = await client.Authentication.SubmitCredentials(Username, Password, preLogInData);
            PowerGuideAuthToken authToken = await client.Authentication.FetchAuthToken(credentialResponseParams);
            LOGGER.Debug("Logged in to MySolarCity");
            return authToken;
        }

        public async Task LogOut()
        {
            if (_authToken != null)
            {
                try
                {
                    await client.Authentication.LogOut();
                }
                catch (PowerGuideException)
                {
                    //proceed even if log out fails, because what else can we do?
                }
            }

            _authToken = null;
            Username = null;
            Password = null;
        }
    }
}