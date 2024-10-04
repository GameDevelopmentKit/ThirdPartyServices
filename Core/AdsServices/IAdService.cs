namespace Core.AdsServices
{
    using System;

    // Do not set place empty by default to make sure we have enough data to analyze
    public interface IAdServices
    {
        string AdPlatform { get; }

        bool IsAdsInitialized();

        #region Banner

        void ShowBannerAd(BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom, int width = 320, int height = 50);

        void HideBannedAd();

        void DestroyBannerAd();

        #endregion

        #region Interstitial

        bool IsInterstitialAdReady(string place);

        void ShowInterstitialAd(string place);

        #endregion

        #region Reward

        bool IsRewardedAdReady(string place);

        void ShowRewardedAd(string place, Action onCompleted, Action onFailed);

        #endregion

        #region RemoveAds

        void RemoveAds();

        bool IsRemoveAds();

        #endregion
    }
}