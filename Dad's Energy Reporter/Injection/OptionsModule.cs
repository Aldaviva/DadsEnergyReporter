using System;
using System.Linq;
using Autofac;
using DadsEnergyReporter.Entry;
using PowerArgs;

namespace DadsEnergyReporter.Injection {

    public class OptionsModule: Module {

        protected override void Load(ContainerBuilder builder) {
            Args.SearchAssemblyForRevivers(); //find the NodaTime revivers
            builder.Register(context => Args.Parse<Options>(Environment.GetCommandLineArgs().Skip(1).ToArray()))
                .As<Options>()
                .SingleInstance();
        }

    }

}