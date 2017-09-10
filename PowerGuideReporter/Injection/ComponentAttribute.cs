using System;

/*
 * Marker attribute for classes that should be automatically registered in the Dependency Injection container
 */
namespace PowerGuideReporter.Injection
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ComponentAttribute : Attribute
    {
    }
}