﻿using System;
using System.Collections.Generic;
using JetBrains.Annotations;
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
    }

    public enum DataStatus
    {
        Validated
    }
}