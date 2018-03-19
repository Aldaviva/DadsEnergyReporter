using Autofac;
using DadsEnergyReporter.Data;

namespace DadsEnergyReporter.Injection
{
    public class SettingsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(Settings.Get()).AsSelf().SingleInstance();
        }
    }
}