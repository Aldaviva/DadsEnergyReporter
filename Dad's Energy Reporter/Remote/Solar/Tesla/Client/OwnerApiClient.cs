using System;
using System.Net.Http;
using System.Net.Http.Headers;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Remote.Common;

namespace DadsEnergyReporter.Remote.Solar.Tesla.Client {

    public interface OwnerApiClient {

        EnergySiteClient energySites { get; }
        OwnerApiAuthenticationClient authentication { get; }
        ProductClient products { get; }

    }

    [Component]
    internal class OwnerApiClientImpl: OwnerApiClient {

        internal readonly ApiClient apiClient;

        public EnergySiteClient energySites { get; }
        public OwnerApiAuthenticationClient authentication { get; }
        public ProductClient products { get; }

        internal static UriBuilder apiRoot => new UriBuilder()
            .UseHttps()
            .WithHost("owner-api.teslamotors.com")
            .WithPathSegment("api")
            .WithPathSegment("1");

        public OwnerApiClientImpl(ApiClient apiClient) {
            this.apiClient = apiClient;
            energySites = new EnergySiteClientImpl(this);
            authentication = new OwnerApiAuthenticationClientImpl(this);
            products = new ProductClientImpl(this);
        }

        internal static HttpRequestMessage createRequest(HttpMethod method, UriBuilder uri, TeslaAuthToken authToken) {
            var request = new HttpRequestMessage(method, uri.Uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken.accessToken);
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Mozilla")));
            return request;
        }

    }

}