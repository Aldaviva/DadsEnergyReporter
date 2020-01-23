using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Remote.Common;
using NodaTime;
using NodaTime.Text;

namespace DadsEnergyReporter.Remote.OrangeRockland.Client {

    public interface BillDocumentClient {

        Task<IHtmlDocument> fetchBillDocumentIndex();

        Task<Stream> fetchBillDocument(BillDocument billDocument);

    }

    public struct BillDocument {

        public long accountId;
        public LocalDate publishingDate;

    }

    internal class BillDocumentClientImpl: AbstractResource, BillDocumentClient {

        public BillDocumentClientImpl(OrangeRocklandClientImpl client): base(client.apiClient) { }

        public async Task<IHtmlDocument> fetchBillDocumentIndex() {
            UriBuilder uri = OrangeRocklandClientImpl.apiRoot
                .WithPathSegment("Billing")
                .WithPathSegment("ViewBills.aspx");

            try {
                using HttpResponseMessage response = await httpClient.GetAsync(uri.Uri);
                return await readContentAsHtml(response);
            } catch (HttpRequestException e) {
                throw new OrangeRocklandException("Failed to fetch list of bill documents", e);
            }
        }

        public async Task<Stream> fetchBillDocument(BillDocument billDocument) {
            UriBuilder uri = OrangeRocklandClientImpl.apiRoot
                .WithPathSegment("..")
                .WithPathSegment("ViewBillImage.aspx")
                .WithParameter("acct", billDocument.accountId.ToString())
                .WithParameter("bill_dt", LocalDatePattern.Iso.Format(billDocument.publishingDate))
                .WithParameter("loc", "app")
                .WithParameter("cd_co", "9");

            try {
                HttpResponseMessage response = await httpClient.GetAsync(uri.Uri);
                // don't close the response because iTextSharp needs the stream to stay open
                return await response.Content.ReadAsStreamAsync();
            } catch (HttpRequestException e) {
                throw new OrangeRocklandException($"Failed to download bill document from {billDocument.publishingDate:d}", e);
            }
        }

    }

}