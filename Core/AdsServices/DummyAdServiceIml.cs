namespace Core.AdsServices
{
    using System;
    using System.Collections.Generic;
    using Core.AdsServices.Signals;
    using GameFoundation.Signals;
    using TheOne.Logging;
    using UnityEngine.Scripting;
    using ILogger = TheOne.Logging.ILogger;

    public class DummyAdServiceIml : IAdServices
    {
        private readonly SignalBus signalBus;
        private readonly ILogger   logger;

        [Preserve]
        public DummyAdServiceIml(SignalBus signalBus, ILoggerManager loggerManager)
        {
            this.signalBus = signalBus;
            this.logger    = loggerManager.GetLogger(this);
        }

        public void GrantDataPrivacyConsent()
        {
            this.logger.Info("Dummy Grant consent");
        }

        public void RevokeDataPrivacyConsent()
        {
            this.logger.Info("Dummy Revoke consent");
        }

        public void GrantDataPrivacyConsent(AdNetwork adNetwork)
        {
            this.logger.Info($"Dummy Grant consent {adNetwork}");
        }

        public void RevokeDataPrivacyConsent(AdNetwork adNetwork)
        {
            this.logger.Info($"Dummy revoke consent {adNetwork}");
        }

        public ConsentStatus GetDataPrivacyConsent(AdNetwork adNetwork)
        {
            return ConsentStatus.Granted;
        }

        public string AdPlatform { get; set; } = "Dummy";

        public void ShowBannerAd(BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom, int width = 320, int height = 50)
        {
            this.logger.Info($"Dummy show banner ad ay {bannerAdsPosition}");
        }

        public void HideBannedAd()
        {
            this.logger.Info($"Dummy hide banner ad");
        }

        public void DestroyBannerAd()
        {
            this.logger.Info($"Dummy destroy banner ad");
        }

        public bool IsInterstitialAdReady(string place)
        {
            return true;
        }

        public void ShowInterstitialAd(string place, Dictionary<string, object> metadata)
        {
            this.signalBus.Fire<InterstitialAdClosedSignal>(new(place, null));
            this.logger.Info($"Dummy show Interstitial ad at {place}");
        }

        public bool IsRewardedAdReady(string place)
        {
            return true;
        }

        public void ShowRewardedAd(string place)
        {
            this.logger.Info($"Dummy show Reward ad at {place}");
        }

        public void ShowRewardedAd(string place, Action onCompleted, Action onFailed, Dictionary<string, object> metadata)
        {
            onCompleted?.Invoke();
            this.signalBus.Fire<RewardedAdClosedSignal>(new(place, null));
            this.logger.Info($"Dummy show Reward ad at {place} then do {onCompleted}");
        }

        public bool IsRewardedInterstitialAdReady()
        {
            return true;
        }

        public void ShowRewardedInterstitialAd(string place)
        {
            this.logger.Info($"Dummy show Rewarded Interstitial ad at {place}");
        }

        public void ShowRewardedInterstitialAd(string place, Action onCompleted)
        {
            this.logger.Info($"Dummy show Rewarded Interstitial ad at {place} then do {onCompleted}");
        }

        public bool IsAdsInitialized()
        {
            return true;
        }
    }
}