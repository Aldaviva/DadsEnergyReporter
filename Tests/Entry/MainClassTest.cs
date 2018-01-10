using System;
using DadsEnergyReporter.Exceptions;
using DadsEnergyReporter.Properties;
using FluentAssertions;
using Xunit;

namespace DadsEnergyReporter.Entry
{
    public class MainClassTest
    {
        [Fact]
        public void StartConsole()
        {
            Settings.Default.reportSenderEmail = "invalid";

            int actual = MainClass.Main();
            actual.Should().Be(1);
        }
    }
}