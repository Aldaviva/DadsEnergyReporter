using System;
using System.Net.Http;
using System.Threading.Tasks;
using NodaTime;
using PowerGuideReporter.Data.Marshal;
using PowerGuideReporter.Service.Remote.Auth;

namespace PowerGuideReporter.Service.Remote
{
    public interface Measurements
    {
        Task<MeasurementsResponse> FetchMeasurements(Guid installationGuid, ZonedDateTime startTime,
            ZonedDateTime endTime);
        Task<Guid> FetchInstallationId();
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