using System;
using System.Net;
using System.Net.Http;
using Autofac;
using Autofac.Core;
using MailKit.Net.Smtp;
using NodaTime;
using Xunit;

// ReSharper disable MemberCanBePrivate.Global

namespace DadsEnergyReporter.Injection
{
    public class InjectionTests
    {
        private readonly ContainerBuilder containerBuilder = new ContainerBuilder();

        public static object[][] Modules = {
            new object[] { new MailKitModule(), typeof(SmtpClient) },
            new object[] { new HttpClientModule(), typeof(CookieContainer), typeof(HttpClient), typeof(HttpMessageHandler) },
            new object[] { new TimeZoneModule(), typeof(DateTimeZone) }
        };
        
        [Theory, MemberData(nameof(Modules))]
        public void LoadModule(IModule module, params Type[] typesToResolve)
        {
            containerBuilder.RegisterModule(module);
            using (IContainer container = containerBuilder.Build())
            using (ILifetimeScope scope =container.BeginLifetimeScope())
            {
                foreach (Type typeToResolve in typesToResolve)
                {
                    scope.Resolve(typeToResolve);
                }
            }
        }
    }
}