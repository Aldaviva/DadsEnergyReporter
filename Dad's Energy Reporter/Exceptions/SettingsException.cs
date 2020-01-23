using System;

namespace DadsEnergyReporter.Exceptions {

    [Serializable]
    internal class SettingsException: Exception {

        public string settingsKey { get; }
        public object invalidValue { get; }

        public SettingsException(string settingsKey, object invalidValue, string message): base(message) {
            this.settingsKey = settingsKey;
            this.invalidValue = invalidValue;
        }

    }

}