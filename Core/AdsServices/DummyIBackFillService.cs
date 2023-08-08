namespace Core.AdsServices
{
    public class DummyIBackFillService : IBackFillAdsService
    {
        public bool IsInterstitialAdReady(string place) => false;
        public void ShowInterstitialAd(string place)    { }
        public void LoadInterstitialAd(string place)    { }
    }
}