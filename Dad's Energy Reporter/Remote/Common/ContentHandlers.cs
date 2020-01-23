using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using AngleSharp;
using AngleSharp.Html.Dom;
using DadsEnergyReporter.Injection;
using Newtonsoft.Json;

namespace DadsEnergyReporter.Remote.Common {

    internal interface ContentHandlers {

        Task<T> readContentAsJson<T>(HttpResponseMessage response);
        Task<XDocument> readContentAsXml(HttpResponseMessage response);
        Task<T> readContentAsXml<T>(HttpResponseMessage response);
        Task<IHtmlDocument> readContentAsHtml(HttpResponseMessage response);

    }

    [Component]
    internal class DefaultContentHandlers: ContentHandlers {

        private static readonly JsonSerializer JSON_SERIALIZER = JsonSerializer.CreateDefault();

        public async Task<T> readContentAsJson<T>(HttpResponseMessage response) {
            response.EnsureSuccessStatusCode();
            using Stream responseStream = await response.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(responseStream);
            return JSON_SERIALIZER.Deserialize<T>(new JsonTextReader(streamReader));
        }

        public async Task<XDocument> readContentAsXml(HttpResponseMessage response) {
            response.EnsureSuccessStatusCode();
            using Stream responseStream = await response.Content.ReadAsStreamAsync();
            return XDocument.Load(responseStream);
        }

        public async Task<T> readContentAsXml<T>(HttpResponseMessage response) {
            response.EnsureSuccessStatusCode();
            using Stream responseStream = await response.Content.ReadAsStreamAsync();
            var deserializer = new XmlSerializer(typeof(T));
            return (T) deserializer.Deserialize(responseStream);
        }

        public async Task<IHtmlDocument> readContentAsHtml(HttpResponseMessage response) {
            response.EnsureSuccessStatusCode();
            Stream responseStream = await response.Content.ReadAsStreamAsync();
            IBrowsingContext browsingContext = BrowsingContext.New();
            return await browsingContext.OpenAsync(virtualResponse => virtualResponse
                .Address(response.RequestMessage.RequestUri)
                .Status(response.StatusCode)
                .Headers(response.Headers.ToDictionary(pair => pair.Key, pair => pair.Value.First()))
                .Content(responseStream)
            ) as IHtmlDocument;
        }

    }

}