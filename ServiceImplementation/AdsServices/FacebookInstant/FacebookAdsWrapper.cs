namespace ServiceImplementation.AdsServices.FacebookInstant
{
    using System;
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using GameFoundation.Scripts.Utilities.LogService;
    using ServiceImplementation.FBInstant.Adsvertising;
    using UnityEngine;
    using Zenject;

    public class FacebookAdsWrapper : MonoBehaviour, IAdServices
    {
        private readonly ILogService      logService;
        private readonly SignalBus        signalBus;
        private readonly AdServicesConfig adServicesConfig;

        public FacebookAdsWrapper(ILogService logService, SignalBus signalBus, AdServicesConfig adServicesConfig)
        {
            this.logService       = logService;
            this.signalBus        = signalBus;
            this.adServicesConfig = adServicesConfig;
        }
        public void          GrantDataPrivacyConsent()                     { throw new NotImplementedException(); }
        public void          RevokeDataPrivacyConsent()                    { throw new NotImplementedException(); }
        public void          GrantDataPrivacyConsent(AdNetwork adNetwork)  { throw new NotImplementedException(); }
        public void          RevokeDataPrivacyConsent(AdNetwork adNetwork) { throw new NotImplementedException(); }
        public ConsentStatus GetDataPrivacyConsent(AdNetwork adNetwork)    { throw new NotImplementedException(); }

        #region browser callback

        public void RequestShowInterstitialAdSucceed()
        {
            this.signalBus.Fire(new InterstitialAdDownloadedSignal(""));
        }
        public void RequestShowInterstitialAdFailed(string error)
        {
            this.signalBus.Fire(new InterstitialAdLoadFailedSignal("", error));
        }
        public void RequestShowRewardAdSucceed()
        {
            this.signalBus.Fire(new RewardedAdLoadedSignal(""));
        }
        public void RequestShowRewardAdFailed(string error)
        {
            this.signalBus.Fire(new RewardedAdLoadFailedSignal("", error));
        }

        #endregion

        #region banner

        public void ShowBannerAd(BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom, int width = 320, int height = 50)
        {
            if (!this.adServicesConfig.EnableBannerAd)
            {
                return;
            }

            this.logService.Log("Enter facebook ad id here!", LogLevel.WARNING);
            FBAds.ShowBannerAd("");
        }
        public void HideBannedAd()    { FBAds.HideBannerAd(); }
        public void DestroyBannerAd() { this.logService.Log("Use HideBannerAd method instead!", LogLevel.EXCEPTION); }

        #endregion

        #region interstitial

        public bool IsInterstitialAdReady(string place) { return FBAds.IsInterstitialAdReady(place); }
        public void ShowInterstitialAd(string place)
        {
            if (!this.adServicesConfig.EnableInterstitialAd)
            {
                return;
            }

            FBAds.ShowInterstitialAd(place);
        }

        #endregion

        #region reward ad

        public bool IsRewardedAdReady(string place) { return FBAds.IsRewardVideoReady(place); }
        public void ShowRewardedAd(string place)
        {
            if (!this.adServicesConfig.EnableRewardedAd)
            {
                return;
            }

            FBAds.ShowRewardedVideoAd(place);
        }
        public void ShowRewardedAd(string place, Action onCompleted)
        {
            if (!this.adServicesConfig.EnableRewardedAd)
            {
                return;
            }

            onCompleted?.Invoke();
            FBAds.ShowRewardedVideoAd(place);
        }

        #endregion

        #region reward interstitial ads

        public bool IsRewardedInterstitialAdReady()                              { throw new NotImplementedException(); }
        public void ShowRewardedInterstitialAd(string place)                     { throw new NotImplementedException(); }
        public void ShowRewardedInterstitialAd(string place, Action onCompleted) { throw new NotImplementedException(); }

        #endregion

        public void RemoveAds(bool revokeConsent = false)
        {
            // TODO Implement remove ad
            // this.adServicesConfig.EnableBannerAd       = false;
            // this.adServicesConfig.EnableInterstitialAd = false;
            // this.adServicesConfig.EnableRewardedAd     = false;
        }

        public bool IsAdsInitialized() { return true; }

        public bool IsRemoveAds() { return false; }
    }
}