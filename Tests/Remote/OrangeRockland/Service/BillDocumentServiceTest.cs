using System.Collections.Generic;
using System.IO;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using DadsEnergyReporter.Remote.OrangeRockland.Client;
using FakeItEasy;
using FluentAssertions;
using NodaTime;
using Xunit;

namespace DadsEnergyReporter.Remote.OrangeRockland.Service
{
    public class BillDocumentServiceTest
    {
        private const int ACCOUNT_ID = 1234567890;
        private readonly BillDocumentServiceImpl billDocumentService;
        private readonly BillDocumentClient billDocumentClient;

        private static readonly IEnumerable<BillDocument> EXPECTED_BILL_DOCUMENTS = new List<BillDocument>
        {
            new BillDocument { AccountId = ACCOUNT_ID, PublishingDate = new LocalDate(2018, 3, 20) },
            new BillDocument { AccountId = ACCOUNT_ID, PublishingDate = new LocalDate(2018, 2, 20) },
            new BillDocument { AccountId = ACCOUNT_ID, PublishingDate = new LocalDate(2018, 1, 26) },
            new BillDocument { AccountId = ACCOUNT_ID, PublishingDate = new LocalDate(2017, 12, 18) },
            new BillDocument { AccountId = ACCOUNT_ID, PublishingDate = new LocalDate(2017, 11, 16) },
            new BillDocument { AccountId = ACCOUNT_ID, PublishingDate = new LocalDate(2017, 10, 18) },
            new BillDocument { AccountId = ACCOUNT_ID, PublishingDate = new LocalDate(2017, 9, 19) },
            new BillDocument { AccountId = ACCOUNT_ID, PublishingDate = new LocalDate(2017, 8, 17) },
            new BillDocument { AccountId = ACCOUNT_ID, PublishingDate = new LocalDate(2017, 7, 18) },
            new BillDocument { AccountId = ACCOUNT_ID, PublishingDate = new LocalDate(2017, 6, 16) },
            new BillDocument { AccountId = ACCOUNT_ID, PublishingDate = new LocalDate(2017, 5, 30) },
            new BillDocument { AccountId = ACCOUNT_ID, PublishingDate = new LocalDate(2017, 4, 18) },
            new BillDocument { AccountId = ACCOUNT_ID, PublishingDate = new LocalDate(2017, 3, 20) }
        };

        public BillDocumentServiceTest()
        {
            var orangeRocklandClient = A.Fake<OrangeRocklandClient>();
            billDocumentService = new BillDocumentServiceImpl(orangeRocklandClient);
            billDocumentClient = A.Fake<BillDocumentClient>();

            A.CallTo(() => orangeRocklandClient.BillDocuments).Returns(billDocumentClient);
        }

        [Fact]
        public void FindBillDocuments()
        {
            IHtmlDocument billDocumentIndex = new HtmlParser().Parse(File.OpenRead("Data/billDocumentIndex.html"));
            IEnumerable<BillDocument> actual = BillDocumentServiceImpl.FindBillDocuments(billDocumentIndex);
            actual.Should().BeEquivalentTo(EXPECTED_BILL_DOCUMENTS);
        }

        [Fact]
        public void FindBillDocumentForBillingInterval()
        {
            BillDocumentServiceImpl.FindBillDocumentForBillingInterval(EXPECTED_BILL_DOCUMENTS, new LocalDate(2018, 3, 18))
                .PublishingDate.Should().Be(new LocalDate(2018, 3, 20));
            BillDocumentServiceImpl.FindBillDocumentForBillingInterval(EXPECTED_BILL_DOCUMENTS, new LocalDate(2018, 3, 19))
                .PublishingDate.Should().Be(new LocalDate(2018, 3, 20));
            BillDocumentServiceImpl.FindBillDocumentForBillingInterval(EXPECTED_BILL_DOCUMENTS, new LocalDate(2018, 3, 20))
                .PublishingDate.Should().Be(new LocalDate(2018, 3, 20));
            BillDocumentServiceImpl.FindBillDocumentForBillingInterval(EXPECTED_BILL_DOCUMENTS, new LocalDate(2018, 2, 21))
                .PublishingDate.Should().Be(new LocalDate(2018, 3, 20));
            BillDocumentServiceImpl.FindBillDocumentForBillingInterval(EXPECTED_BILL_DOCUMENTS, new LocalDate(2018, 2, 20))
                .PublishingDate.Should().Be(new LocalDate(2018, 2, 20));
        }

        [Fact]
        public void ExtractEnergyPurchased()
        {
            FileStream documentContents = File.OpenRead("Data/billEnergyPurchased.pdf");
            var billDocument = new BillDocument { AccountId = ACCOUNT_ID, PublishingDate = new LocalDate(2017, 10, 18) };
            int actual = BillDocumentServiceImpl.ExtractEnergyPurchasedOrSold(billDocument, documentContents);
            actual.Should().Be(535);
        }

        [Fact]
        public void ExtractEnergySold()
        {
            FileStream documentContents = File.OpenRead("Data/billEnergySold.pdf");
            var billDocument = new BillDocument { AccountId = ACCOUNT_ID, PublishingDate = new LocalDate(2017, 9, 19) };
            int actual = BillDocumentServiceImpl.ExtractEnergyPurchasedOrSold(billDocument, documentContents);
            actual.Should().Be(-184);
        }

        [Fact]
        public async void FetchEnergyPurchasedOrSoldKWh()
        {
            A.CallTo(() => billDocumentClient.FetchBillDocumentIndex())
                .Returns(new HtmlParser().Parse(File.OpenRead("Data/billDocumentIndex.html")));
            A.CallTo(() => billDocumentClient.FetchBillDocument(A<BillDocument>._))
                .Returns(File.OpenRead("Data/billEnergyPurchased.pdf"));

            int actualPurchasedKWh = await billDocumentService.FetchEnergyPurchasedOrSoldKWh(new LocalDate(2017, 10, 18));
            actualPurchasedKWh.Should().Be(535);
        }
    }
}