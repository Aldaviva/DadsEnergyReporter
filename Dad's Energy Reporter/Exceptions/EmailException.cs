using System;

namespace DadsEnergyReporter.Exceptions
{
    [Serializable]
    internal class EmailException : Exception
    {
        public EmailException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}