using DadsEnergyReporter.Injection;

namespace DadsEnergyReporter.Remote.OrangeRockland.Service
{
    public interface OrangeRocklandService
    {
        OrangeRocklandAuthenticationService Authentication { get; }
        GreenButtonService GreenButton { get; }
    }

    [Component]
    internal class OrangeRocklandServiceImpl : OrangeRocklandService
    {
        public OrangeRocklandAuthenticationService Authentication { get; }
        public GreenButtonService GreenButton { get; }

        public OrangeRocklandServiceImpl(OrangeRocklandAuthenticationService orangeRocklandAuthentication, GreenButtonService greenButtonService)
        {
            Authentication = orangeRocklandAuthentication;
            GreenButton = greenButtonService;
        }
    }
}