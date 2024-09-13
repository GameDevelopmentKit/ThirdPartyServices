namespace Core.AdsServices
{
    using ServiceImplementation.Configs.Ads;

    public interface IAdLoadService
    {
        AdNetworkSettings AdNetworkSettings { get;}
        bool              IsRewardedAdReady(string     place ="");
        bool              IsInterstitialAdReady(string place ="");
        bool              IsRemoveAds();
        public void       LoadRewardAds(string      place ="");
        public void       LoadInterstitialAd(string place ="");
        bool            TryGetInterstitialPlacementId(string placement, out string id);
    }
}