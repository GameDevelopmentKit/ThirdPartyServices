﻿namespace ServiceImplementation.AdsServices.FacebookInstant
{
    using System;
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Utilities.LogService;
    using ServiceImplementation.FBInstant.Adsvertising;
    using UnityEngine;
    using Zenject;

    public class FacebookAdsWrapper : MonoBehaviour, IAdServices
    {
        private readonly ILogService              logService;
        private readonly SignalBus                signalBus;
        private readonly FacebookAdServicesConfig adServicesConfig;

        public FacebookAdsWrapper(ILogService logService, SignalBus signalBus, AdServicesConfig adServicesConfig)
        {
            this.logService       = logService;
            this.signalBus        = signalBus;
            this.adServicesConfig = adServicesConfig as FacebookAdServicesConfig ?? throw new("Please bind FacebookAdServicesConfig to AdServicesConfig!");
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

        private async void LoadInterstitialAd(string place)
        {
            var ucs = new UniTaskCompletionSource<bool>();
            FBAds.LoadInterstitialAd(
                place,
                onSuccess: () =>
                {
                    ucs.TrySetResult(true);
                },
                onFail: err =>
                {
                    ucs.TrySetResult(false);
                }
            );
            if (!await ucs.Task)
            {
                this.LoadInterstitialAd(place);
            }
        }

        public bool IsInterstitialAdReady(string place)
        {
            return FBAds.IsInterstitialAdReady(place);
        }

        public void ShowInterstitialAd(string place)
        {
            if (!this.adServicesConfig.EnableInterstitialAd) return;

            FBAds.ShowInterstitialAd(
                place,
                onSuccess: () =>
                {
                    this.signalBus.Fire(new InterstitialAdDownloadedSignal(place));
                    this.LoadInterstitialAd(place);
                },
                onFail: err =>
                {
                    this.signalBus.Fire(new InterstitialAdLoadFailedSignal(place, err));
                    this.LoadInterstitialAd(place);
                }
            );
        }

        #endregion

        #region RewardedAd

        private async void LoadRewardedAd(string place)
        {
            var ucs = new UniTaskCompletionSource<bool>();
            FBAds.LoadRewardedAd(
                place,
                onSuccess: () =>
                {
                    ucs.TrySetResult(true);
                },
                onFail: err =>
                {
                    ucs.TrySetResult(false);
                }
            );
            if (!await ucs.Task)
            {
                this.LoadRewardedAd(place);
            }
        }

        public bool IsRewardedAdReady(string place)
        {
            return FBAds.IsRewardedAdReady(place);
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
                    this.LoadRewardedAd(place);
                },
                onFail: err =>
                {
                    this.LoadRewardedAd(place);
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