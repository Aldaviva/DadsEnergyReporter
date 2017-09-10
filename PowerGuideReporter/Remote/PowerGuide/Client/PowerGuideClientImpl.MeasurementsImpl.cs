using System;
using System.Net.Http;
using System.Threading.Tasks;
using NodaTime;
using PowerGuideReporter.Data.Marshal;

namespace PowerGuideReporter.Remote.PowerGuide.Client
{
    public interface Measurements
    {
        Task<MeasurementsResponse> FetchMeasurements(Guid installationGuid, ZonedDateTime startTime,
            ZonedDateTime endTime);
    }

    internal partial class PowerGuideClientImpl
    {
        /**
         * https://api.solarcity.com/powerguide/Help/Api/GET-v1.0-measurements-InstallationGUID_Period_StartTime_EndTime_IsByDevice_IncludeCurrent/
         */
        protected class MeasurementsImpl : Resource, Measurements
        {

            public MeasurementsImpl(PowerGuideClientImpl client) : base(client)
            {
            }

            public async Task<MeasurementsResponse> FetchMeasurements(Guid installationGuid, ZonedDateTime startTime, ZonedDateTime endTime)
            {
                UriBuilder uri = ApiRoot;
                uri.Path += "measurements/"
                            + installationGuid;
                uri.Query = $"StartTime={FormatDate(startTime)}" +
                            $"&EndTime={FormatDate(endTime)}" +
                            "&Period=Day"; //https://api.solarcity.com/powerguide/Help/ResourceModel?modelName=Period

                try
                {
                    using (HttpResponseMessage response = await HttpClient.GetAsync(uri.Uri))
                    {
                        return await ReadContentJsonAs<MeasurementsResponse>(response.EnsureSuccessStatusCode());
                    }
                }
                catch (HttpRequestException e)
                {
                    throw new PowerGuideException("Failed to get solar output measurements", e);
                }
            }
        }
    }
}