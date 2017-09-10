using System;
using System.Collections.Generic;
using NodaTime;

namespace PowerGuideReporter.Data.Marshal
{
    public class Measurement
    {
        public Instant Timestamp { get; set; }
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

    public class AuthToken
    {
        public string FedAuth { get; set; }
    }

    public enum DataStatus
    {
        Validated
    }
}
