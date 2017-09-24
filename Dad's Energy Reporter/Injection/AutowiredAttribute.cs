using System;

/*
 * Marker attribute for properties whose value should be filled in by autowiring
 */
namespace DadsEnergyReporter.Injection
{
    [AttributeUsage(AttributeTargets.Field)]
    public class AutowiredAttribute : Attribute
    {
    }
}