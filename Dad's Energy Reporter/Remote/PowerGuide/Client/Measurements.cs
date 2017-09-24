using System;
using System.Net.Http;
using System.Threading.Tasks;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Remote.Common;
using NodaTime;

namespace DadsEnergyReporter.Remote.PowerGuide.Client
{
    public interface Measurements
    {
        Task<MeasurementsResponse> FetchMeasurements(Guid installationGuid, ZonedDateTime startTime,
            ZonedDateTime endTime);
    }

    internal class MeasurementsImpl : AbstractResource, Measurements
    {
        /**
         * https://api.solarcity.com/powerguide/Help/Api/GET-v1.0-measurements-InstallationGUID_Period_StartTime_EndTime_IsByDevice_IncludeCurrent/
         */
        public MeasurementsImpl(PowerGuideClientImpl client) : base(client.ApiClient) { }

        public async Task<MeasurementsResponse> FetchMeasurements(Guid installationGuid, ZonedDateTime startTime,
            ZonedDateTime endTime)
        {
            UriBuilder uri = PowerGuideClientImpl.ApiRoot;
            uri.Path += "measurements/"
                        + installationGuid;
            uri.Query = $"StartTime={PowerGuideClientImpl.FormatDate(startTime)}" +
                        $"&EndTime={PowerGuideClientImpl.FormatDate(endTime)}" +
                        "&Period=Day"; //https://api.solarcity.com/powerguide/Help/ResourceModel?modelName=Period

            try
            {
                using (HttpResponseMessage response = await HttpClient.GetAsync(uri.Uri))
                {
                    return await ReadContentJsonAs<MeasurementsResponse>(response);
                }
            }
            catch (HttpRequestException e)
            {
                throw new PowerGuideException("Failed to get solar output measurements", e);
            }
        }
    }
}