using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Remote.Common;

namespace DadsEnergyReporter.Remote.OrangeRockland.Client {

    public interface OrangeRocklandClient {

        OrangeRocklandAuthenticationClient authentication { get; }
        GreenButtonClient greenButton { get; }
        BillDocumentClient billDocuments { get; }

    }

    [Component]
    internal class OrangeRocklandClientImpl: OrangeRocklandClient {

        internal virtual ApiClient apiClient { get; }

        public OrangeRocklandAuthenticationClient authentication { get; }
        public GreenButtonClient greenButton { get; }
        public BillDocumentClient billDocuments { get; }

        public static UriBuilder apiRoot => new UriBuilder()
            .UseHttps()
            .WithHost("apps.coned.com")
            .WithPathSegment("ORMyAccount")
            .WithPathSegment("Forms");

        public OrangeRocklandClientImpl(ApiClient apiClient) {
            this.apiClient = apiClient;
            authentication = new OrangeRocklandAuthenticationClientImpl(this);
            greenButton = new GreenButtonClientImpl(this);
            billDocuments = new BillDocumentClientImpl(this);
        }

        public virtual async Task<IDictionary<string, string>> fetchHiddenFormData(Uri uri) {
            using HttpResponseMessage response = await apiClient.httpClient.GetAsync(uri);
            using IHtmlDocument html = await apiClient.contentHandlers.readContentAsHtml(response);
            return html.QuerySelectorAll("input[type=hidden]").ToDictionary(
                e => e.GetAttribute("name"),
                e => e.GetAttribute("value"));
        }

    }

}