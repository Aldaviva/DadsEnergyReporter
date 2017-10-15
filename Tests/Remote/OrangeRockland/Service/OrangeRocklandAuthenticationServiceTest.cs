using System.Threading.Tasks;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Remote.OrangeRockland.Client;
using FakeItEasy;
using FluentAssertions;
using Xunit;

// ReSharper disable SuggestVarOrType_Elsewhere

namespace DadsEnergyReporter.Remote.OrangeRockland.Service
{
    public class OrangeRocklandAuthenticationServiceTest
    {
        private readonly OrangeRocklandAuthenticationServiceImpl orangeRocklandAuthenticationService;
        private readonly OrangeRocklandClient client = A.Fake<OrangeRocklandClient>();
        private readonly OrangeRocklandAuthenticationClient authClient = A.Fake<OrangeRocklandAuthenticationClient>();

        public OrangeRocklandAuthenticationServiceTest()
        {
            orangeRocklandAuthenticationService = new OrangeRocklandAuthenticationServiceImpl(client);
            A.CallTo(() => client.OrangeRocklandAuthenticationClient).Returns(authClient);
        }

        [Fact]
        public async void GetAuthToken()
        {
            var token = new OrangeRocklandAuthToken();
            var tokenTask = Task.FromResult(token);
            A.CallTo(() => authClient.SubmitCredentials(A<string>._, A<string>._))
                .Returns(tokenTask);

            orangeRocklandAuthenticationService.Username = "user";
            orangeRocklandAuthenticationService.Password = "pass";
            OrangeRocklandAuthToken actual = await orangeRocklandAuthenticationService.GetAuthToken();
            actual.Should().BeSameAs(token);

            A.CallTo(() => authClient.SubmitCredentials("user", "pass"))
                .MustHaveHappened(Repeated.Exactly.Once);

            actual = await orangeRocklandAuthenticationService.GetAuthToken();
            actual.Should().BeSameAs(token);

            A.CallTo(() => authClient.SubmitCredentials("user", "pass"))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async void LogOut()
        {
            await orangeRocklandAuthenticationService.LogOut();

            A.CallTo(() => authClient.LogOut()).MustNotHaveHappened();

            A.CallTo(() => authClient.SubmitCredentials(A<string>._, A<string>._))
                .Returns(Task.FromResult(new OrangeRocklandAuthToken()));

            orangeRocklandAuthenticationService.Username = "user";
            orangeRocklandAuthenticationService.Password = "pass";
            await orangeRocklandAuthenticationService.GetAuthToken();

            await orangeRocklandAuthenticationService.LogOut();

            A.CallTo(() => authClient.LogOut()).MustHaveHappened(Repeated.Exactly.Once);
        }
        
        [Fact]
        public async void LogOutContinuesOnException()
        {
            A.CallTo(() => authClient.SubmitCredentials(A<string>._, A<string>._))
                .Returns(Task.FromResult(new OrangeRocklandAuthToken()));
            
            orangeRocklandAuthenticationService.Username = "user";
            orangeRocklandAuthenticationService.Password = "pass";
            await orangeRocklandAuthenticationService.GetAuthToken();

            A.CallTo(() => authClient.LogOut()).Throws(new OrangeRocklandException("test"));

            Task task = orangeRocklandAuthenticationService.LogOut();
            await task;

            A.CallTo(() => authClient.LogOut()).MustHaveHappened(Repeated.Exactly.Once);

            task.IsFaulted.Should().BeFalse();
        }
    }
}