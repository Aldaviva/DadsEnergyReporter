using System;
using System.Threading.Tasks;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Remote.PowerGuide.Client;

namespace DadsEnergyReporter.Remote.PowerGuide.Service
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