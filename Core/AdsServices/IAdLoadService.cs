namespace Core.AdsServices
{
    using System.Collections.Generic;
    using ServiceImplementation.Configs.Ads;

    public interface IAdLoadService
    {
        AdNetworkSettings AdNetworkSettings { get;}
        bool              IsRewardedAdReady(string place="");
        bool              IsInterstitialAdReady(string place="");
        bool              IsRemoveAds();
        public void       LoadRewardAds();
        public void       LoadInterstitialAd();
    }
}