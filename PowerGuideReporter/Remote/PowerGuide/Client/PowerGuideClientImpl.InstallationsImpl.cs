using System;
using System.Net.Http;
using System.Threading.Tasks;
using PowerGuideReporter.Data.Marshal;

namespace PowerGuideReporter.Remote.PowerGuide.Client
{
    public interface Installations
    {
        Task<Guid> FetchInstallationId();
    }
    
    internal partial class PowerGuideClientImpl
    {
        protected class InstallationsImpl : Resource, Installations
        {
            public InstallationsImpl(PowerGuideClientImpl client) : base(client)
            {
            }

            public async Task<Guid> FetchInstallationId()
            {
                UriBuilder uri = ApiRoot;
                uri.Path += "installations";

                try
                {
                    using (HttpResponseMessage response = await HttpClient.GetAsync(uri.Uri))
                    {
                        InstallationsResponse installationsResponse = await ReadContentJsonAs<InstallationsResponse>(response.EnsureSuccessStatusCode());
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
}
