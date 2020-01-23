using DadsEnergyReporter.Injection;

namespace DadsEnergyReporter.Remote.Solar.Tesla.Service {

    public interface OwnerApiService {

        OwnerApiAuthenticationService authentication { get; }
        ProductService product { get; }
        MeasurementService measurement { get; }

    }

    [Component]
    internal class OwnerApiServiceImpl: OwnerApiService {

        public OwnerApiAuthenticationService authentication { get; }
        public ProductService product { get; }
        public MeasurementService measurement { get; }

        public OwnerApiServiceImpl(OwnerApiAuthenticationService ownerApiAuthenticationService, ProductService productService, MeasurementService measurementService) {
            authentication = ownerApiAuthenticationService;
            product = productService;
            measurement = measurementService;
        }

    }

}