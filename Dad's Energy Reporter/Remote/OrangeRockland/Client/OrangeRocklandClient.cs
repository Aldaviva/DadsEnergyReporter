using System;
using DadsEnergyReporter.Injection;
using DadsEnergyReporter.Remote.Common;

namespace DadsEnergyReporter.Remote.OrangeRockland.Client
{
    public interface OrangeRocklandClient
    {
        OrangeRocklandAuthenticationClient OrangeRocklandAuthenticationClient { get; }
        GreenButtonClient GreenButtonClient { get; }
    }
    
    [Component]
    internal class OrangeRocklandClientImpl : OrangeRocklandClient
    {
        internal readonly ApiClient ApiClient;
        
        public OrangeRocklandAuthenticationClient OrangeRocklandAuthenticationClient { get; }
        public GreenButtonClient GreenButtonClient { get; }

        internal static UriBuilder ApiRoot => new UriBuilder("https", "apps.coned.com", -1, "/ORMyAccount/Forms/");

        public OrangeRocklandClientImpl(ApiClient apiClient)
        {
            ApiClient = apiClient;
            OrangeRocklandAuthenticationClient = new OrangeRocklandAuthenticationClientImpl(this);
            GreenButtonClient = new GreenButtonClientImpl(this);
        }
    }
}
