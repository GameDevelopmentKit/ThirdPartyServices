namespace ServiceImplementation.AdsServices.FacebookInstant
{
    using System;
    using System.Collections.Generic;
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using GameFoundation.Scripts.Utilities.LogService;
    using Newtonsoft.Json;
    using ServiceImplementation.FBInstant.Adsvertising;
    using UnityEngine;
    using Zenject;

    public class FacebookAdsWrapper : MonoBehaviour, IAdServices
    {
        private readonly ILogService              logService;
        private readonly SignalBus                signalBus;
        private readonly AdServicesConfig         adServicesConfig;
        private readonly Dictionary<string, bool> isInterstitialAdLoading = new();
        private readonly Dictionary<string, bool> isRewardedAdLoading     = new();
        private          Action                   onShowRewardedAdCompleted;

        public FacebookAdsWrapper(ILogService logService, SignalBus signalBus, AdServicesConfig adServicesConfig)
        {
            this.logService       = logService;
            this.signalBus        = signalBus;
            this.adServicesConfig = adServicesConfig;
            this.PreloadAds();
        }

        private void PreloadAds()
        {
            this.adServicesConfig.InterstitialAdPlacements.ForEach(this.LoadInterstitialAd);
            this.adServicesConfig.RewardedAdPlacements.ForEach(this.LoadRewardedAd);
        }

        #region Banner

        public void ShowBannerAd(BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom, int width = 320, int height = 50)
        {
            if (!this.adServicesConfig.EnableBannerAd)
            {
                return;
            }

            this.logService.Log("Enter facebook ad id here!", LogLevel.WARNING);
            FBAds.ShowBannerAd("");
        }

        public void HideBannedAd()
        {
            FBAds.HideBannerAd();
        }

        public void DestroyBannerAd()
        {
            this.logService.Log("Use HideBannerAd method instead!", LogLevel.EXCEPTION);
        }

        #endregion

        #region InterstitialAd

        public bool IsInterstitialAdReady(string place)
        {
            return FBAds.IsInterstitialAdReady(place);
        }

        private void OnInterstitialAdLoaded(string message)
        {
            var @params = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
            var place   = @params["place"];
            var error   = @params["error"];

            this.isInterstitialAdLoading[place] = false;

            if (error is not null)
            {
                this.LoadInterstitialAd(place);
            }
        }

        private void LoadInterstitialAd(string place)
        {
            if (this.isInterstitialAdLoading.GetValueOrDefault(place, false)) return;
            this.isInterstitialAdLoading[place] = true;

            FBAds.LoadInterstitialAd(place, this.gameObject.name, nameof(this.OnInterstitialAdLoaded));
        }

        private void OnInterstitialAdShown(string message)
        {
            var @params = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
            var place   = @params["place"];
            var error   = @params["error"];

            this.LoadInterstitialAd(place);
            if (error is not null)
            {
                this.signalBus.Fire(new InterstitialAdLoadFailedSignal(place, error));
            }
            else
            {
                this.signalBus.Fire(new InterstitialAdDownloadedSignal(place));
            }
        }

        public void ShowInterstitialAd(string place)
        {
            if (!this.adServicesConfig.EnableInterstitialAd) return;

            FBAds.ShowInterstitialAd(place, this.gameObject.name, nameof(this.OnInterstitialAdShown));
        }

        #endregion

        #region RewardedAd

        public bool IsRewardedAdReady(string place)
        {
            return FBAds.IsRewardedAdReady(place);
        }

        private void OnRewardedAdLoaded(string message)
        {
            var @params = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
            var place   = @params["place"];
            var error   = @params["error"];

            this.isRewardedAdLoading[place] = false;

            if (error is not null)
            {
                this.LoadRewardedAd(place);
            }
        }

        private void LoadRewardedAd(string place)
        {
            if (this.isRewardedAdLoading.GetValueOrDefault(place, false)) return;
            this.isRewardedAdLoading[place] = true;

            FBAds.LoadRewardedAd(place, this.gameObject.name, nameof(this.OnRewardedAdLoaded));
        }

        private void OnRewardedAdShown(string message)
        {
            var @params = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
            var place   = @params["place"];
            var error   = @params["error"];

            this.LoadRewardedAd(place);
            if (error is not null)
            {
                this.signalBus.Fire(new RewardedAdLoadFailedSignal(place, error));
            }
            else
            {
                this.signalBus.Fire(new RewardedAdLoadedSignal(place));
                this.onShowRewardedAdCompleted?.Invoke();
            }
        }

        public void ShowRewardedAd(string place)
        {
            this.ShowRewardedAd(place, null);
        }

        public void ShowRewardedAd(string place, Action onCompleted)
        {
            if (!this.adServicesConfig.EnableRewardedAd) return;

            this.onShowRewardedAdCompleted = onCompleted;
            FBAds.ShowRewardedAd(place, this.gameObject.name, nameof(this.OnRewardedAdShown));
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

        public void GrantDataPrivacyConsent(AdNetwork adNetwork)
        {
            throw new NotImplementedException();
        }

        public void RevokeDataPrivacyConsent(AdNetwork adNetwork)
        {
            throw new NotImplementedException();
        }

        public ConsentStatus GetDataPrivacyConsent(AdNetwork adNetwork)
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