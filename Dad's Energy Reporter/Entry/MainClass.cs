using System.Threading.Tasks;
using Autofac;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Service;

namespace DadsEnergyReporter.Entry
{
    public static class MainClass
    {
        public static void Main()
        {
            JsonSerializerConfigurer.ConfigureDefault();

            using (IContainer container = ContainerFactory.CreateContainer())
            using (ILifetimeScope scope = container.BeginLifetimeScope())
            {
                var energyReporter = scope.Resolve<EnergyReporter>();
                Task start = energyReporter.Start();

                // block program from exiting until `start` completes
                start.GetAwaiter().GetResult();
            }
        }
    }
}