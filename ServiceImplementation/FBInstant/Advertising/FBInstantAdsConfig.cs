namespace ServiceImplementation.FBInstant.Advertising
{
    public class FBInstantAdsConfig
    {
        public readonly string BannerAdId;
        public readonly string InterstitialAdId;
        public readonly string RewardedAdId;

        public FBInstantAdsConfig(string bannerAdId, string interstitialAdId, string rewardedAdId)
        {
            this.BannerAdId       = bannerAdId;
            this.InterstitialAdId = interstitialAdId;
            this.RewardedAdId     = rewardedAdId;
        }
    }
}