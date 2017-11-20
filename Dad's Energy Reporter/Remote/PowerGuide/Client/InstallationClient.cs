using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Remote.Common;

namespace DadsEnergyReporter.Remote.PowerGuide.Client
{
    public interface InstallationClient
    {
        Task<IEnumerable<Installation>> FetchInstallations();
    }

    internal class InstallationClientImpl : AbstractResource, InstallationClient
    {
        public InstallationClientImpl(PowerGuideClientImpl client) : base(client.ApiClient)
        {
        }

        public async Task<IEnumerable<Installation>> FetchInstallations()
        {
            UriBuilder uri = PowerGuideClientImpl.ApiRoot
                .WithPathSegment("installations");

            try
            {
                using (HttpResponseMessage response = await HttpClient.GetAsync(uri.Uri))
                {
                    InstallationsResponse installationsResponse = await ReadContentAsJson<InstallationsResponse>(response);
                    return installationsResponse.Data;
                }
            }
            catch (HttpRequestException e)
            {
                throw new PowerGuideException("Failed to fetch installation ID of the solar panels at the house", e);
            }
        }
    }
}