using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace DadsEnergyReporter.Remote.OrangeRockland.Service
{
    public class OrangeRocklandServiceTest
    {
        private readonly OrangeRocklandServiceImpl orangeRocklandService;
        private readonly GreenButtonService greenButtonService = A.Fake<GreenButtonService>();
        private readonly OrangeRocklandAuthenticationService orangeRocklandAuthentication = A.Fake<OrangeRocklandAuthenticationService>();
        private readonly BillDocumentService billDocumentService = A.Fake<BillDocumentService>();

        public OrangeRocklandServiceTest()
        {
            orangeRocklandService = new OrangeRocklandServiceImpl(orangeRocklandAuthentication, greenButtonService, billDocumentService);
        }

        [Fact]
        public void DependenciesInjected()
        {
            orangeRocklandService.Authentication.Should().BeSameAs(orangeRocklandAuthentication);
            orangeRocklandService.GreenButton.Should().BeSameAs(greenButtonService);
        }
    }
}