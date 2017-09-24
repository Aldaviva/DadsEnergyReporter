using System;

namespace DadsEnergyReporter.Remote.PowerGuide
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