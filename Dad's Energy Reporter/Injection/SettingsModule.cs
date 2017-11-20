using Autofac;
using DadsEnergyReporter.Properties;

namespace DadsEnergyReporter.Injection
{
    public class SettingsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(Settings.Default).AsSelf().SingleInstance();
        }
    }
}