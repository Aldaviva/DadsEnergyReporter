using System.Threading.Tasks;
using Autofac;
using DadsEnergyReporter.Data.Marshal;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Service;
using NLog;

namespace DadsEnergyReporter.Entry {

    public static class MainClass {

        public static int Main() {
            JsonSerializerConfigurer.configureDefault();

            using IContainer container = ContainerFactory.createContainer();
            using ILifetimeScope scope = container.BeginLifetimeScope();
            var energyReporter = scope.Resolve<EnergyReporter>();
            var options = scope.Resolve<Options>();

            configureLogging(options.skipUtility);

            Task<int> start = options.skipUtility
                ? energyReporter.sendSolarReport()
                : energyReporter.sendSolarAndUtilityReport();

            // block program from exiting until `start` completes
            int result = start.GetAwaiter().GetResult();
            return result;
        }

        private static void configureLogging(bool skipUtility) {
            // SkipUtility relies on console output to return its data to the PHP calling script, so don't
            // include log messages as well
            if (skipUtility) {
                LogManager.GlobalThreshold = LogLevel.Error;
            }
        }

    }

}