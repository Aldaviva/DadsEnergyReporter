using System;
using System.Reflection;
using Autofac;

namespace PowerGuideReporter.Injection
{
    public static class ContainerFactory
    {
        public static IContainer CreateContainer()
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(t => t.GetCustomAttribute<ComponentAttribute>() != null)
                .AsImplementedInterfaces()
                .PropertiesAutowired((info, o) => info.GetCustomAttribute<AutowiredAttribute>() != null)
//                .PropertiesAutowired()
                .InstancePerLifetimeScope()
                .OnActivated(eventArgs => eventArgs.Instance.GetType().GetMethod("PostConstruct", new Type[0])
                    ?.Invoke(eventArgs.Instance, new object[0]))
                //.AutoActivate()
                ;

            containerBuilder.RegisterModule<HttpClientModule>();

            return containerBuilder.Build();
        }
    }
}