using System.Threading.Tasks;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Remote.Solar.Tesla.Client;
using NLog;

namespace DadsEnergyReporter.Remote.Solar.Tesla.Service {

    public interface OwnerApiAuthenticationService {

        string username { get; set; }
        string password { set; }

        Task<TeslaAuthToken> getAuthToken();
        Task logOut();
    }

    [Component]
    internal class OwnerApiAuthenticationServiceImpl: OwnerApiAuthenticationService {

        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        private readonly OwnerApiClient client;

        public string username { get; set; }
        public string password { private get; set; }

        private TeslaAuthToken _authToken;

        public OwnerApiAuthenticationServiceImpl(OwnerApiClient client) {
            this.client = client;
        }

        public async Task<TeslaAuthToken> getAuthToken() {
            return _authToken ??= await logIn();
        }

        private async Task<TeslaAuthToken> logIn() {
            LOGGER.Debug("Logging in to Owner API as {0}", username);
            TeslaAuthToken authToken = await client.authentication.fetchAuthToken(username, password);
            LOGGER.Debug("Logged in to Owner API");
            LOGGER.Trace("Owner API auth token: {0}", authToken.accessToken);
            return authToken;
        }

        public async Task logOut() {
            if (_authToken != null) {
                try {
                    await client.authentication.logOut(_authToken);
                } catch (TeslaException) {
                    //proceed even if log out fails, because what else can we do?
                }
            }

            _authToken = null;
            username = null;
            password = null;
        }

    }

}