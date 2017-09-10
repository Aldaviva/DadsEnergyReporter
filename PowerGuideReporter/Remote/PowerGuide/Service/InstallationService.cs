using System;
using System.Threading.Tasks;
using PowerGuideReporter.Injection;
using PowerGuideReporter.Remote.PowerGuide.Client;

namespace PowerGuideReporter.Remote.PowerGuide.Service
{
    public interface InstallationService
    {
        Task<Guid> FetchInstallationId();
    }
    
    [Component]
    public class InstallationServiceImpl : InstallationService
    {
        private readonly PowerGuideClient client;
        
        public InstallationServiceImpl(PowerGuideClient client)
        {
            this.client = client;
        }

        public Task<Guid> FetchInstallationId()
        {
            return client.Installations.FetchInstallationId();
        }
    }
}