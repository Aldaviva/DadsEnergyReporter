using System;
using System.Net;
using System.Net.Http;
using Autofac;
using DadsEnergyReporter.Properties;

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
                Proxy = string.IsNullOrWhiteSpace(Settings.Default.httpProxy) ? null : new WebProxy(Settings.Default.httpProxy)
            }).As<HttpMessageHandler>().SingleInstance();

            builder.Register(c => new HttpClient(c.Resolve<HttpMessageHandler>())
            {
                Timeout = TimeSpan.FromSeconds(30)
            }).AsSelf();
        }
    }
}