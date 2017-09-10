using System.ServiceProcess;
using Autofac;
using PowerGuideReporter.Injection;
using PowerGuideReporter.Service;

namespace PowerGuideReporter.Entry
{
    public partial class PowerGuideReporter : ServiceBase
    {
        private IContainer _container;
        private ILifetimeScope _scope;

        public PowerGuideReporter()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _container = ContainerFactory.CreateContainer();
            _scope = _container.BeginLifetimeScope();

            var powerGuideReporter = _scope.Resolve<PowerGuideReporterService>();
            powerGuideReporter.Start();
        }

        protected override void OnStop()
        {
            _scope.Dispose();
            _container.Dispose();
        }
    }
}
