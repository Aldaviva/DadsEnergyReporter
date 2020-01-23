using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Remote.Common;
using Newtonsoft.Json.Linq;
using NodaTime;

namespace DadsEnergyReporter.Remote.Solar.Tesla.Client {

    public interface OwnerApiAuthenticationClient {

        Task logOut(TeslaAuthToken authToken);

        Task<TeslaAuthToken> fetchAuthToken(string username, string password);

    }

    internal class OwnerApiAuthenticationClientImpl: AbstractResource, OwnerApiAuthenticationClient {

        private const string CLIENT_ID = "81527cff06843c8634fdc09e8ac0abefb46ac849f38fe1e431c2ef2106796384";
        private const string CLIENT_SECRET = "c7257eb71a564034f9419ee651c7d0e5f7aa6bfbd18bafb5c5c033b093bb2fa3";

        public OwnerApiAuthenticationClientImpl(OwnerApiClientImpl client): base(client.apiClient) { }

        public async Task logOut(TeslaAuthToken authToken) {
            UriBuilder requestUri = OwnerApiClientImpl.apiRoot;
            requestUri.Path = "/oauth/revoke";

            var requestBody = new Dictionary<string, string> {
                { "token", authToken.accessToken }
            };

            try {
                using HttpResponseMessage response = await httpClient.PostAsync(requestUri.Uri, new JsonEncodedContent(requestBody));
                response.EnsureSuccessStatusCode();
            } catch (HttpRequestException e) {
                throw new TeslaException("Failed to log out", e);
            }
        }

        public async Task<TeslaAuthToken> fetchAuthToken(string username, string password) {
            UriBuilder requestUri = OwnerApiClientImpl.apiRoot;
            requestUri.Path = "/oauth/token";

            var requestBody = new Dictionary<string, string> {
                { "email", username },
                { "password", password },
                { "grant_type", "password" },
                { "client_id", CLIENT_ID },
                { "client_secret", CLIENT_SECRET }
            };

            try {
                using HttpResponseMessage response = await httpClient.PostAsync(requestUri.Uri, new JsonEncodedContent(requestBody));
                var responseBody = await readContentAsJson<JObject>(response);
                Instant now = SystemClock.Instance.GetCurrentInstant();

                var authToken = new TeslaAuthToken {
                    accessToken = responseBody["access_token"].ToObject<string>(),
                    tokenType = responseBody["token_type"].ToObject<string>(),
                    expiration = now + Duration.FromSeconds(responseBody["expires_in"].ToObject<long>()),
                    creation = Instant.FromUnixTimeSeconds(responseBody["created_at"].ToObject<long>())
                };
                return authToken;
            } catch (HttpRequestException e) {
                throw new TeslaException("Failed to log in with credentials, username or password may be incorrect.", e);
            }
        }

    }

}