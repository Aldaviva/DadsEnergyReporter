using System.Threading.Tasks;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Remote.OrangeRockland.Client;
using NLog;

namespace DadsEnergyReporter.Remote.OrangeRockland.Service
{
    public interface OrangeRocklandAuthenticationService
    {
        string Username { get; set; }
        string Password { set; }
        
        Task<OrangeRocklandAuthToken> GetAuthToken();
        Task LogOut();
    }

    [Component]
    public class OrangeRocklandAuthenticationServiceImpl : OrangeRocklandAuthenticationService
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        
        private readonly OrangeRocklandClient client;

        public string Username { get; set; }
        public string Password { private get; set; }
        
        private OrangeRocklandAuthToken _authToken;

        public OrangeRocklandAuthenticationServiceImpl(OrangeRocklandClient client)
        {
            this.client = client;
        }

        public async Task<OrangeRocklandAuthToken> GetAuthToken()
        {
            return _authToken ?? (_authToken = await LogIn());
        }

        private async Task<OrangeRocklandAuthToken> LogIn()
        {
            LOGGER.Debug("Logging in to Orange & Rockland as {0}", Username);
            OrangeRocklandAuthToken authToken = await client.OrangeRocklandAuthenticationClient.SubmitCredentials(Username, Password);
            LOGGER.Debug("Logged into Orange & Rockland");
            return authToken;
        }

        public async Task LogOut()
        {
            if (_authToken != null)
            {
                try
                {
                    await client.OrangeRocklandAuthenticationClient.LogOut();
                }
                catch (OrangeRocklandException)
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