using System;
using System.Collections.Generic;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Remote.PowerGuide.Client;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace DadsEnergyReporter.Remote.PowerGuide.Service
{
    public class InstallationServiceTest
    {
        private readonly InstallationServiceImpl installationService;
        private readonly PowerGuideClient powerGuideClient = A.Fake<PowerGuideClient>();
        private readonly InstallationClient installationClient = A.Fake<InstallationClient>();

        public InstallationServiceTest()
        {
            installationService = new InstallationServiceImpl(powerGuideClient);
            A.CallTo(() => powerGuideClient.Installations).Returns(installationClient);
        }

        [Fact]
        public async void FetchInstallationId()
        {
            A.CallTo(() => installationClient.FetchInstallations()).Returns(new List<Installation>
            {
                new Installation
                {
                    Guid = new Guid("45c0c40e-a9ad-11e7-abc4-cec278b6b50a"),
//                    JobId = "abc",
//                    SystemSize = 12.1
                },
                new Installation
                {
                    Guid = new Guid("45c0c7b0-a9ad-11e7-abc4-cec278b6b50a"),
//                    JobId = "def",
//                    SystemSize = 6
                },
            });

            Guid installationId = await installationService.FetchInstallationId();

            installationId.Should().Be(new Guid("45c0c40e-a9ad-11e7-abc4-cec278b6b50a"));
        }
    }
}