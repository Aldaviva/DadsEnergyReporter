using System;

namespace DadsEnergyReporter.Exceptions
{
    [Serializable]
    internal class PowerGuideException : Exception
    {
        public PowerGuideException(string message) : base(message)
        {
        }

        public PowerGuideException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}