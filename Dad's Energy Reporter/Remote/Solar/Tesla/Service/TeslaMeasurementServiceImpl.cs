using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Remote.Solar.Tesla.Client;
using NodaTime;
using Measurement = DadsEnergyReporter.Data.Measurement;

namespace DadsEnergyReporter.Remote.Solar.Tesla.Service {

    [Component]
    public class TeslaMeasurementServiceImpl: MeasurementService {

        private readonly OwnerApiClient client;
        private readonly OwnerApiAuthenticationService authService;
        private readonly ProductService productService;
        private readonly DateTimeZone reportTimeZone;

        public TeslaMeasurementServiceImpl(OwnerApiClient client, OwnerApiAuthenticationService authService,
            ProductService productService, DateTimeZone reportTimeZone) {
            this.client = client;
            this.authService = authService;
            this.productService = productService;
            this.reportTimeZone = reportTimeZone;
        }

        public async Task<Measurement> measure(DateInterval billingInterval) {
            TeslaAuthToken authToken = await authService.getAuthToken();
            long energySiteId = await productService.fetchEnergySiteId(authToken);

            ICollection<Task<HistoricalCalendarDataResponse>> requests = new List<Task<HistoricalCalendarDataResponse>> {
                client.energySites.fetchHistoricalCalendarData(energySiteId, billingInterval.Start, reportTimeZone, authToken)
            };

            if (billingInterval.Start.Month != billingInterval.End.Month) {
                requests.Add(client.energySites.fetchHistoricalCalendarData(energySiteId, billingInterval.End, reportTimeZone,
                    authToken));
            }

            HistoricalCalendarDataResponse[] responses = await Task.WhenAll(requests);
            IEnumerable<EnergyTimeSeriesEntry> timeSeries = responses.SelectMany(response => response.timeSeries);

            IEnumerable<EnergyTimeSeriesEntry> entriesInBillingInterval =
                timeSeries.Where(entry => billingInterval.Contains(entry.timestamp.Date));

            double energyExportedWattHours = entriesInBillingInterval.Aggregate(0.0, (sum, entry) => sum + entry.solarEnergyExported);

            return new Measurement { generatedKilowattHours = energyExportedWattHours / 1000 };
        }

    }

}