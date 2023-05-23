namespace ServiceImplementation.FBInstant.Advertising
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class FBInstantAdsConfig
    {
        public readonly string                     BannerAdPlacement;
        public readonly ReadOnlyCollection<string> InterstitialAdPlacements;
        public readonly ReadOnlyCollection<string> RewardedAdPlacements;

        public FBInstantAdsConfig(string bannerAdPlacement, List<string> interstitialAdPlacements, List<string> rewardedAdPlacements)
        {
            this.BannerAdPlacement        = bannerAdPlacement;
            this.InterstitialAdPlacements = interstitialAdPlacements.AsReadOnly();
            this.RewardedAdPlacements     = rewardedAdPlacements.AsReadOnly();
        }
    }
}