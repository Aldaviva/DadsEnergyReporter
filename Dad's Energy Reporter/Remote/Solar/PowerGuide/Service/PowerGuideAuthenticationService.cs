using System.Collections.Generic;
using System.Threading.Tasks;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Remote.Solar.PowerGuide.Client;
using NLog;

namespace DadsEnergyReporter.Remote.Solar.PowerGuide.Service {

    public interface PowerGuideAuthenticationService {

        string username { get; set; }
        string password { set; }

        Task<PowerGuideAuthToken> getAuthToken();
        Task logOut();

    }

    [Component]
    internal class PowerGuideAuthenticationServiceImpl: PowerGuideAuthenticationService {

        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        private readonly PowerGuideClient client;

        public string username { get; set; }
        public string password { private get; set; }

        private PowerGuideAuthToken _authToken;

        public PowerGuideAuthenticationServiceImpl(PowerGuideClient client) {
            this.client = client;
        }

        public async Task<PowerGuideAuthToken> getAuthToken() {
            return _authToken ?? (_authToken = await logIn());
        }

        private async Task<PowerGuideAuthToken> logIn() {
            LOGGER.Debug("Logging in to MySolarCity as {0}", username);
            PreLogInData preLogInData = await client.authentication.fetchPreLogInData();
            IDictionary<string, string> credentialResponseParams =
                await client.authentication.submitCredentials(username, password, preLogInData);
            PowerGuideAuthToken authToken = await client.authentication.fetchAuthToken(credentialResponseParams);
            LOGGER.Debug("Logged in to MySolarCity");
            LOGGER.Trace($"MySolarCity auth token: {authToken}");
            return authToken;
        }

        public async Task logOut() {
            if (_authToken != null) {
                try {
                    await client.authentication.logOut();
                } catch (PowerGuideException) {
                    //proceed even if log out fails, because what else can we do?
                }
            }

            _authToken = null;
            username = null;
            password = null;
        }

    }

}