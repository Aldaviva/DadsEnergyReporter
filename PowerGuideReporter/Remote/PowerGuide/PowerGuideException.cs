using System;
using System.Runtime.Serialization;

namespace PowerGuideReporter.Remote.PowerGuide
{
    [Serializable]
    internal class PowerGuideException : Exception
    {
        public PowerGuideException()
        {
        }

        public PowerGuideException(string message) : base(message)
        {
        }

        public PowerGuideException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PowerGuideException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}