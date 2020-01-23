using DadsEnergyReporter.Injection;

namespace DadsEnergyReporter.Remote.Solar.PowerGuide.Service {

    public interface PowerGuideService {

        PowerGuideAuthenticationService authentication { get; }
        InstallationService installation { get; }
        MeasurementService measurement { get; }

    }

    [Component]
    internal class PowerGuideServiceImpl: PowerGuideService {

        public PowerGuideAuthenticationService authentication { get; }
        public InstallationService installation { get; }
        public MeasurementService measurement { get; }

        public PowerGuideServiceImpl(PowerGuideAuthenticationService authentication, InstallationService installationService,
            MeasurementService measurementService) {
            this.authentication = authentication;
            installation = installationService;
            measurement = measurementService;
        }

    }

}