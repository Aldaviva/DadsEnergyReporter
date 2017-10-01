using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Remote.Common;

namespace DadsEnergyReporter.Remote.OrangeRockland.Client
{
    public interface GreenButtonClient
    {
        Task<XDocument> FetchGreenButtonData();
    }

    internal class GreenButtonClientImpl : AbstractResource, GreenButtonClient
    {
        public GreenButtonClientImpl(OrangeRocklandClientImpl client) : base(client.ApiClient) { }

        public async Task<XDocument> FetchGreenButtonData()
        {
            UriBuilder uri = OrangeRocklandClientImpl.ApiRoot
                .WithPathSegment("Billing")
                .WithPathSegment("GreenButtonData.aspx");

            HttpContent formValues = new FormUrlEncodedContent(
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("OptEnergy", "E"),
                    new KeyValuePair<string, string>("optFileFormat", "XML")
                });

            try
            {
                using (HttpResponseMessage response = await HttpClient.PostAsync(uri.Uri, formValues))
                {
                    return await ReadContentAsXml(response);
                }
            }
            catch (HttpRequestException e)
            {
                throw new OrangeRocklandException("Failed to download Green Button data.", e);
            }
        }
    }
}