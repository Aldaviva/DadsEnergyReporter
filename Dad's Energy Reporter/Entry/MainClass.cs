using System.Threading.Tasks;
using Autofac;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Service;

namespace DadsEnergyReporter.Entry
{
    public static class MainClass
    {
        public static int Main()
        {
            JsonSerializerConfigurer.ConfigureDefault();

            using (IContainer container = ContainerFactory.CreateContainer())
            using (ILifetimeScope scope = container.BeginLifetimeScope())
            {
                var energyReporter = scope.Resolve<EnergyReporter>();
                Task<int> start = energyReporter.Start();

                // block program from exiting until `start` completes
                int result = start.GetAwaiter().GetResult();
                return result;
            }
        }
    }
}