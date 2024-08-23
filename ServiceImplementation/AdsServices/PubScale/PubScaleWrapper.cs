#if ADMOB_NATIVE_ADS && IMMERSIVE_ADS
namespace ServiceImplementation.AdsServices.PubScale
{
    using System;
    using System.Collections.Generic;
    using Core.AdsServices.ImmersiveAds;
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using Core.AnalyticServices.Signal;
    using GameFoundation.Scripts.UIModule.ScreenFlow.Managers;
    using GameFoundation.Scripts.Utilities.LogService;
    using global::PubScale.SdkOne.NativeAds;
    using GoogleMobileAds.Api;
    using ServiceImplementation.Configs;
    using UnityEngine;
    using Zenject;

    public class PubScaleWrapper : IImmersiveAdsService
    {
#region Inject

        private readonly IScreenManager     screenManager;
        private readonly SignalBus          signalBus;
        private readonly IAnalyticServices  analyticServices;
        private readonly ILogService        logService;
        private readonly ThirdPartiesConfig thirdPartiesConfig;

#endregion

        private readonly HashSet<NativeAdHolder> cacheNativeAdHolder = new();
        private          Canvas                  cacheCanvas;

        public PubScaleWrapper
        (
            IScreenManager     screenManager,
            SignalBus          signalBus,
            IAnalyticServices  analyticServices,
            ILogService        logService,
            ThirdPartiesConfig thirdPartiesConfig
        )
        {
            this.screenManager      = screenManager;
            this.signalBus          = signalBus;
            this.analyticServices   = analyticServices;
            this.logService         = logService;
            this.thirdPartiesConfig = thirdPartiesConfig;
        }

#region Immersive Ads

        public void InitNativeAdHolder(ImmersiveAdsView immersiveAdsView, string placement, bool worldSpace = false)
        {
            var nativeAdHolder = immersiveAdsView.NativeAdHolder;
            if (!worldSpace)
            {
                var canvas = this.cacheCanvas ?? this.screenManager.RootUICanvas.GetComponentInChildren<Canvas>();
                this.cacheCanvas      = canvas;
                nativeAdHolder.canvas = this.cacheCanvas;
            }

            immersiveAdsView.NativeAdStatusVisualiser.gameObject.SetActive(this.thirdPartiesConfig.AdSettings.ImmersiveAds.UserTestMode);
            if (this.thirdPartiesConfig.AdSettings.ImmersiveAds.UserTestMode)
            {
                immersiveAdsView.NativeAdStatusVisualiser.AdTagDisplay.text = placement;
            }

            nativeAdHolder.adTag = placement;
            nativeAdHolder.StopRefresh();
            nativeAdHolder.FetchAd();
            if (this.cacheNativeAdHolder.TryGetValue(nativeAdHolder, out _)) return;

            this.cacheNativeAdHolder.Add(nativeAdHolder);
            nativeAdHolder.Event_OnAdPaid     += this.OnAdPaid;
            nativeAdHolder.Event_AdImpression += this.OnAdImpression;
            nativeAdHolder.Event_AdFailed     += this.OnAdFailed;
            nativeAdHolder.Event_AdLoaded     += this.OnAdLoaded;
            nativeAdHolder.Event_AdRequest    += this.OnAdRequest;
        }

        private void OnAdImpression(object arg1, EventArgs arg2)
        {
            this.logService.Log($"Immersive Ads Impression: {arg1}\n{arg2}");
        }

        private void OnAdFailed(object arg1, AdFailedToLoadEventArgs arg2)
        {
            this.logService.Log($"Immersive Ads Failed: {arg1}\nError: {arg2.LoadAdError.GetResponseInfo()}");
        }

        private void OnAdLoaded(object arg1, NativeAdEventArgs arg2)
        {
            if (arg2?.nativeAd == null || arg1 == null)
            {
                this.logService.Log($"Immersive Ads Loaded: {arg1}\nNative Ads: NULL");
                return;
            }
            this.logService.Log($"Immersive Ads Loaded: {arg1}\nNative Ads: {arg2.nativeAd}");
        }

        private void OnAdRequest()
        {
            this.logService.Log("Immersive Ads Request");
        }

        private void OnAdPaid(AdValue obj)
        {
            var adsRevenueEvent = new AdsRevenueEvent
                                  {
                                      AdsRevenueSourceId = AdRevenueConstants.ARSourceImmersiveAds,
                                      Revenue            = obj.Value / 1e6,
                                      Currency           = "USD",
                                      Placement          = "ImmersiveAds",
                                      AdNetwork          = "AdMob"
                                  };

            this.analyticServices.Track(adsRevenueEvent);
            this.signalBus.Fire(new AdRevenueSignal(adsRevenueEvent));
        }

#endregion
    }
}
#endif