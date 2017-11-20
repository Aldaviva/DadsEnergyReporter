using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Remote.Common;
using NodaTime;
using NodaTime.Text;

namespace DadsEnergyReporter.Remote.OrangeRockland.Client
{
    public interface BillDocumentClient
    {
        Task<IHtmlDocument> FetchBillDocumentIndex();

        Task<Stream> FetchBillDocument(BillDocument billDocument);
    }

    public struct BillDocument
    {
        public long AccountId;
        public LocalDate PublishingDate;
    }
    
    internal class BillDocumentClientImpl : AbstractResource, BillDocumentClient
    {
        public BillDocumentClientImpl(OrangeRocklandClientImpl client) : base(client.ApiClient)
        {
        }

        public async Task<IHtmlDocument> FetchBillDocumentIndex()
        {
            UriBuilder uri = OrangeRocklandClientImpl.ApiRoot
                .WithPathSegment("Billing")
                .WithPathSegment("ViewBills.aspx");

            try
            {
                using (HttpResponseMessage response = await HttpClient.GetAsync(uri.Uri))
                {
                    return await ReadContentAsHtml(response);
                }
            }
            catch (HttpRequestException e)
            {
                throw new OrangeRocklandException("Failed to fetch list of bill documents", e);
            }
        }

        public async Task<Stream> FetchBillDocument(BillDocument billDocument)
        {
            UriBuilder uri = OrangeRocklandClientImpl.ApiRoot
                .WithPathSegment("..")
                .WithPathSegment("ViewBillImage.aspx")
                .WithParameter("acct", billDocument.AccountId.ToString())
                .WithParameter("bill_dt", LocalDatePattern.Iso.Format(billDocument.PublishingDate))
                .WithParameter("loc", "app")
                .WithParameter("cd_co", "9");
                
            try
            {
                HttpResponseMessage response = await HttpClient.GetAsync(uri.Uri);
                // don't close the response because iTextSharp needs the stream to stay open
                return await response.Content.ReadAsStreamAsync();
            }
            catch (HttpRequestException e)
            {
                throw new OrangeRocklandException($"Failed to download bill document from {billDocument.PublishingDate:d}", e);
            }
        }
    }

}