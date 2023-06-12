namespace ServiceImplementation.FBInstant.Advertising
{
    using System;
    using System.Collections.Generic;
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Utilities.LogService;
    using Newtonsoft.Json;
    using UnityEngine;
    using Zenject;

    public class FBInstantAdsWrapper : MonoBehaviour, IAdServices
    {
        #region Inject

        private SignalBus          signalBus;
        private FBInstantAdsConfig config;
        private ILogService        logger;

        [Inject]
        public void Inject(SignalBus signalBus, FBInstantAdsConfig config, ILogService logger)
        {
            this.signalBus        = signalBus;
            this.config           = config;
            this.logger           = logger;
            this.LoadInterstitialAd();
            this.LoadRewardedAd();
        }

        #endregion

        private static readonly int[] AdRetryInterval = { 5, 8, 16, 32, 64 };

        private int bannerAdRetryCount;
        private int interstitialAdRetryCount;
        private int rewardedAdRetryCount;

        private bool isInterstitialAdLoading;
        private bool isRewardedAdLoading;

        private Action onShowRewardedAdCompleted;

        #region Banner

        private void OnBannerAdShown(string message)
        {
            var @params = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
            var error   = @params["error"];

            if (error is not null)
            {
                this.logger.Error($"Banner ad load failed {++this.bannerAdRetryCount} times!");
                UniTask.Delay(TimeSpan.FromSeconds(AdRetryInterval[Math.Min(this.bannerAdRetryCount, AdRetryInterval.Length) - 1]))
                       .ContinueWith(() => this.ShowBannerAd());
            }
            else
            {
                this.bannerAdRetryCount = 0;
            }
        }

        public void ShowBannerAd( int width = 320, int height = 50)
        {
            FBInstantAds.ShowBannerAd(this.config.BannerAdId, this.gameObject.name, nameof(this.OnBannerAdShown));
        }

        private void OnBannerAdHidden(string message)
        {
            var @params = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
            var error   = @params["error"];

            if (error is not null)
            {
                this.logger.Error(error);
            }
        }

        public void HideBannedAd()
        {
            FBInstantAds.HideBannerAd(this.gameObject.name, nameof(this.OnBannerAdHidden));
        }

        public void DestroyBannerAd()
        {
            throw new NotImplementedException("Use HideBannerAd method instead!");
        }

        #endregion

        #region InterstitialAd

        public bool IsInterstitialAdReady(string place)
        {
            return FBInstantAds.IsInterstitialAdReady();
        }

        private void OnInterstitialAdLoaded(string message)
        {
            var @params = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
            var error   = @params["error"];

            this.isInterstitialAdLoading = false;

            if (error is not null)
            {
                this.logger.Error($"Interstitial ad load failed {++this.interstitialAdRetryCount} times!");
                UniTask.Delay(TimeSpan.FromSeconds(AdRetryInterval[Math.Min(this.interstitialAdRetryCount, AdRetryInterval.Length) - 1]))
                       .ContinueWith(this.LoadInterstitialAd)
                       .Forget();
            }
            else
            {
                this.interstitialAdRetryCount = 0;
            }
        }

        private void LoadInterstitialAd()
        {
            if (this.isInterstitialAdLoading) return;
            this.isInterstitialAdLoading = true;

            FBInstantAds.LoadInterstitialAd(this.config.InterstitialAdId, this.gameObject.name, nameof(this.OnInterstitialAdLoaded));
        }

        private void OnInterstitialAdShown(string message)
        {
            var @params = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
            var place   = @params["place"];
            var error   = @params["error"];

            this.LoadInterstitialAd();

            if (error is not null)
            {
                this.signalBus.Fire(new InterstitialAdLoadFailedSignal(place, error));
            }
            else
            {
                this.signalBus.Fire(new InterstitialAdDownloadedSignal(place));
            }
            
            this.signalBus.Fire(new InterstitialAdClosedSignal(place));
        }

        public void ShowInterstitialAd(string place)
        {

            FBInstantAds.ShowInterstitialAd(place, this.gameObject.name, nameof(this.OnInterstitialAdShown));
        }

        #endregion

        #region RewardedAd

        public bool IsRewardedAdReady(string place)
        {
            return FBInstantAds.IsRewardedAdReady();
        }

        private void OnRewardedAdLoaded(string message)
        {
            var @params = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
            var error   = @params["error"];

            if (error is not null)
            {
                this.logger.Error($"Rewarded ad load failed {++this.rewardedAdRetryCount} times!");
                UniTask.Delay(TimeSpan.FromSeconds(AdRetryInterval[Math.Min(this.rewardedAdRetryCount, AdRetryInterval.Length) - 1]))
                       .ContinueWith(this.LoadRewardedAd)
                       .Forget();
            }
            else
            {
                this.rewardedAdRetryCount = 0;
            }
        }

        private void LoadRewardedAd()
        {
            if (this.isRewardedAdLoading) return;
            this.isRewardedAdLoading = true;

            FBInstantAds.LoadRewardedAd(this.config.RewardedAdId, this.gameObject.name, nameof(this.OnRewardedAdLoaded));
        }

        private void OnRewardedAdShown(string message)
        {
            var @params = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
            var place   = @params["place"];
            var error   = @params["error"];

            this.LoadRewardedAd();

            if (error is not null)
            {
                this.signalBus.Fire(new RewardedAdLoadFailedSignal(place, error));
            }
            else
            {
                this.signalBus.Fire(new RewardedAdLoadedSignal(place));
                this.onShowRewardedAdCompleted?.Invoke();
            }
            
            this.signalBus.Fire(new RewardedAdCompletedSignal(place));
        }

        public void ShowRewardedAd(string place)
        {
            this.ShowRewardedAd(place, null);
        }

        public void ShowRewardedAd(string place, Action onCompleted)
        {
            this.onShowRewardedAdCompleted = onCompleted;
            FBInstantAds.ShowRewardedAd(place, this.gameObject.name, nameof(this.OnRewardedAdShown));
        }

        #endregion

        #region RemoveAds

        public void RemoveAds(bool revokeConsent = false)
        {
            // TODO Implement remove ad
            // this.adServicesConfig.EnableBannerAd       = false;
            // this.adServicesConfig.EnableInterstitialAd = false;
            // this.adServicesConfig.EnableRewardedAd     = false;
        }

        public bool IsRemoveAds()
        {
            return false;
        }

        #endregion

        #region RewardedInterstitialAd

        public bool IsRewardedInterstitialAdReady()
        {
            throw new NotImplementedException();
        }

        public void ShowRewardedInterstitialAd(string place)
        {
            throw new NotImplementedException();
        }

        public void ShowRewardedInterstitialAd(string place, Action onCompleted)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Consent

        public void GrantDataPrivacyConsent()
        {
            throw new NotImplementedException();
        }

        public void RevokeDataPrivacyConsent()
        {
            throw new NotImplementedException();
        }

        #endregion

        public bool IsAdsInitialized()
        {
            return true;
        }
    }
}