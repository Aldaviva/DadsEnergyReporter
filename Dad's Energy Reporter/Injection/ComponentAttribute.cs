using System;
using JetBrains.Annotations;

/*
 * Marker attribute for classes that should be automatically registered in the Dependency Injection container
 */
namespace DadsEnergyReporter.Injection
{
    [AttributeUsage(AttributeTargets.Class)]
    [MeansImplicitUse]
    public class ComponentAttribute : Attribute
    {
    }
}