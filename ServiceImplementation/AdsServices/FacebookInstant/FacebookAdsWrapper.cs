namespace ServiceImplementation.AdsServices.FacebookInstant
{
    using System;
    using System.Collections.Generic;
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using GameFoundation.Scripts.Utilities.LogService;
    using UnityEngine;
    using Zenject;

    public class FacebookAdsWrapper : MonoBehaviour, IAdServices
    {
        private readonly ILogService      logService;
        private readonly SignalBus        signalBus;
        private readonly AdServicesConfig adServicesConfig;
        private readonly Dictionary<string, bool> isInterstitialAdReady;
        private readonly Dictionary<string, bool> isRewardedAdReady;

        public FacebookAdsWrapper(ILogService logService, SignalBus signalBus, AdServicesConfig adServicesConfig)
        {
            this.logService       = logService;
            this.signalBus        = signalBus;
            this.adServicesConfig = adServicesConfig;
            this.isInterstitialAdReady = new();
            this.isRewardedAdReady     = new();
        }

        private void PreloadAds()
        {
            
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
            return this.isInterstitialAdReady.GetValueOrDefault(place);
        }

        public void ShowInterstitialAd(string place)
        {
            if (!this.adServicesConfig.EnableInterstitialAd) return;

            FBAds.ShowInterstitialAd(
                place,
                onSuccess: () =>
                {
                    this.signalBus.Fire(new InterstitialAdDownloadedSignal(place));
                    
                },
                onFail: err =>
                {
                    this.signalBus.Fire(new InterstitialAdLoadFailedSignal(place, err));
                }
            );
        }

        #endregion

        #region RewardedAd

        public bool IsRewardedAdReady(string place)
        {
            return this.isRewardedAdReady.GetValueOrDefault(place);
        }

        public void ShowRewardedAd(string place)
        {
            this.ShowRewardedAd(place, null);
        }

        public void ShowRewardedAd(string place, Action onCompleted)
        {
            if (!this.adServicesConfig.EnableRewardedAd) return;

            FBAds.ShowRewardedAd(
                place,
                onSuccess: () =>
                {
                    onCompleted?.Invoke();
                },
                onFail: err =>
                {
                }
            );
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