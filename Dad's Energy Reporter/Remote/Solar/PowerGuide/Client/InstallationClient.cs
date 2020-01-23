using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Remote.Common;

namespace DadsEnergyReporter.Remote.Solar.PowerGuide.Client {

    public interface InstallationClient {

        Task<IEnumerable<Installation>> fetchInstallations();

    }

    internal class InstallationClientImpl: AbstractResource, InstallationClient {

        public InstallationClientImpl(PowerGuideClientImpl client): base(client.apiClient) { }

        public async Task<IEnumerable<Installation>> fetchInstallations() {
            UriBuilder uri = PowerGuideClientImpl.apiRoot
                .WithPathSegment("installations");

            try {
                using HttpResponseMessage response = await httpClient.GetAsync(uri.Uri);
                var installationsResponse = await readContentAsJson<InstallationsResponse>(response);
                return installationsResponse.data;
            } catch (HttpRequestException e) {
                throw new PowerGuideException("Failed to fetch installation ID of the solar panels at the house", e);
            }
        }

    }

}