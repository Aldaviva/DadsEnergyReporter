using System;
using System.Net.Http;
using System.Threading.Tasks;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Remote.Common;
using NodaTime;

namespace DadsEnergyReporter.Remote.Solar.PowerGuide.Client {

    public interface MeasurementClient {

        Task<MeasurementsResponse> fetchMeasurements(Guid installationGuid, ZonedDateTime startTime,
            ZonedDateTime endTime);

    }

    internal class MeasurementClientImpl: AbstractResource, MeasurementClient {

        /**
         * https://api.solarcity.com/powerguide/Help/Api/GET-v1.0-measurements-InstallationGUID_Period_StartTime_EndTime_IsByDevice_IncludeCurrent/
         */
        public MeasurementClientImpl(PowerGuideClientImpl client): base(client.apiClient) { }

        public async Task<MeasurementsResponse> fetchMeasurements(Guid installationGuid, ZonedDateTime startTime,
            ZonedDateTime endTime) {
            UriBuilder uri = PowerGuideClientImpl.apiRoot
                .WithPathSegment("measurements")
                .WithPathSegment(installationGuid.ToString())
                .WithParameter("StartTime", PowerGuideClientImpl.formatDate(startTime))
                .WithParameter("EndTime", PowerGuideClientImpl.formatDate(endTime))
                .WithParameter("Period", "Day");

            try {
                using HttpResponseMessage response = await httpClient.GetAsync(uri.Uri);
                return await readContentAsJson<MeasurementsResponse>(response);
            } catch (HttpRequestException e) {
                throw new PowerGuideException("Failed to get solar output measurements", e);
            }
        }

    }

}