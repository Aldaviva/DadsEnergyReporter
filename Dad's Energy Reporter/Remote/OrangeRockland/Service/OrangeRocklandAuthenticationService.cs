using System.Collections.Generic;
using System.Threading.Tasks;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Remote.OrangeRockland.Client;

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
            IDictionary<string, string> preLogInData = await client.OrangeRocklandAuthenticationClient.FetchPreLogInData();
            OrangeRocklandAuthToken authToken = await client.OrangeRocklandAuthenticationClient.SubmitCredentials(Username, Password, preLogInData);
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