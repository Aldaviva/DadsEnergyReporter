using Autofac;
using MailKit.Net.Smtp;

namespace DadsEnergyReporter.Injection
{
    internal class MailKitModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SmtpClient>().AsImplementedInterfaces().AsSelf().SingleInstance();
        }
    }
}