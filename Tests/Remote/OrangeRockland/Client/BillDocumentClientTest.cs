using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Remote.Common;
using FakeItEasy;
using FluentAssertions;
using NodaTime;
using Xunit;

namespace DadsEnergyReporter.Remote.OrangeRockland.Client
{
    public class BillDocumentClientTest
    {
        private readonly BillDocumentClient billDocumentClient;
        private readonly ApiClient apiClient = A.Fake<ApiClient>();
        private readonly FakeHttpMessageHandler httpMessageHander = A.Fake<FakeHttpMessageHandler>();
        private readonly ContentHandlers contentHandlers = A.Fake<ContentHandlers>();

        public BillDocumentClientTest()
        {
            var client = A.Fake<OrangeRocklandClientImpl>();
            A.CallTo(() => client.ApiClient).Returns(apiClient);

            billDocumentClient = new BillDocumentClientImpl(client);
            A.CallTo(() => apiClient.HttpClient).Returns(new HttpClient(httpMessageHander));
            A.CallTo(() => apiClient.ContentHandlers).Returns(contentHandlers);
        }

        [Fact]
        public async void FetchBillDocumentIndex()
        {
            var response = A.Fake<HttpResponseMessage>();
            var htmlResponse = A.Fake<IHtmlDocument>();
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._)).Returns(response);
            A.CallTo(() => contentHandlers.ReadContentAsHtml(response)).Returns(htmlResponse);

            IHtmlDocument actual = await billDocumentClient.FetchBillDocumentIndex();

            actual.Should().BeSameAs(htmlResponse);

            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>.That.Matches(message =>
                message.Method == HttpMethod.Get
                && message.RequestUri.ToString().Equals("https://apps.coned.com/ORMyAccount/Forms/Billing/ViewBills.aspx")
            ))).MustHaveHappened();
        }

        [Fact]
        public void FetchBillDocumentIndexFailure()
        {
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._))
                .ThrowsAsync(new HttpRequestException());

            Func<Task> thrower = async () => await billDocumentClient.FetchBillDocumentIndex();
            thrower.Should().Throw<OrangeRocklandException>()
                .WithMessage("Failed to fetch list of bill documents");
        }

        [Fact]
        public async void FetchBillDocument()
        {
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._)).Returns(new HttpResponseMessage
            {
                Content = new StringContent("hello")
            });

            var billDocument = new BillDocument
            {
                AccountId = 123,
                PublishingDate = new LocalDate(2018, 3, 31)
            };

            Stream actual = await billDocumentClient.FetchBillDocument(billDocument);

            new StreamReader(actual).ReadToEnd().Should().Be("hello");

            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>.That.Matches(message =>
                message.Method == HttpMethod.Get
                && message.RequestUri.ToString()
                    .Equals("https://apps.coned.com/ORMyAccount/ViewBillImage.aspx?acct=123&bill_dt=2018-03-31&loc=app&cd_co=9")
            ))).MustHaveHappened();
        }

        [Fact]
        public void FetchBillDocumentFailure()
        {
            A.CallTo(() => httpMessageHander.SendAsync(A<HttpRequestMessage>._))
                .ThrowsAsync(new HttpRequestException());

            Func<Task> thrower = async () => await billDocumentClient.FetchBillDocument(new BillDocument
            {
                AccountId = 123,
                PublishingDate = new LocalDate(2018, 3, 31)
            });

            thrower.Should().Throw<OrangeRocklandException>()
                .WithMessage("Failed to download bill document from 3/31/2018");
        }
    }
}