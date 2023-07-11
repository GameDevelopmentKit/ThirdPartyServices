namespace Core.AdsServices
{
    public interface IBackFillAdsService
    {
        bool IsInterstitialAdReady(string place);
        void ShowInterstitialAd(string place);
        void LoadInterstitialAd(string place);
    }
}