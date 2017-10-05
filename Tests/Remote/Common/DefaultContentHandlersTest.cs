using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;
using System.Xml.Serialization;
using AngleSharp.Dom.Html;
using FluentAssertions;
using Xunit;

namespace DadsEnergyReporter.Remote.Common
{
    public class DefaultContentHandlersTest
    {
        private readonly DefaultContentHandlers contentHandlers;

        public DefaultContentHandlersTest()
        {
            contentHandlers = new DefaultContentHandlers();
        }

        [Fact]
        public async void ReadContentAsJson()
        {
            var responseMessage = new HttpResponseMessage { Content = new StringContent(@"[""a"", ""b"", ""c""]") };
            IList<string> actual = await contentHandlers.ReadContentAsJson<IList<string>>(responseMessage);
            actual.Should().Equal("a", "b", "c");
        }

        [Fact]
        public async void ReadContentAsXmlDocument()
        {
            var responseMessage =
                new HttpResponseMessage { Content = new StringContent(@"<parent><child attribute=""value"" /></parent>") };
            XDocument actual = await contentHandlers.ReadContentAsXml(responseMessage);
            XAttribute attribute = actual.Descendants(XName.Get("child")).First().Attribute(XName.Get("attribute"));
            attribute.Should().NotBeNull();
            attribute?.Value.Should().Be("value");
        }

        [Fact]
        public async void ReadContentAsXmlObject()
        {
            var responseMessage =
                new HttpResponseMessage { Content = new StringContent(@"<?xml version=""1.0""?><parent><child attribute=""value"" /></parent>") };
            Parent actual = await contentHandlers.ReadContentAsXml<Parent>(responseMessage);
            actual.Child.Attribute.Should().Be("value");
        }
        
        [Fact]
        public async void ReadContentAsHtml()
        {
            var responseMessage = new HttpResponseMessage { Content = new StringContent("<html><head></head><body><p>Hello</p></body></html>") };
            IHtmlDocument actual = await contentHandlers.ReadContentAsHtml(responseMessage);
            actual.QuerySelector("p").TextContent.Should().Be("Hello");
        }
        
        [XmlRoot(ElementName = "parent")]
        public class Parent
        {
            [XmlElement(ElementName = "child")]
            public Child Child { get; set; }
        }

        public class Child
        {
            [XmlAttribute(AttributeName = "attribute")]
            public string Attribute { get; set; }
        }
    }
}