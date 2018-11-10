using System.Threading.Tasks;
using Autofac;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Service;
using NLog;

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
                var options = scope.Resolve<Options>();

                ConfigureLogging(options.SkipUtility);

                Task<int> start = options.SkipUtility
                    ? energyReporter.SendSolarReport()
                    : energyReporter.SendSolarAndUtilityReport();

                // block program from exiting until `start` completes
                int result = start.GetAwaiter().GetResult();
                return result;
            }
        }

        private static void ConfigureLogging(bool skipUtility)
        {
            // SkipUtility relies on console output to return its data to the PHP calling script, so don't
            // include log messages as well
            if (skipUtility)
            {
                LogManager.GlobalThreshold = LogLevel.Error;
            }
        }
    }
}