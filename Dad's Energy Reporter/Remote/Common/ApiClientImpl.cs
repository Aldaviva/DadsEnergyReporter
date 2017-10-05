using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using AngleSharp.Dom.Html;
using DadsEnergyReporter.Injection;

namespace DadsEnergyReporter.Remote.Common
{
    internal interface ApiClient
    {
        ContentHandlers ContentHandlers { get; set; }
        HttpClient HttpClient { get; }
        CookieContainer Cookies { get; }
    }

    [Component]
    internal class ApiClientImpl : ApiClient
    {
        public ContentHandlers ContentHandlers { get; set; }
        public HttpClient HttpClient { get; }
        public CookieContainer Cookies { get; }

        public ApiClientImpl(HttpClient httpClient, CookieContainer cookies, ContentHandlers contentHandlers)
        {
            ContentHandlers = contentHandlers;
            HttpClient = httpClient;
            Cookies = cookies;
        }
    }

    internal abstract class AbstractResource : ContentHandlers
    {
        protected ApiClient ApiClient { get; }
        protected HttpClient HttpClient => ApiClient.HttpClient;

        protected AbstractResource(ApiClient apiClient)
        {
            ApiClient = apiClient;
        }

        public Task<T> ReadContentAsJson<T>(HttpResponseMessage response)
        {
            return ApiClient.ContentHandlers.ReadContentAsJson<T>(response);
        }

        public Task<XDocument> ReadContentAsXml(HttpResponseMessage response)
        {
            return ApiClient.ContentHandlers.ReadContentAsXml(response);
        }

        public Task<T> ReadContentAsXml<T>(HttpResponseMessage response)
        {
            return ApiClient.ContentHandlers.ReadContentAsXml<T>(response);
        }

        public Task<IHtmlDocument> ReadContentAsHtml(HttpResponseMessage response)
        {
            return ApiClient.ContentHandlers.ReadContentAsHtml(response);
        }
    }

}
