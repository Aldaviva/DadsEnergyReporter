using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;

namespace PowerGuideReporter.Service.Remote
{
    public static class HttpClientExtensions
    {
        

        /*public static async Task<T> ReadJsonAs<T>(this HttpContent httpContent)
        {
            
        }

        public static async Task<XDocument> ReadAsXml(this HttpContent httpContent)
        {
            
        }

        public static async Task<IHtmlDocument> ReadAsHtml(this HttpContent httpContent)
        {
            using (Stream responseStream = await httpContent.ReadAsStreamAsync())
            {
                return await HTML_PARSER.ParseAsync(responseStream);
            }
        }*/
    }

    /*public class XPathTestMain
    {
        public static void Main2(string[] args)
        {
            XDocument document = XDocument.Load("file:///c:/users/ben/desktop/mysolarcity.html");
            using (XmlReader xmlReader = document.CreateReader())
            {
                XPathDocument xdoc = new XPathDocument(xmlReader);
                XPathNavigator nav = xdoc.CreateNavigator();
                XPathNodeIterator matches = nav.Select("//script[@id='modelJson']");
                foreach (object match in matches)
                {
                    Console.WriteLine("match: "+match);
                }
            }
        }

        public static void Main(string[] args)
        {
            FileStream fileStream = new FileStream("c:/users/ben/desktop/mysolarcity.html", FileMode.Open, FileAccess.Read);
            HtmlParser htmlParser = new HtmlParser();
            IHtmlDocument doc = htmlParser.Parse(fileStream);

            string modelJson = HttpUtility.HtmlDecode(doc.QuerySelector("script#modelJson").Te.Trim());
            JObject model = JsonConvert.DeserializeObject<JObject>(modelJson);
            Uri loginUrl = new Uri(new Uri((string)model["siteUrl"]), (string)model["loginUrl"]);
            string csrfToken = (string) model["antiForgery"]["value"];
            Console.WriteLine($"loginUrl = {loginUrl}");
            Console.WriteLine($"csrfToken = {csrfToken}");
        }
    }*/
}
