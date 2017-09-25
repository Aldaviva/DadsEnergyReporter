using DadsEnergyReporter.Remote.Common;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace DadsEnergyReporter.Remote.PowerGuide.Client
{
    public class PowerGuideClientTest
    {
        private readonly PowerGuideClientImpl client;
        private readonly ApiClient apiClient = A.Fake<ApiClient>();

        public PowerGuideClientTest()
        {
            client = new PowerGuideClientImpl(apiClient);
        }

        [Fact]
        public void DependenciesInjected()
        {
            client.ApiClient.Should().BeSameAs(apiClient);
        }

        [Fact]
        public void ApiRoot()
        {
            PowerGuideClientImpl.ApiRoot.Uri.ToString().Should().Be("https://mysolarcity.com/solarcity-api/powerguide/v1.0");
        }

        [Fact]
        public void Resources()
        {
            client.Authentication.Should().BeOfType<PowerGuideAuthenticationClientImpl>();
            client.Installations.Should().BeOfType<InstallationClientImpl>();
            client.Measurements.Should().BeOfType<MeasurementClientImpl>();
        }
    }
}