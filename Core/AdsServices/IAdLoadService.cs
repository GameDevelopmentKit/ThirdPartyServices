namespace Core.AdsServices
{
    public interface IAdLoadService
    {
        bool        IsRewardedAdReady(string place="");
        bool        IsInterstitialAdReady(string place="");
        bool        IsRemoveAds();
        public void LoadRewardAds();
        public void LoadInterstitialAd();
    }
}