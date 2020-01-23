using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Remote.Common;

namespace DadsEnergyReporter.Remote.Solar.Tesla.Client {

    public interface ProductClient {

        Task<IEnumerable<EnergySiteProduct>> fetchProducts(TeslaAuthToken authToken);

    }

    internal class ProductClientImpl: AbstractResource, ProductClient {

        public ProductClientImpl(OwnerApiClientImpl client): base(client.apiClient) { }

        public async Task<IEnumerable<EnergySiteProduct>> fetchProducts(TeslaAuthToken authToken) {
            UriBuilder uri = OwnerApiClientImpl.apiRoot
                .WithPathSegment("products");

            try {
                HttpRequestMessage request = OwnerApiClientImpl.createRequest(HttpMethod.Get, uri, authToken);
                using HttpResponseMessage response = await httpClient.SendAsync(request);

                var products = await readContentAsJson<ProductsResponse>(response);
                return products.response;
            } catch (HttpRequestException e) {
                throw new TeslaException("Failed to fetch site ID of the solar panels at the house", e);
            }

        }

    }

}