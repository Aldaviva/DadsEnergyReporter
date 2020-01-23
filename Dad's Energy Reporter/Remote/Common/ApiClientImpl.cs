using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using AngleSharp.Html.Dom;
using DadsEnergyReporter.Injection;

namespace DadsEnergyReporter.Remote.Common {

    internal interface ApiClient {

        ContentHandlers contentHandlers { get; set; }
        HttpClient httpClient { get; }
        CookieContainer cookies { get; }

    }

    [Component]
    internal class ApiClientImpl: ApiClient {

        public ContentHandlers contentHandlers { get; set; }
        public HttpClient httpClient { get; }
        public CookieContainer cookies { get; }

        public ApiClientImpl(HttpClient httpClient, CookieContainer cookies, ContentHandlers contentHandlers) {
            this.contentHandlers = contentHandlers;
            this.httpClient = httpClient;
            this.cookies = cookies;
        }

    }

    internal abstract class AbstractResource: ContentHandlers {

        protected ApiClient apiClient { get; }
        protected HttpClient httpClient => apiClient.httpClient;

        protected AbstractResource(ApiClient apiClient) {
            this.apiClient = apiClient;
        }

        public Task<T> readContentAsJson<T>(HttpResponseMessage response) {
            return apiClient.contentHandlers.readContentAsJson<T>(response);
        }

        public Task<XDocument> readContentAsXml(HttpResponseMessage response) {
            return apiClient.contentHandlers.readContentAsXml(response);
        }

        public Task<T> readContentAsXml<T>(HttpResponseMessage response) {
            return apiClient.contentHandlers.readContentAsXml<T>(response);
        }

        public Task<IHtmlDocument> readContentAsHtml(HttpResponseMessage response) {
            return apiClient.contentHandlers.readContentAsHtml(response);
        }

    }

}