namespace Core.AdsServices
{
    using ServiceImplementation.Configs.Ads;

    public interface IAdLoadService
    {
        string            AdPlatform        { get; }
        AdNetworkSettings AdNetworkSettings { get; }
        bool              IsRewardedAdReady(string     place = "");
        bool              IsInterstitialAdReady(string place = "");
        bool              IsRemoveAds();
        public void       LoadRewardAds(string                 place = "");
        bool              TryGetRewardPlacementId(string       placement, out string id);
        public void       LoadInterstitialAd(string            place = "");
        bool              TryGetInterstitialPlacementId(string placement, out string id);
    }
}