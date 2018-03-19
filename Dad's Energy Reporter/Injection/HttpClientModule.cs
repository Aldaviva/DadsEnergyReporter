using System;
using System.Net;
using System.Net.Http;
using Autofac;
using DadsEnergyReporter.Data;

namespace DadsEnergyReporter.Injection
{
    internal class HttpClientModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CookieContainer>().AsSelf().SingleInstance();

            builder.Register(c =>
            {
                var settings = c.Resolve<Settings>();
                return new HttpClientHandler
                {
                    CookieContainer = c.Resolve<CookieContainer>(),
                    Proxy = string.IsNullOrWhiteSpace(settings.HttpProxy) ? null : new WebProxy(settings.HttpProxy)
                };
            }).As<HttpMessageHandler>().SingleInstance();

            builder.Register(c => new HttpClient(c.Resolve<HttpMessageHandler>())
            {
                Timeout = TimeSpan.FromSeconds(30)
            }).AsSelf();
        }
    }
}