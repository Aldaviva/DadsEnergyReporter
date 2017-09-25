using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DadsEnergyReporter.Data.Marshal;
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

        public async Task<Guid> FetchInstallationId()
        {
            IEnumerable<Installation> installations = await client.Installations.FetchInstallations();
            return installations.First().Guid;
        }
    }
}