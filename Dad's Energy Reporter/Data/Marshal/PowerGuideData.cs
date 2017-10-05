using System;
using System.Collections.Generic;
using NodaTime;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace DadsEnergyReporter.Data.Marshal
{
    public class Measurement
    {
        public LocalDateTime Timestamp { get; set; }
        public double CumulativekWh { get; set; }
        public double EnergyInIntervalkWh { get; set; }
        public DataStatus DataStatus { get; set; }
    }

    public class MeasurementsResponse
    {
        public List<Measurement> Measurements { get; set; }
        public double TotalEnergyInIntervalkWh { get; set; }
    }

    public class Installation
    {
        public Guid Guid { get; set; }
        public double SystemSize { get; set; }
        public string JobId { get; set; }
    }

    public class InstallationsResponse
    {
        public int ResultTotal { get; set; }
        public List<Installation> Data { get; set; }
    }

    public struct PreLogInData
    {
        public string CsrfToken;
        public Uri LogInUri;
    }

    public class PowerGuideAuthToken
    {
        public string FedAuth { get; set; }

        private bool Equals(PowerGuideAuthToken other)
        {
            return string.Equals(FedAuth, other.FedAuth);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((PowerGuideAuthToken) obj);
        }

        public override int GetHashCode()
        {
            return FedAuth != null ? FedAuth.GetHashCode() : 0;
        }
    }

    public enum DataStatus
    {
        Validated
    }
}
