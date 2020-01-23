using System;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using Autofac;
using DadsEnergyReporter.Data;

namespace DadsEnergyReporter.Injection {

    internal class HttpClientModule: Module {

        protected override void Load(ContainerBuilder builder) {
            builder.RegisterType<CookieContainer>().AsSelf().SingleInstance();

            builder.Register(c => {
                var settings = c.Resolve<Settings>();
                return new HttpClientHandler {
                    CookieContainer = c.Resolve<CookieContainer>(),
                    Proxy = string.IsNullOrWhiteSpace(settings.httpProxy) ? null : new WebProxy(settings.httpProxy),

                    // Use TLS1.2, otherwise SolarCity requests fail with "System.Net.WebException: The request was aborted: Could not create SSL/TLS secure channel."
                    SslProtocols = SslProtocols.Tls12
                };
            }).As<HttpMessageHandler>().SingleInstance();

            builder.Register(c => new HttpClient(c.Resolve<HttpMessageHandler>()) {
                Timeout = TimeSpan.FromSeconds(30)
            }).AsSelf();
        }

    }

}