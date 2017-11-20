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
            
            Action thrower = () => MainClass.Main();
            thrower.ShouldThrow<SettingsException>();
        }

        /*[Fact]
        public void StartAndStop()
        {
            Settings.Default.reportSenderEmail = "invalid";
            var service = new TestableService();
            service.TestStart(new string[0]);
            service.Stop();
            service.Dispose();
        }

        private class TestableService : DadsEnergyReporter
        {
            public void TestStart(string[] args)
            {
                OnStart(args);
            }
        }*/
    }
}