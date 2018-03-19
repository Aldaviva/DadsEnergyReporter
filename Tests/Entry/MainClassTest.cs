using DadsEnergyReporter.Data;
using FluentAssertions;
using Xunit;

namespace DadsEnergyReporter.Entry
{
    public class MainClassTest
    {
        [Fact]
        public void StartConsole()
        {
            Settings.SettingsManager.Filename = @"%localappdata%/Dad's Energy Reporter/test-settings.json";
            Settings.Get().ReportSenderEmail = "invalid";

            int actual = MainClass.Main();
            actual.Should().Be(1);
        }
    }
}