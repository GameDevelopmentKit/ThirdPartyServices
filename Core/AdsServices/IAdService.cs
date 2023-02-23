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
        void          GrantDataPrivacyConsent(AdNetwork  adNetwork);
        void          RevokeDataPrivacyConsent(AdNetwork adNetwork);
        ConsentStatus GetDataPrivacyConsent(AdNetwork    adNetwork);

        #endregion

        #region Banner

        void ShowBannerAd(BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom);
        void HideBannedAd();
        void DestroyBannerAd();

        #endregion

        #region Interstitial

        event Action<InterstitialAdNetwork, string> InterstitialAdCompleted;

        bool IsInterstitialAdReady();
        void ShowInterstitialAd(string place);

        #endregion

        #region Reward

        event Action<InterstitialAdNetwork, string> RewardedAdCompleted;
        event Action<InterstitialAdNetwork, string> RewardedAdSkipped;

        bool IsRewardedAdReady();
        void ShowRewardedAd(string place);
        void ShowRewardedAd(string place, Action onCompleted);

        #endregion

        #region RewardedInterstitialAd

        event Action<InterstitialAdNetwork, string> RewardedInterstitialAdCompleted;
        event Action<InterstitialAdNetwork, string> RewardedInterstitialAdSkipped;

        bool IsRewardedInterstitialAdReady();
        void ShowRewardedInterstitialAd(string place);
        void ShowRewardedInterstitialAd(string place, Action onCompleted);

        #endregion

        #region RemoveAds

        event Action AdsRemoved;
        void RemoveAds(bool revokeConsent = false);

        #endregion
    }
}