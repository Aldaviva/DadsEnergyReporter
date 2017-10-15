using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Remote.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DadsEnergyReporter.Remote.PowerGuide.Client
{
    public interface PowerGuideAuthenticationClient
    {
        Task LogOut();
        Task<PreLogInData> FetchPreLogInData();

        Task<IDictionary<string, string>>
            SubmitCredentials(string username, string password, PreLogInData preLogInData);

        Task<PowerGuideAuthToken> FetchAuthToken(IDictionary<string, string> credentialResponse);
    }

    /**
     * OAuth API is only available for partners of SolarCity, so we're using web authentication instead
     */
    internal class PowerGuideAuthenticationClientImpl : AbstractResource, PowerGuideAuthenticationClient
    {
        public PowerGuideAuthenticationClientImpl(PowerGuideClientImpl client) : base(client.ApiClient)
        {
        }

        public async Task LogOut()
        {
            UriBuilder requestUri = PowerGuideClientImpl.ApiRoot;
            requestUri.Path = "Logout.aspx";

            try
            {
                using (HttpResponseMessage request = await HttpClient.GetAsync(requestUri.Uri))
                {
                    request.EnsureSuccessStatusCode();
                }
            }
            catch (HttpRequestException e)
            {
                throw new PowerGuideException("Failed to log out", e);
            }
        }

        public async Task<PreLogInData> FetchPreLogInData()
        {
            UriBuilder requestUri = PowerGuideClientImpl.ApiRoot;
            requestUri.Path = "/";

            try
            {
                using (HttpResponseMessage response = await HttpClient.GetAsync(requestUri.Uri))
                using (IHtmlDocument html = await ReadContentAsHtml(response))
                {
                    string modelJson = html.QuerySelector("script#modelJson").FirstChild.NodeValue.Trim();
                    string decodedModelJson = WebUtility.HtmlDecode(modelJson);
                    var model = JsonConvert.DeserializeObject<JObject>(decodedModelJson);

                    var siteUrl = new Uri((string) model["siteUrl"]);
                    var logInUrl = new Uri(siteUrl, (string) model["loginUrl"]);
                    string csrfToken = (string) model["antiForgery"]["value"];

                    return new PreLogInData
                    {
                        LogInUri = logInUrl,
                        CsrfToken = csrfToken
                    };
                }
            }
            catch (HttpRequestException e)
            {
                throw new PowerGuideException("Auth Phase 1/3: Failed to fetch pre-log-in data", e);
            }
        }

        public async Task<IDictionary<string, string>> SubmitCredentials(string username, string password,
            PreLogInData preLogInData)
        {
            IEnumerable<KeyValuePair<string, string>> formValues = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("idsrv.xsrf", preLogInData.CsrfToken),
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("RecaptchaResponse", string.Empty)
            };

            try
            {
                using (HttpResponseMessage response =
                    await HttpClient.PostAsync(preLogInData.LogInUri, new FormUrlEncodedContent(formValues)))
                using (IHtmlDocument html = await ReadContentAsHtml(response))
                {
                    return html.QuerySelectorAll("input[name]").ToDictionary(
                        e => e.GetAttribute("name"),
                        e => e.GetAttribute("value"));
                }
            }
            catch (HttpRequestException e)
            {
                throw new PowerGuideException(
                    "Auth Phase 2/3: Failed to log in with credentials, username or password may be incorrect.", e);
            }
        }

        public async Task<PowerGuideAuthToken> FetchAuthToken(IDictionary<string, string> credentialResponse)
        {
            UriBuilder requestUri = PowerGuideClientImpl.ApiRoot;
            requestUri.Path = "/";

            try
            {
                using (HttpResponseMessage response = await HttpClient.PostAsync(requestUri.Uri,
                    new FormUrlEncodedContent(credentialResponse.ToList())))
                {
                    response.EnsureSuccessStatusCode();
                    Cookie fedAuth = ApiClient.Cookies.GetCookies(requestUri.Uri)["FedAuth"];

                    if (fedAuth == null)
                    {
                        throw new PowerGuideException(
                            $"Auth Phase 3/3: No FedAuth cookie was set while fetching auth token from {requestUri.Uri}");
                    }

                    return new PowerGuideAuthToken(fedAuth.Value);
                }
            }
            catch (HttpRequestException e)
            {
                throw new PowerGuideException("Auth Phase 3/3: Failed to fetch auth token based on credential response",
                    e);
            }
        }
    }
}