using System.ServiceProcess;
using System.Threading.Tasks;
using Autofac;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Service;

namespace DadsEnergyReporter.Entry
{
    public static class MainClass
    {
        public static void Main(string[] args)
        {
            JsonSerializerConfigurer.ConfigureDefault();
            
            if (args.Length >= 1 && args[0] == "--console")
            {
                using (IContainer container = ContainerFactory.CreateContainer())
                using (ILifetimeScope scope = container.BeginLifetimeScope())
                {
                    var energyReporter = scope.Resolve<EnergyReporter>();
                    Task start = energyReporter.Start();
                    
                    // block program from exiting until `start` completes
                    start.GetAwaiter().GetResult();
                }
            }
            else
            {
                ServiceBase.Run(new DadsEnergyReporter());
            }
        }
    }
}
