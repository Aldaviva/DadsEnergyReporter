using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Remote.Solar.PowerGuide.Client;

namespace DadsEnergyReporter.Remote.Solar.PowerGuide.Service {

    public interface InstallationService {

        Task<Guid> fetchInstallationId();

    }

    [Component]
    public class InstallationServiceImpl: InstallationService {

        private readonly PowerGuideClient client;

        public InstallationServiceImpl(PowerGuideClient client) {
            this.client = client;
        }

        public async Task<Guid> fetchInstallationId() {
            IEnumerable<Installation> installations = await client.installations.fetchInstallations();
            return installations.First().guid;
        }

    }

}