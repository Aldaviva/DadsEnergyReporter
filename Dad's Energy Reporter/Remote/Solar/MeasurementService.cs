using System.Threading.Tasks;
using DadsEnergyReporter.Data;
using NodaTime;

namespace DadsEnergyReporter.Remote.Solar {

    public interface MeasurementService
    {

        Task<Measurement> measure(DateInterval billingInterval);

    }

}