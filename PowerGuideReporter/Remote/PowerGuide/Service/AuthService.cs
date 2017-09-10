using System.Collections.Generic;
using System.Threading.Tasks;
using PowerGuideReporter.Data.Marshal;
using PowerGuideReporter.Injection;
using PowerGuideReporter.Remote.PowerGuide.Client;

namespace PowerGuideReporter.Remote.PowerGuide.Service
{
    internal interface AuthService
    {
        Task<AuthToken> GetAuthToken();
        Task LogOut();
        string Username { get; set; }
        string Password { set; }
    }

    [Component]
    internal class AuthServiceImpl : AuthService
    {
        private readonly PowerGuideClient client;

        public string Username { get; set; }
        public string Password { private get; set; }

        private AuthToken _authToken;

        public AuthServiceImpl(PowerGuideClient client)
        {
            this.client = client;
        }

        public async Task<AuthToken> GetAuthToken()
        {
            return _authToken ?? (_authToken = await LogIn());
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

        private async Task<AuthToken> LogIn()
        {
            PreLogInData preLogInData = await client.Authentication.FetchPreLogInData();
            IDictionary<string, string> credentialResponseParams = await client.Authentication.SubmitCredentials(Username, Password, preLogInData);
            AuthToken authToken = await client.Authentication.FetchAuthToken(credentialResponseParams);
            return authToken;
        }
    }
}