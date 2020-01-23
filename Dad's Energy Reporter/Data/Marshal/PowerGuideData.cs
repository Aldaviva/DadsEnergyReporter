using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NodaTime;

// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace DadsEnergyReporter.Data.Marshal {

    public class Measurement {

        public LocalDateTime timestamp { get; set; }

        public double cumulativekWh { get; set; }

        //public double EnergyInIntervalkWh { get; set; } //is sometimes null and sometimes negative, so ignore and use CumulativekWh instead
        public DataStatus dataStatus { get; set; }

    }

    public class MeasurementsResponse {

        public List<Measurement> measurements { get; set; }
        public double totalEnergyInIntervalkWh { get; set; }

    }

    public class Installation {

        public Guid guid { get; set; }
//        public double SystemSize { get; set; }
//        public string JobId { get; set; }

    }

    public class InstallationsResponse {

//        public int ResultTotal { get; set; }
        public List<Installation> data { get; set; }

    }

    public struct PreLogInData {

        public string csrfToken;
        public Uri logInUri;

    }

    public class PowerGuideAuthToken {

        public string fedAuth { get; }

        public PowerGuideAuthToken(string fedAuth) {
            this.fedAuth = fedAuth;
        }

        private bool @equals(PowerGuideAuthToken other) {
            return string.Equals(fedAuth, other.fedAuth);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && @equals((PowerGuideAuthToken) obj);
        }

        public override int GetHashCode() {
            return fedAuth != null ? fedAuth.GetHashCode() : 0;
        }

        public override string ToString() {
            return $"{nameof(fedAuth)}: {fedAuth}";
        }

    }

    public enum DataStatus {

        VALIDATED

    }

}