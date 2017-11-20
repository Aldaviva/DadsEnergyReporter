using DadsEnergyReporter.Injection;

namespace DadsEnergyReporter.Remote.OrangeRockland.Service
{
    public interface OrangeRocklandService
    {
        OrangeRocklandAuthenticationService Authentication { get; }
        GreenButtonService GreenButton { get; }
        BillDocumentService BillDocuments { get; }
    }

    [Component]
    internal class OrangeRocklandServiceImpl : OrangeRocklandService
    {
        public OrangeRocklandAuthenticationService Authentication { get; }
        public GreenButtonService GreenButton { get; }
        public BillDocumentService BillDocuments { get; }

        public OrangeRocklandServiceImpl(OrangeRocklandAuthenticationService orangeRocklandAuthentication,
            GreenButtonService greenButtonService, BillDocumentService billDocuments)
        {
            Authentication = orangeRocklandAuthentication;
            GreenButton = greenButtonService;
            BillDocuments = billDocuments;
        }
    }
}