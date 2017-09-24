using DadsEnergyReporter.Injection;

namespace DadsEnergyReporter.Remote.PowerGuide.Service
{
    public interface PowerGuideService
    {
        PowerGuideAuthenticationService Authentication { get; }
        InstallationService InstallationService { get; }
        MeasurementService MeasurementService { get; }
    }
    
    [Component]
    internal class PowerGuideServiceImpl : PowerGuideService
    {
        public PowerGuideAuthenticationService Authentication { get; }
        public InstallationService InstallationService { get; }
        public MeasurementService MeasurementService { get; }
        
        public PowerGuideServiceImpl(PowerGuideAuthenticationService authentication, InstallationService installationService, MeasurementService measurementService)
        {
            Authentication = authentication;
            InstallationService = installationService;
            MeasurementService = measurementService;
        }
        
        
    }
}