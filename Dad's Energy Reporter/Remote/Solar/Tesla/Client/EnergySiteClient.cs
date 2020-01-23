using System;
using System.Net.Http;
using System.Threading.Tasks;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Remote.Common;
using NodaTime;
using NodaTime.Text;

namespace DadsEnergyReporter.Remote.Solar.Tesla.Client {

    public interface EnergySiteClient {

        Task<HistoricalCalendarDataResponse> fetchHistoricalCalendarData(long siteId, LocalDate dayInDesiredMonth,
            DateTimeZone reportTimeZone, TeslaAuthToken authToken);

    }

    internal class EnergySiteClientImpl: AbstractResource, EnergySiteClient {

        private static readonly ZonedDateTimePattern ISO_8601_DATETIME_MILLIS_ZONE =
            ZonedDateTimePattern.CreateWithCurrentCulture(@"uuuu'-'MM'-'dd'T'HH':'mm':'ss;fffo<m>", null);

        public EnergySiteClientImpl(OwnerApiClientImpl client): base(client.apiClient) { }

        public async Task<HistoricalCalendarDataResponse> fetchHistoricalCalendarData(long siteId,
            LocalDate dayInDesiredMonth, DateTimeZone reportTimeZone, TeslaAuthToken authToken) {
            ZonedDateTime endOfMonth = getEndOfMonth(dayInDesiredMonth, reportTimeZone);
            UriBuilder uri = OwnerApiClientImpl.apiRoot
                .WithPathSegment("energy_sites")
                .WithPathSegment(siteId.ToString())
                .WithPathSegment("calendar_history")
                .WithParameter("kind", "energy")
                .WithParameter("period", "month")
                .WithParameter("end_date", ISO_8601_DATETIME_MILLIS_ZONE.Format(endOfMonth));

            try {
                HttpRequestMessage request = OwnerApiClientImpl.createRequest(HttpMethod.Get, uri, authToken);
                using HttpResponseMessage response = await httpClient.SendAsync(request);
                var historicalCalendarData = await readContentAsJson<HistoricalCalendarData>(response);
                return historicalCalendarData.response;
            } catch (HttpRequestException e) {
                throw new TeslaException($"Failed to get historical solar calendar data for the month ending on {endOfMonth}", e);
            }
        }

        private static ZonedDateTime getEndOfMonth(LocalDate dayInMonth, DateTimeZone zone) {
            return dayInMonth
                .With(DateAdjusters.EndOfMonth)
                .At(LocalTime.MaxValue)
                .InZoneLeniently(zone);
        }

    }

}