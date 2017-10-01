using DadsEnergyReporter.Injection;

namespace DadsEnergyReporter.Remote.PowerGuide.Service
{
    public interface PowerGuideService
    {
        PowerGuideAuthenticationService Authentication { get; }
        InstallationService Installation { get; }
        MeasurementService Measurement { get; }
    }
    
    [Component]
    internal class PowerGuideServiceImpl : PowerGuideService
    {
        public PowerGuideAuthenticationService Authentication { get; }
        public InstallationService Installation { get; }
        public MeasurementService Measurement { get; }
        
        public PowerGuideServiceImpl(PowerGuideAuthenticationService authentication, InstallationService installationService, MeasurementService measurementService)
        {
            Authentication = authentication;
            Installation = installationService;
            Measurement = measurementService;
        }
        
        
    }
}