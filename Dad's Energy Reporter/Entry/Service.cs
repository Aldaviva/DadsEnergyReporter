using System.ServiceProcess;
using Autofac;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Service;

namespace DadsEnergyReporter.Entry
{
    public partial class DadsEnergyReporter : ServiceBase
    {
        private IContainer _container;
        private ILifetimeScope _scope;

        public DadsEnergyReporter()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _container = ContainerFactory.CreateContainer();
            _scope = _container.BeginLifetimeScope();

            var powerGuideReporter = _scope.Resolve<EnergyReporter>();
            powerGuideReporter.Start();
        }

        protected override void OnStop()
        {
            _scope.Dispose();
            _container.Dispose();
        }
    }
}
