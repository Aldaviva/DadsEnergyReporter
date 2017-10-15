using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Remote.Common;

namespace DadsEnergyReporter.Remote.OrangeRockland.Client
{
    public interface OrangeRocklandClient
    {
        OrangeRocklandAuthenticationClient OrangeRocklandAuthenticationClient { get; }
        GreenButtonClient GreenButtonClient { get; }
    }
    
    [Component]
    internal class OrangeRocklandClientImpl : OrangeRocklandClient
    {
        internal virtual ApiClient ApiClient { get; }
        
        public OrangeRocklandAuthenticationClient OrangeRocklandAuthenticationClient { get; }
        public GreenButtonClient GreenButtonClient { get; }

        public static UriBuilder ApiRoot => new UriBuilder()
            .UseHttps()
            .WithHost("apps.coned.com")
            .WithPathSegment("ORMyAccount")
            .WithPathSegment("Forms");

        public OrangeRocklandClientImpl(ApiClient apiClient)
        {
            ApiClient = apiClient;
            OrangeRocklandAuthenticationClient = new OrangeRocklandAuthenticationClientImpl(this);
            GreenButtonClient = new GreenButtonClientImpl(this);
        }

        public virtual async Task<IDictionary<string, string>> FetchHiddenFormData(Uri uri)
        {
            using (HttpResponseMessage response = await ApiClient.HttpClient.GetAsync(uri))
            using (IHtmlDocument html = await ApiClient.ContentHandlers.ReadContentAsHtml(response))
            {
                return html.QuerySelectorAll("input[type=hidden]").ToDictionary(
                    e => e.GetAttribute("name"),
                    e => e.GetAttribute("value"));
            }
        }
    }
}
