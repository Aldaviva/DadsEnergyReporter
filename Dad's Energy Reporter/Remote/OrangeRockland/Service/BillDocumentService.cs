using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using AngleSharp.Dom.Html;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Remote.OrangeRockland.Client;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using NLog;
using NodaTime;
using NodaTime.Text;

namespace DadsEnergyReporter.Remote.OrangeRockland.Service
{
    public interface BillDocumentService
    {
        /// <summary>
        /// Download the amount of energy that was bought or sold from Orange &amp; Rockland. When the solar panels
        /// generate more electricity than the house uses in a month, the surplus electricity is sold to ORU, and the
        /// bill PDF shows a negative number of kilowatt-hours.
        /// </summary>
        /// <param name="billingIntervalEndDate">Last day of the billing cycle</param>
        /// <returns>Number of kilowatt-hours. Positive means bought from utility, negative means sold to utility.</returns>
        Task<int> FetchEnergyPurchasedOrSoldKWh(LocalDate billingIntervalEndDate);
    }

    //TODO create unit tests for this class
    [Component]
    internal class BillDocumentServiceImpl : BillDocumentService
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        private static readonly ITextExtractionStrategy TEXT_EXTRACTION_STRATEGY = new SimpleTextExtractionStrategy();
        private static readonly Regex USAGE_PATTERN = new Regex(@"Total Usage KWH \d+ Days (-?\d+)", RegexOptions.IgnoreCase);
        private static readonly LocalDatePattern BILL_DATE_PATTERN = LocalDatePattern.CreateWithCurrentCulture("MM-dd-uuuu");
        
        private readonly OrangeRocklandClient client;

        public BillDocumentServiceImpl(OrangeRocklandClient client)
        {
            this.client = client;
        }

        public async Task<int> FetchEnergyPurchasedOrSoldKWh(LocalDate billingIntervalEndDate)
        {
            LOGGER.Info("Fetching index of bill PDFs");
            IHtmlDocument billDocumentIndex = await client.BillDocuments.FetchBillDocumentIndex();
            IEnumerable<BillDocument> billDocuments = FindBillDocuments(billDocumentIndex);
            BillDocument billDocument = FindBillDocumentForBillingInterval(billDocuments, billingIntervalEndDate);
            LOGGER.Info("Downloading bill PDF from {0}", billDocument.PublishingDate);
            using (Stream documentContents = await client.BillDocuments.FetchBillDocument(billDocument))
            {
                return ExtractEnergyPurchasedOrSold(billDocument, documentContents);
            }
        }

        internal static IEnumerable<BillDocument> FindBillDocuments(IHtmlDocument billDocumentIndex)
        {
            var baseUri = new Uri(billDocumentIndex.BaseUri);
            return billDocumentIndex.QuerySelectorAll("a[href *= 'ViewBillDisplay']").Select(aEl =>
            {
                var href = new Uri(baseUri, aEl.GetAttribute("href"));
                return new BillDocument
                {
                    AccountId = long.Parse(HttpUtility.ParseQueryString(href.Query)["acct_no"]),
                    PublishingDate = BILL_DATE_PATTERN.Parse(aEl.TextContent.Trim()).Value
                };
            });
        }

        internal static BillDocument FindBillDocumentForBillingInterval(IEnumerable<BillDocument> billDocuments,
            LocalDate billingIntervalEndDate)
        {
            return billDocuments.Where(published => billingIntervalEndDate <= published.PublishingDate).Min();
        }

        internal static int ExtractEnergyPurchasedOrSold(BillDocument billDocument, Stream documentContents)
        {
            using (var pdfReader = new PdfReader(documentContents))
            {
                string pageText = PdfTextExtractor.GetTextFromPage(pdfReader, 1, TEXT_EXTRACTION_STRATEGY);
                Match match = USAGE_PATTERN.Match(pageText);
                if (!match.Success)
                {
                    throw new OrangeRocklandException(
                        $"Could not parse Total Usage KWH from bill published on {billDocument.PublishingDate:d}");
                }

                return int.Parse(match.Groups[1].Value);
            }
        }
    }
}