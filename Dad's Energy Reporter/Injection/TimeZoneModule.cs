using Autofac;
using NodaTime;

namespace DadsEnergyReporter.Injection
{
    public class TimeZoneModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(DateTimeZoneProviders.Tzdb["America/New_York"]).AsSelf().SingleInstance();
        }
    }
}