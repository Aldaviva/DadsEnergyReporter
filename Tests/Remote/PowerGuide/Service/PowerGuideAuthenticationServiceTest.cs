using System.Collections.Generic;
using System.Threading.Tasks;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Remote.PowerGuide.Client;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace DadsEnergyReporter.Remote.PowerGuide.Service
{
    public class PowerGuideAuthenticationServiceTest
    {
        private readonly PowerGuideAuthenticationServiceImpl service;
        private readonly PowerGuideClient client = A.Fake<PowerGuideClient>();
        private readonly PowerGuideAuthenticationClient authClient = A.Fake<PowerGuideAuthenticationClient>();

        public PowerGuideAuthenticationServiceTest()
        {
            service = new PowerGuideAuthenticationServiceImpl(client);

            A.CallTo(() => client.Authentication).Returns(authClient);
        }

        [Fact]
        public async void GetAuthToken()
        {
            service.Username = "foo";
            service.Password = "bar";

            PowerGuideAuthToken actual = await MockLogIn();

            actual.Should().Be(new PowerGuideAuthToken
            {
                FedAuth = "abcdef"
            });
        }

        private Task<PowerGuideAuthToken> MockLogIn()
        {
            var preLogInData = new PreLogInData();
            A.CallTo(() => authClient.FetchPreLogInData()).Returns(preLogInData);

            IDictionary<string, string> credentialResponseParams = new Dictionary<string, string>();
            A.CallTo(() => authClient.SubmitCredentials(A<string>._, A<string>._, A<PreLogInData>._))
                .Returns(credentialResponseParams);

            
            A.CallTo(() => authClient.FetchAuthToken(A<IDictionary<string, string>>._)).Returns(new PowerGuideAuthToken
            {
                FedAuth = "abcdef"
            });

            return service.GetAuthToken();
        }

        [Fact]
        public async void LogOutSucceeds()
        {
            await MockLogIn();
            await service.LogOut();

            service.Username.Should().BeNull();
        }
        
        [Fact]
        public async void LogOutFails()
        {
            A.CallTo(() => authClient.LogOut()).ThrowsAsync(new PowerGuideException("failed to log out"));
            
            await MockLogIn();
            await service.LogOut();

            service.Username.Should().BeNull();
        }
    }
}