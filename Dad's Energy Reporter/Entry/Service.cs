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

            var energyReporter = _scope.Resolve<EnergyReporter>();
            energyReporter.Start();
        }

        protected override void OnStop()
        {
            _scope.Dispose();
            _container.Dispose();
        }
    }
}
