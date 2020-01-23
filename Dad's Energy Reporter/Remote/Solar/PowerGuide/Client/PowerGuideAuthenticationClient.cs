using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Remote.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DadsEnergyReporter.Remote.Solar.PowerGuide.Client {

    public interface PowerGuideAuthenticationClient {

        Task logOut();

        Task<PreLogInData> fetchPreLogInData();

        Task<IDictionary<string, string>>
            submitCredentials(string username, string password, PreLogInData preLogInData);

        Task<PowerGuideAuthToken> fetchAuthToken(IDictionary<string, string> credentialResponse);

    }

    /**
     * OAuth API is only available for partners of SolarCity, so we're using web authentication instead
     * TODO getting some TLS error, we might need to change the URLs around a little. the overall flow looks the same.
     */
    internal class PowerGuideAuthenticationClientImpl: AbstractResource, PowerGuideAuthenticationClient {

        public PowerGuideAuthenticationClientImpl(PowerGuideClientImpl client): base(client.apiClient) { }

        public async Task logOut() {
            UriBuilder requestUri = PowerGuideClientImpl.apiRoot;
            requestUri.Path = "Logout.aspx";

            try {
                using HttpResponseMessage response = await httpClient.GetAsync(requestUri.Uri);
                response.EnsureSuccessStatusCode();
            } catch (HttpRequestException e) {
                throw new PowerGuideException("Failed to log out", e);
            }
        }

        public async Task<PreLogInData> fetchPreLogInData() {
            UriBuilder requestUri = PowerGuideClientImpl.apiRoot;
            requestUri.Path = "/";

            try {
                using HttpResponseMessage response = await httpClient.GetAsync(requestUri.Uri);
                using IHtmlDocument html = await readContentAsHtml(response);
                string modelJson = html.QuerySelector("script#modelJson").FirstChild.NodeValue.Trim();
                string decodedModelJson = WebUtility.HtmlDecode(modelJson);
                var model = JsonConvert.DeserializeObject<JObject>(decodedModelJson);

                var siteUrl = new Uri((string) model["siteUrl"]);
                var logInUrl = new Uri(siteUrl, (string) model["loginUrl"]);
                string csrfToken = (string) model["antiForgery"]["value"];

                return new PreLogInData {
                    logInUri = logInUrl,
                    csrfToken = csrfToken
                };
            } catch (HttpRequestException e) {
                throw new PowerGuideException("Auth Phase 1/3: Failed to fetch pre-log-in data", e);
            }
        }

        public async Task<IDictionary<string, string>> submitCredentials(string username, string password,
            PreLogInData preLogInData) {
            IEnumerable<KeyValuePair<string, string>> formValues = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("idsrv.xsrf", preLogInData.csrfToken),
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("RecaptchaResponse", string.Empty)
            };

            try {
                using HttpResponseMessage response =
                    await httpClient.PostAsync(preLogInData.logInUri, new FormUrlEncodedContent(formValues));
                using IHtmlDocument html = await readContentAsHtml(response);
                return html.QuerySelectorAll("input[name]").ToDictionary(
                    e => e.GetAttribute("name"),
                    e => e.GetAttribute("value"));
            } catch (HttpRequestException e) {
                throw new PowerGuideException(
                    "Auth Phase 2/3: Failed to log in with credentials, username or password may be incorrect.", e);
            }
        }

        public async Task<PowerGuideAuthToken> fetchAuthToken(IDictionary<string, string> credentialResponse) {
            UriBuilder requestUri = PowerGuideClientImpl.apiRoot;
            requestUri.Path = "/";

            try {
                using HttpResponseMessage response = await httpClient.PostAsync(requestUri.Uri,
                    new FormUrlEncodedContent(credentialResponse.ToList()));
                response.EnsureSuccessStatusCode();
                Cookie fedAuth = apiClient.cookies.GetCookies(requestUri.Uri)["FedAuth"];

                if (fedAuth == null) {
                    throw new PowerGuideException(
                        $"Auth Phase 3/3: No FedAuth cookie was set while fetching auth token from {requestUri.Uri}");
                }

                return new PowerGuideAuthToken(fedAuth.Value);
            } catch (HttpRequestException e) {
                throw new PowerGuideException("Auth Phase 3/3: Failed to fetch auth token based on credential response",
                    e);
            }
        }

    }

}