using DadsEnergyReporter.Injection;

namespace DadsEnergyReporter.Remote.OrangeRockland.Service
{
    public interface OrangeRocklandService
    {
        OrangeRocklandAuthenticationService OrangeRocklandAuthentication { get; }
        GreenButtonService GreenButtonService { get; }
    }

    [Component]
    internal class OrangeRocklandServiceImpl : OrangeRocklandService
    {
        public OrangeRocklandAuthenticationService OrangeRocklandAuthentication { get; }
        public GreenButtonService GreenButtonService { get; }

        public OrangeRocklandServiceImpl(OrangeRocklandAuthenticationService orangeRocklandAuthentication, GreenButtonService greenButtonService)
        {
            OrangeRocklandAuthentication = orangeRocklandAuthentication;
            GreenButtonService = greenButtonService;
        }
    }
}