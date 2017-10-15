using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Remote.Common;

namespace DadsEnergyReporter.Remote.OrangeRockland.Client
{
    public interface OrangeRocklandAuthenticationClient
    {
        Task<OrangeRocklandAuthToken> SubmitCredentials(string username, string password);

        Task LogOut();
    }

    internal class OrangeRocklandAuthenticationClientImpl : AbstractResource, OrangeRocklandAuthenticationClient
    {
        private const string AUTH_COOKIE_NAME = "LogCOOKPl95FnjAT";
        private readonly OrangeRocklandClientImpl client;

        public OrangeRocklandAuthenticationClientImpl(OrangeRocklandClientImpl client) : base(client.ApiClient)
        {
            this.client = client;
        }

        public async Task LogOut()
        {
            UriBuilder uri = OrangeRocklandClientImpl.ApiRoot
                .WithPathSegment("logoff.aspx");

            try
            {
                using (HttpResponseMessage response = await HttpClient.GetAsync(uri.Uri))
                {
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (HttpRequestException e)
            {
                throw new OrangeRocklandException("Failed to log out", e);
            }
        }

        public async Task<OrangeRocklandAuthToken> SubmitCredentials(string username, string password)
        {
            UriBuilder uri = OrangeRocklandClientImpl.ApiRoot
                .WithPathSegment("login.aspx");

            IDictionary<string, string> hiddenLoginFormData;
            try
            {
                hiddenLoginFormData = await client.FetchHiddenFormData(uri.Uri);
            }
            catch (HttpRequestException e)
            {
                throw new OrangeRocklandException("Auth Phase 1/2: Failed to fetch pre-log-in data", e);
            }

            var formValues = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("txtUsername", username),
                new KeyValuePair<string, string>("txtPassword", password),
                new KeyValuePair<string, string>("imgGo.x", "1"),
                new KeyValuePair<string, string>("imgGo.y", "1")
            };
            formValues.AddRange(hiddenLoginFormData);

            try
            {
                using (HttpResponseMessage response =
                    await HttpClient.PostAsync(uri.Uri, new FormUrlEncodedContent(formValues)))
                {
                    response.EnsureSuccessStatusCode();
                    Cookie logInCookie = ApiClient.Cookies.GetCookies(uri.Uri)[AUTH_COOKIE_NAME];

                    if (logInCookie == null)
                    {
                        throw new OrangeRocklandException(
                            "Auth Phase 2/2: No LogCOOKPl95FnjAT cookie was set after submitting credentials, username or password may be incorrect.");
                    }

                    return new OrangeRocklandAuthToken { LogInCookie = logInCookie.Value };
                }
            }
            catch (HttpRequestException e)
            {
                throw new OrangeRocklandException(
                    "Auth Phase 2/2: Failed to log in with credentials, Orange and Rockland site may be unavailable.",
                    e);
            }
        }
    }
}