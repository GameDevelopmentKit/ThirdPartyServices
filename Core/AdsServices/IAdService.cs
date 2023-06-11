namespace Core.AdsServices
{
    using System;

    /// <summary>
    /// Do not set place default by empty to make sure we have enough data to analize
    /// </summary>
    public interface IAdServices
    {
        #region consent

        void          GrantDataPrivacyConsent();
        void          RevokeDataPrivacyConsent();

        #endregion

        #region Banner

        void ShowBannerAd(int width = 320, int height = 50);
        void HideBannedAd();
        void DestroyBannerAd();

        #endregion

        #region Interstitial

        bool IsInterstitialAdReady(string place);
        void ShowInterstitialAd(string    place);

        #endregion

        #region Reward

        bool IsRewardedAdReady(string place);
        void ShowRewardedAd(string    place);
        void ShowRewardedAd(string    place, Action onCompleted);

        #endregion

        #region RewardedInterstitialAd

        bool IsRewardedInterstitialAdReady();
        void ShowRewardedInterstitialAd(string place);
        void ShowRewardedInterstitialAd(string place, Action onCompleted);

        #endregion

        #region RemoveAds

        void         RemoveAds(bool revokeConsent = false);

        #endregion

        bool IsAdsInitialized();
        bool IsRemoveAds();
    }
}