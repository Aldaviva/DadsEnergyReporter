using System.ServiceProcess;
using System.Threading.Tasks;
using Autofac;
using PowerGuideReporter.Injection;
using PowerGuideReporter.Service;

namespace PowerGuideReporter.Entry
{
    public static class MainClass
    {
        public static void Main(string[] args)
        {
            if (args.Length >= 1 && args[0] == "--interactive")
            {
                using (IContainer container = ContainerFactory.CreateContainer())
                using (ILifetimeScope scope = container.BeginLifetimeScope())
                {
                    var powerGuideReporter = scope.Resolve<PowerGuideReporterService>();
                    Task start = powerGuideReporter.Start();
                    
                    // block program from exiting until `start` completes
                    start.GetAwaiter().GetResult();
                }
            }
            else
            {
                ServiceBase.Run(new PowerGuideReporter());
            }
        }
    }
}
