using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace DadsEnergyReporter.Remote.Common {

    public class JsonEncodedContent: ByteArrayContent {

        private static readonly JsonSerializer SERIALIZER = new JsonSerializer();

        public JsonEncodedContent(object value): base(getContentByteArray(value)) {
            Headers.ContentType = new MediaTypeHeaderValue("application/json");
        }

        private static byte[] getContentByteArray(object value) {
            using var memoryStream = new MemoryStream();
            using var jsonTextWriter = new JsonTextWriter(new StreamWriter(memoryStream, Encoding.UTF8));
            SERIALIZER.Serialize(jsonTextWriter, value);
            jsonTextWriter.Flush();
            return memoryStream.ToArray();
        }

    }

}