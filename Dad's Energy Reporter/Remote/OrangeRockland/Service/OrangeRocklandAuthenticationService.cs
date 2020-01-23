using System.Threading.Tasks;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Remote.OrangeRockland.Client;
using NLog;

namespace DadsEnergyReporter.Remote.OrangeRockland.Service {

    public interface OrangeRocklandAuthenticationService {

        string username { get; set; }
        string password { set; }

        Task<OrangeRocklandAuthToken> getAuthToken();
        Task logOut();

    }

    [Component]
    public class OrangeRocklandAuthenticationServiceImpl: OrangeRocklandAuthenticationService {

        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        private readonly OrangeRocklandClient client;

        public string username { get; set; }
        public string password { private get; set; }

        private OrangeRocklandAuthToken _authToken;

        public OrangeRocklandAuthenticationServiceImpl(OrangeRocklandClient client) {
            this.client = client;
        }

        public async Task<OrangeRocklandAuthToken> getAuthToken() {
            return _authToken ?? (_authToken = await logIn());
        }

        private async Task<OrangeRocklandAuthToken> logIn() {
            LOGGER.Debug("Logging in to Orange & Rockland as {0}", username);
            OrangeRocklandAuthToken authToken = await client.authentication.submitCredentials(username, password);
            LOGGER.Debug("Logged into Orange & Rockland");
            LOGGER.Trace($"ORU auth token: {authToken}");
            return authToken;
        }

        public async Task logOut() {
            if (_authToken != null) {
                try {
                    await client.authentication.logOut();
                } catch (OrangeRocklandException) {
                    //proceed even if log out fails, because what else can we do?
                }
            }

            _authToken = null;
            username = null;
            password = null;
        }

    }

}