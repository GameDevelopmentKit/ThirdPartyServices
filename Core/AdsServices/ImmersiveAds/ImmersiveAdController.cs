namespace Core.AdsServices.ImmersiveAds
{
    using System;
    using System.Threading;
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using Core.AnalyticServices.Signal;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.Presenter;
    using GameFoundation.Scripts.UIModule.ScreenFlow.Managers;
    using GameFoundation.Scripts.Utilities.LogService;
    using GoogleMobileAds.Api;
    using PubScale.SdkOne.NativeAds;
    using R3;
    using ServiceImplementation.AdsServices.Signal;
    using UnityEngine;
    using Zenject;

#if ADMOB_NATIVE_ADS && IMMERSIVE_ADS
    [RequireComponent(typeof(NativeAdHolder))]
#endif
    public class ImmersiveAdController : MonoBehaviour
    {
#if ADMOB_NATIVE_ADS && IMMERSIVE_ADS
        private NativeAdHolder nativeAdHolder;

        public NativeAdHolder NativeAdHolder => this.nativeAdHolder ??= this.GetComponent<NativeAdHolder>();

        private bool                    isAdLoaded;
        private bool                    autoRefreshAd;
        private CancellationTokenSource source;
        private IDisposable             changeScreenDisposable;
        private IScreenManager          screenManager;
        private ISignalBus              signalBus;
        private IScreenPresenter        visibleScreen;
        private ILogService             logService;
        private IAnalyticServices       analyticServices;

        private void Awake()
        {
            var container = ProjectContext.Instance.Container;

            this.screenManager = container.Resolve<IScreenManager>();
            this.signalBus     = container.Resolve<ISignalBus>();

            this.changeScreenDisposable = this.screenManager.CurrentActiveScreen
                .Subscribe(this.OnChangeScreen);

            this.NativeAdHolder.Event_AdLoaded  += this.OnAdLoaded;
            this.NativeAdHolder.Event_AdFailed  += this.OnAdFailed;
            this.NativeAdHolder.Event_AdClicked += this.OnAdClicked;

            this.NativeAdHolder.Event_OnAdPaid     += this.OnAdPaid;
            this.NativeAdHolder.Event_AdImpression += this.OnAdImpression;
            this.NativeAdHolder.Event_AdRequest    += this.OnAdRequest;

            if (this.IsRemoveAds())
            {
                this.NativeAdHolder.StopRefresh();
                this.NativeAdHolder.DisableAd(true);
                this.gameObject.SetActive(false);
            }
        }

        public void BindScreen(IScreenPresenter screenPresenter, string adsTag, Canvas c = null)
        {
            this.visibleScreen        = screenPresenter;
            this.NativeAdHolder.adTag = adsTag;

            if (c == null)
            {
                var canvas = this.screenManager.RootUICanvas.GetComponentInChildren<Canvas>(true);
                this.nativeAdHolder.canvas = canvas;
            }
        }

        private void OnAdImpression(object arg1, EventArgs arg2)
        {
            this.logService.Log($"Immersive Ads Impression: {arg1}\n{arg2}");

            this.signalBus.Fire(new NativeAdsShowSignal());
        }

        private void OnAdRequest()
        {
            this.logService.Log("Immersive Ads Request");
            this.signalBus.Fire(new NativeAdsRequestSignal());
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

        private void OnAdClicked(object arg1, EventArgs arg2) { this.signalBus.Fire(new NativeAdClickSignal()); }

        private void OnChangeScreen(IScreenPresenter screenPresenter)
        {
            if (this.IsRemoveAds())
            {
                this.nativeAdHolder.DisableAd(true);

                return;
            }

            if (this.visibleScreen == null) return;

            this.nativeAdHolder.DisableAd(this.visibleScreen != screenPresenter);
        }

        private void OnAdFailed(object arg1, LoadAdError arg2)
        {
            this.logService.Log($"Immersive Ads Failed: {arg1}\nError: {arg2.GetResponseInfo()}");
            this.signalBus.Fire(new NativeAdsLoadFailedSignal());
        }

        private void OnAdLoaded(object arg1, NativeAdEventArgs arg2)
        {
            if (arg2?.nativeAd == null || arg1 == null)
            {
                this.logService.Log($"Immersive Ads Loaded: {arg1}\nNative Ads: NULL");

                return;
            }

            this.logService.Log($"Immersive Ads Loaded: {arg1}\nNative Ads: {arg2.nativeAd}");
            this.signalBus.Fire(new NativeAdsLoadedSignal());
            this.nativeAdHolder.DisableAd(false);
        }

        private bool IsRemoveAds() { return PlayerPrefs.HasKey("ADMOB_REMOVE_ADS"); }

        private void OnDestroy() { this.changeScreenDisposable?.Dispose(); }
#endif
    }
}