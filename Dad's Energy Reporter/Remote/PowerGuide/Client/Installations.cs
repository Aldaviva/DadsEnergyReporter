using System;
using System.Net.Http;
using System.Threading.Tasks;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Remote.Common;

namespace DadsEnergyReporter.Remote.PowerGuide.Client
{
    public interface Installations
    {
        Task<Guid> FetchInstallationId();
    }

    internal class InstallationsImpl : AbstractResource, Installations
    {
        public InstallationsImpl(PowerGuideClientImpl client) : base(client.ApiClient) { }

        public async Task<Guid> FetchInstallationId()
        {
            UriBuilder uri = PowerGuideClientImpl.ApiRoot;
            uri.Path += "installations";

            try
            {
                using (HttpResponseMessage response = await HttpClient.GetAsync(uri.Uri))
                {
                    InstallationsResponse installationsResponse = await ReadContentJsonAs<InstallationsResponse>(response);
                    return installationsResponse.Data[0].Guid;
                }
            }
            catch (HttpRequestException e)
            {
                throw new PowerGuideException("Failed to fetch installation ID of the solar panels at the house", e);
            }
        }
    }
}
