namespace ServiceImplementation.AdsServices.FacebookInstant
{
    using System.Collections.Generic;

    public class FacebookAdServicesConfig : AdServicesConfig
    {
        public List<string> InterstitialAdPlacements { get; set; } = new();
        public List<string> RewardedAdPlacements     { get; set; } = new();
    }
}