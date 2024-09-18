namespace Core.AdsServices
{
    using System;
    using GameFoundation.Scripts.Utilities.LogService;
    using UnityEngine;

    public class DummyAdServiceIml : IAdServices
    {
        private readonly ILogService logService;

        public DummyAdServiceIml(ILogService logService) { this.logService = logService; }

        public void          GrantDataPrivacyConsent()                     { this.logService.Log("Dummy Grant consent"); }
        public void          RevokeDataPrivacyConsent()                    { this.logService.Log("Dummy Revoke consent"); }
        public void          GrantDataPrivacyConsent(AdNetwork adNetwork)  { this.logService.Log($"Dummy Grant consent {adNetwork}"); }
        public void          RevokeDataPrivacyConsent(AdNetwork adNetwork) { this.logService.Log($"Dummy revoke consent {adNetwork}"); }
        public ConsentStatus GetDataPrivacyConsent(AdNetwork adNetwork)    { return ConsentStatus.Granted; }

        public string AdPlatform { get; set; } = "Dummy";

        public void ShowBannerAd(BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom, int width = 320, int height = 50)
        {
            this.logService.Log($"Dummy show banner ad ay {bannerAdsPosition}");
        }

        public void HideBannedAd()                      { this.logService.Log($"Dummy hide banner ad"); }
        public void DestroyBannerAd()                   { this.logService.Log($"Dummy destroy banner ad"); }
        public bool IsInterstitialAdReady(string place) { return true; }
        public void ShowInterstitialAd(string place)    { this.logService.Log($"Dummy show Interstitial ad at {place}"); }
        public bool IsRewardedAdReady(string place)     { return true; }
        public void ShowRewardedAd(string place)        { this.logService.Log($"Dummy show Reward ad at {place}"); }

        public void ShowRewardedAd(string place, Action onCompleted, Action onFailed)
        {
            onCompleted?.Invoke();
            this.logService.Log($"Dummy show Reward ad at {place} then do {onCompleted}");
        }

        public bool IsRewardedInterstitialAdReady()                              { return true; }
        public void ShowRewardedInterstitialAd(string place)                     { this.logService.Log($"Dummy show Rewarded Interstitial ad at {place}"); }
        public void ShowRewardedInterstitialAd(string place, Action onCompleted) { this.logService.Log($"Dummy show Rewarded Interstitial ad at {place} then do {onCompleted}"); }

        public void RemoveAds(bool revokeConsent = false)
        {
            PlayerPrefs.SetInt("EM_REMOVE_ADS", -1);
            this.logService.Log($"Dummy remove Ads");
        }

        public bool IsAdsInitialized() { return true; }

        public bool IsRemoveAds() { return PlayerPrefs.HasKey("EM_REMOVE_ADS"); }
    }
}