using DadsEnergyReporter.Injection;

namespace DadsEnergyReporter.Remote.OrangeRockland.Service {

    public interface OrangeRocklandService {

        OrangeRocklandAuthenticationService authentication { get; }
        GreenButtonService greenButton { get; }
        BillDocumentService billDocuments { get; }

    }

    [Component]
    internal class OrangeRocklandServiceImpl: OrangeRocklandService {

        public OrangeRocklandAuthenticationService authentication { get; }
        public GreenButtonService greenButton { get; }
        public BillDocumentService billDocuments { get; }

        public OrangeRocklandServiceImpl(OrangeRocklandAuthenticationService orangeRocklandAuthentication,
            GreenButtonService greenButtonService, BillDocumentService billDocuments) {
            authentication = orangeRocklandAuthentication;
            greenButton = greenButtonService;
            this.billDocuments = billDocuments;
        }

    }

}