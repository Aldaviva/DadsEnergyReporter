using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Autofac;
using Autofac.Core;
using DadsEnergyReporter.Data;
using MailKit.Net.Smtp;
using NodaTime;
using Xunit;

// ReSharper disable MemberCanBePrivate.Global

namespace DadsEnergyReporter.Injection
{
    public class InjectionTests
    {
        private readonly ContainerBuilder containerBuilder = new ContainerBuilder();

        public InjectionTests()
        {
            Settings.SettingsManager.Filename = @"%localappdata%/Dad's Energy Reporter/test-settings.json";
        }

        public static object[][] Modules =
        {
            new object[] { new MailKitModule(), typeof(SmtpClient) },
            new object[] { new HttpClientModule(), typeof(CookieContainer), typeof(HttpClient), typeof(HttpMessageHandler) },
            new object[] { new TimeZoneModule(), typeof(DateTimeZone) },
            new object[] { new SettingsModule(), typeof(Settings) }
        };

        [Theory, MemberData(nameof(Modules))]
        public void LoadModule(IModule module, params Type[] typesToResolve)
        {
            if (!typesToResolve.Contains(typeof(Settings)))
            {
                containerBuilder.RegisterInstance(new Settings
                {
                    SmtpHost = "mail.example.com"
                });
            }

            containerBuilder.RegisterModule(module);
            using (IContainer container = containerBuilder.Build())
            using (ILifetimeScope scope = container.BeginLifetimeScope())
            {
                foreach (Type typeToResolve in typesToResolve)
                {
                    scope.Resolve(typeToResolve);
                }
            }
        }
    }
}