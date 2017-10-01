using System;

namespace DadsEnergyReporter.Exceptions
{
    [Serializable]
    internal class OrangeRocklandException : Exception
    {
        public OrangeRocklandException(string message) : base(message)
        {
        }

        public OrangeRocklandException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}