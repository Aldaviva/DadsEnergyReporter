using System;
using System.Net;
using System.Net.Http;
using Autofac;

namespace DadsEnergyReporter.Injection
{
    internal class HttpClientModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CookieContainer>().AsSelf().SingleInstance();

            builder.Register(c => new HttpClientHandler
            {
                CookieContainer = c.Resolve<CookieContainer>(),
                Proxy = new WebProxy("127.0.0.1", 9998) // Fiddler
            }).As<HttpMessageHandler>().SingleInstance();

            builder.Register(c => new HttpClient(c.Resolve<HttpMessageHandler>())
            {
                Timeout = TimeSpan.FromSeconds(30)
            }).AsSelf();
        }
    }
}