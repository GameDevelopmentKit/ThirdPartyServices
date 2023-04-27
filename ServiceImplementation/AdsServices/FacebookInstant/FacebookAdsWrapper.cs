namespace ServiceImplementation.AdsServices.FacebookInstant
{
    using System;
    using Core.AdsServices;
    using GameFoundation.Scripts.Utilities.LogService;
    using Plugins.WebGL;
    using UnityEngine;
    using Zenject;
   
    public class FacebookAdsWrapper : MonoBehaviour, IAdServices
    {
        public Action         OnInterstitialAdCompleted = null;
        public Action<string> OnInterstitialAdFailed    = null;
        public Action         OnRewardVideoAdCompleted  = null;
        public Action<string> OnRewardVideoAdFailed     = null;

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

        public void RequestShowInterstitialAdSuccessed()          { this.OnInterstitialAdCompleted?.Invoke(); }
        public void RequestShowInterstitialAdFailed(string error) { this.OnInterstitialAdFailed?.Invoke(error); }
        public void RequestShowRewardAdSuccessed()                { this.OnRewardVideoAdCompleted?.Invoke(); }
        public void RequestShowRewardAdFailed(string error)       { this.OnRewardVideoAdFailed?.Invoke(error); }

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

        public bool IsInterstitialAdReady(string place)
        {
            return FBAds.IsInterstitialAdReady(place);
        }
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
            onCompleted = this.OnRewardVideoAdCompleted;
            onCompleted?.Invoke();
            FBAds.ShowRewardedVideoAd(place);
        }

        #endregion
        
        public bool IsRewardedInterstitialAdReady()                              { throw new NotImplementedException(); }
        public void ShowRewardedInterstitialAd(string place)                     { throw new NotImplementedException(); }
        public void ShowRewardedInterstitialAd(string place, Action onCompleted) { throw new NotImplementedException(); }
        public void RemoveAds(bool revokeConsent = false)
        {
            // TODO Implement remove ad
            // this.adServicesConfig.EnableBannerAd       = false;
            // this.adServicesConfig.EnableInterstitialAd = false;
            // this.adServicesConfig.EnableRewardedAd     = false;
        }
        public bool IsAdsInitialized()
        {
            return true;
        }
        public bool IsRemoveAds() { return false; }
    }
}