using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Text;
using PowerGuideReporter.Injection;

namespace PowerGuideReporter.Remote.PowerGuide.Client
{
    public interface PowerGuideClient
    {
        Measurements Measurements { get; }
        Authentication Authentication { get; }
        Installations Installations { get; }
    }

    [Component]
    internal partial class PowerGuideClientImpl : PowerGuideClient
    {
        private readonly HttpClient httpClient;
        private readonly CookieContainer cookies;

        public Measurements Measurements { get; }
        public Installations Installations { get; }
        public Authentication Authentication { get; }
        internal IResponseReaders ResponseReaders { get; set; }

        private static readonly ZonedDateTimePattern ISO8601_NO_ZONE_NO_MILLIS_PATTERN = ZonedDateTimePattern.CreateWithCurrentCulture("uuuu-MM-ddTHH:mm:ss", null);
        private static UriBuilder ApiRoot => new UriBuilder("https", "mysolarcity.com", 443, "/solarcity-api/powerguide/v1.0/");

        public PowerGuideClientImpl(HttpClient httpClient, CookieContainer cookies)
        {
            this.httpClient = httpClient;
            this.cookies = cookies;

            Measurements = new MeasurementsImpl(this);
            Installations = new InstallationsImpl(this);
            Authentication = new AuthenticationImpl(this);
        }

        internal interface IResponseReaders
        {
            Task<T> ReadContentJsonAs<T>(HttpResponseMessage response);
            Task<XDocument> ReadContentAsXml(HttpResponseMessage response);
            Task<IHtmlDocument> ReadContentAsHtml(HttpResponseMessage response);
        }

        protected class DefaultResponseReaders : IResponseReaders
        {
            private static readonly JsonSerializer JSON_SERIALIZER = JsonSerializer.CreateDefault();
            private static readonly HtmlParser HTML_PARSER = new HtmlParser();

            public async Task<T> ReadContentJsonAs<T>(HttpResponseMessage response)
            {
                response.EnsureSuccessStatusCode();
                using (Stream responseStream = await response.Content.ReadAsStreamAsync())
                using (var streamReader = new StreamReader(responseStream))
                {
                    return JSON_SERIALIZER.Deserialize<T>(new JsonTextReader(streamReader));
                }
            }

            public async Task<XDocument> ReadContentAsXml(HttpResponseMessage response)
            {
                response.EnsureSuccessStatusCode();
                using (Stream responseStream = await response.Content.ReadAsStreamAsync())
                {
                    return XDocument.Load(responseStream);
                }
            }

            public async Task<IHtmlDocument> ReadContentAsHtml(HttpResponseMessage response)
            {
                response.EnsureSuccessStatusCode();
                using (Stream responseStream = await response.Content.ReadAsStreamAsync())
                {
                    return await HTML_PARSER.ParseAsync(responseStream);
                }
            }
        }

        private static string FormatDate(ZonedDateTime date)
        {
            return ISO8601_NO_ZONE_NO_MILLIS_PATTERN.Format(date);
        }

        protected abstract class Resource : IResponseReaders
        {
            private PowerGuideClientImpl Client { get; }
            protected HttpClient HttpClient => Client.httpClient;
            protected CookieContainer Cookies => Client.cookies;

            protected Resource(PowerGuideClientImpl client)
            {
                Client = client;
            }

            public Task<T> ReadContentJsonAs<T>(HttpResponseMessage response)
            {
                return Client.ResponseReaders.ReadContentJsonAs<T>(response);
            }

            public Task<XDocument> ReadContentAsXml(HttpResponseMessage response)
            {
                return Client.ResponseReaders.ReadContentAsXml(response);
            }

            public Task<IHtmlDocument> ReadContentAsHtml(HttpResponseMessage response)
            {
                return Client.ResponseReaders.ReadContentAsHtml(response);
            }
        }
    }
}
