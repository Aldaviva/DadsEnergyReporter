using System;

namespace DadsEnergyReporter.Exceptions {

    [Serializable]
    public class TeslaException: Exception {

        public TeslaException(string message): base(message) { }
        public TeslaException(string message, Exception innerException): base(message, innerException) { }

    }

}