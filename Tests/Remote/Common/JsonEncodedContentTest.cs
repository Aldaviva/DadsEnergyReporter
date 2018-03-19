using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace DadsEnergyReporter.Remote.Common
{
    public class JsonEncodedContentTest
    {
        [Fact]
        public async void Serialize()
        {
            var dictionary = new Dictionary<string, object>
            {
                { "a", 1 },
                { "b", "two" }
            };

            var jsonEncodedContent = new JsonEncodedContent(dictionary);

            jsonEncodedContent.Headers.ContentType.MediaType.Should().Be("application/json");

            string actual = await jsonEncodedContent.ReadAsStringAsync();

            actual.Should().NotBeEmpty();
            actual.Should().Be(@"{""a"":1,""b"":""two""}");
        }
    }
}