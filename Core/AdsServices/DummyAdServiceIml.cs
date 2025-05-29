namespace Core.AdsServices
{
    using System;
    using Core.AdsServices.Signals;
    using GameFoundation.Scripts.Utilities.LogService;
    using GameFoundation.Signals;
    using UnityEngine;
    using UnityEngine.Scripting;

    public class DummyAdServiceIml : IAdServices
    {
        private readonly ILogService logService;
        private readonly SignalBus   signalBus;

        [Preserve]
        public DummyAdServiceIml(ILogService logService, SignalBus signalBus)
        {
            this.logService = logService;
            this.signalBus  = signalBus;
        }

        public void GrantDataPrivacyConsent()
        {
            this.logService.Log("Dummy Grant consent");
        }

        public void RevokeDataPrivacyConsent()
        {
            this.logService.Log("Dummy Revoke consent");
        }

        public void GrantDataPrivacyConsent(AdNetwork adNetwork)
        {
            this.logService.Log($"Dummy Grant consent {adNetwork}");
        }

        public void RevokeDataPrivacyConsent(AdNetwork adNetwork)
        {
            this.logService.Log($"Dummy revoke consent {adNetwork}");
        }

        public ConsentStatus GetDataPrivacyConsent(AdNetwork adNetwork)
        {
            return ConsentStatus.Granted;
        }

        public string AdPlatform { get; set; } = "Dummy";

        public void ShowBannerAd(BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom, int width = 320, int height = 50)
        {
            this.logService.Log($"Dummy show banner ad ay {bannerAdsPosition}");
        }

        public void HideBannedAd()
        {
            this.logService.Log($"Dummy hide banner ad");
        }

        public void DestroyBannerAd()
        {
            this.logService.Log($"Dummy destroy banner ad");
        }

        public bool IsInterstitialAdReady(string place)
        {
            return true;
        }

        public void ShowInterstitialAd(string place)
        {
            this.signalBus.Fire<InterstitialAdClosedSignal>(new(place, null));
            this.logService.Log($"Dummy show Interstitial ad at {place}");
        }

        public bool IsRewardedAdReady(string place)
        {
            return true;
        }

        public void ShowRewardedAd(string place)
        {
            this.logService.Log($"Dummy show Reward ad at {place}");
        }

        public void ShowRewardedAd(string place, Action onCompleted, Action onFailed)
        {
            onCompleted?.Invoke();
            this.signalBus.Fire<RewardedAdClosedSignal>(new(place, null));
            this.logService.Log($"Dummy show Reward ad at {place} then do {onCompleted}");
        }

        public bool IsRewardedInterstitialAdReady()
        {
            return true;
        }

        public void ShowRewardedInterstitialAd(string place)
        {
            this.logService.Log($"Dummy show Rewarded Interstitial ad at {place}");
        }

        public void ShowRewardedInterstitialAd(string place, Action onCompleted)
        {
            this.logService.Log($"Dummy show Rewarded Interstitial ad at {place} then do {onCompleted}");
        }

        public bool IsAdsInitialized()
        {
            return true;
        }
    }
}