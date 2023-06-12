namespace Core.AdsServices
{
    using System;
    using Core.AdsServices.Signals;
    using GameFoundation.Scripts.Utilities.LogService;
    using UnityEngine;
    using Zenject;

    public class DummyAdServiceIml : IAdServices
    {
        private readonly ILogService logService;
        private readonly SignalBus   signalBus;

        public DummyAdServiceIml(ILogService logService, SignalBus signalBus)
        {
            this.logService = logService;
            this.signalBus  = signalBus;
        }

        public void GrantDataPrivacyConsent()                      { this.logService.Log("Dummy Grant consent"); }
        public void RevokeDataPrivacyConsent()                     { this.logService.Log("Dummy Revoke consent"); }
        public void ShowBannerAd(int width = 320, int height = 50) { this.logService.Log("Dummy Show banner"); }
        public void HideBannedAd()                                 { this.logService.Log($"Dummy hide banner ad"); }
        public void DestroyBannerAd()                              { this.logService.Log($"Dummy destroy banner ad"); }
        public bool IsInterstitialAdReady(string place)            { return true; }

        public void ShowInterstitialAd(string place)
        {
            this.logService.Log($"Dummy show Interstitial ad at {place}");
            this.signalBus.Fire(new InterstitialAdClosedSignal(place));
        }
        public bool IsRewardedAdReady(string place)                { return true; }
        public void ShowRewardedAd(string place)                   { this.logService.Log($"Dummy show Reward ad at {place}"); }

        public void ShowRewardedAd(string place, Action onCompleted)
        {
            onCompleted?.Invoke();
            this.logService.Log($"Dummy show Reward ad at {place} then do {onCompleted}");
            this.signalBus.Fire(new RewardedAdCompletedSignal(place));
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