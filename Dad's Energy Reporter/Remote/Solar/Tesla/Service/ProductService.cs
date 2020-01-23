using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Remote.Solar.Tesla.Client;

namespace DadsEnergyReporter.Remote.Solar.Tesla.Service {

    public interface ProductService {

        Task<long> fetchEnergySiteId(TeslaAuthToken authToken);

    }

    [Component]
    public class ProductServiceImpl: ProductService {

        private readonly OwnerApiClient client;

        public ProductServiceImpl(OwnerApiClient client) {
            this.client = client;
        }

        public async Task<long> fetchEnergySiteId(TeslaAuthToken authToken) {
            IEnumerable<EnergySiteProduct> products = await client.products.fetchProducts(authToken);
            return products.First().energySiteId;
        }

    }

}