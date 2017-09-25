using DadsEnergyReporter.Remote.Common;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace DadsEnergyReporter.Remote.OrangeRockland.Client
{
    public class OrangeRocklandClientTest
    {
        private readonly OrangeRocklandClientImpl client;
        private readonly ApiClient apiClient = A.Fake<ApiClient>();

        public OrangeRocklandClientTest()
        {
            client = new OrangeRocklandClientImpl(apiClient);
        }

        [Fact]
        public void DependenciesInjected()
        {
            client.ApiClient.Should().BeSameAs(apiClient);
        }

        [Fact]
        public void ApiRoot()
        {
            OrangeRocklandClientImpl.ApiRoot.Uri.ToString().Should().Be("https://apps.coned.com/ORMyAccount/Forms");
        }

        [Fact]
        public void Resources()
        {
            client.OrangeRocklandAuthenticationClient.Should().BeOfType<OrangeRocklandAuthenticationClientImpl>();
            client.GreenButtonClient.Should().BeOfType<GreenButtonClientImpl>();
        }
    }
}