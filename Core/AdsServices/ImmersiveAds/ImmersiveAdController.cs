namespace Core.AdsServices.ImmersiveAds
{
    using System;
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using Core.AnalyticServices.Signal;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.Presenter;
    using GameFoundation.Scripts.UIModule.ScreenFlow.Managers;
    using GameFoundation.Scripts.Utilities.LogService;
#if ADMOB_NATIVE_ADS && IMMERSIVE_ADS
    using GoogleMobileAds.Api;
    using PubScale.SdkOne.NativeAds;
#endif
    using R3;
    using ServiceImplementation.AdsServices.Signal;
    using UnityEngine;
    using Zenject;

    /// <summary>
    /// Note that GameObject must disable first before call bindScreen
    /// </summary>
#if ADMOB_NATIVE_ADS && IMMERSIVE_ADS
    [RequireComponent(typeof(NativeAdHolder))]
    public class ImmersiveAdController : MonoBehaviour
    {
        private NativeAdHolder nativeAdHolder => this.GetComponent<NativeAdHolder>();

        private IDisposable       changeScreenDisposable;
        private IScreenManager    screenManager;
        private IScreenPresenter  visibleScreen;
        private ISignalBus        signalBus        => ProjectContext.Instance.Container.Resolve<ISignalBus>();
        private ILogService       logService       => ProjectContext.Instance.Container.Resolve<ILogService>();
        private IAnalyticServices analyticServices => ProjectContext.Instance.Container.Resolve<IAnalyticServices>();
        private bool              IsAdsLoaded      { get; set; }

        private void Awake()
        {
            this.nativeAdHolder.Event_AdLoaded += this.OnAdLoaded;
            this.nativeAdHolder.Event_AdFailed += this.OnAdFailed;
            this.nativeAdHolder.Event_AdClicked += this.OnAdClicked;
            this.nativeAdHolder.Event_OnAdPaid += this.OnAdPaid;
            this.nativeAdHolder.Event_AdImpression += this.OnAdImpression;
            this.nativeAdHolder.Event_AdRequest += this.OnAdRequest;
        }

        public async UniTaskVoid BindScreen(IScreenPresenter screenPresenter, string adsTag, ScreenManager screenManager, Canvas c = null)
        {
#if CREATIVE ||DISABLE_IMMERSIVE
            this.gameObject.SetActive(false);
            return;
#endif
            await UniTask.WaitForSeconds(0.2f);
            this.screenManager = screenManager;
            this.visibleScreen = screenPresenter;

            this.nativeAdHolder.canvas = c != null ? c : this.screenManager.RootUICanvas.GetComponentInChildren<Canvas>(true);
            this.nativeAdHolder.adTag = adsTag;

            this.nativeAdHolder.DisableAd(false);
            this.nativeAdHolder.AutoFetch = true;
            this.gameObject.SetActive(true);

            this.changeScreenDisposable = this.screenManager.CurrentActiveScreen
                .Subscribe(this.OnChangeScreen);

            if (this.IsRemoveAds())
            {
                this.nativeAdHolder.StopRefresh();
                this.gameObject.SetActive(false);
                this.nativeAdHolder.DisableAd(true);
            }
        }

        private void ShowAds(bool show) { this.nativeAdHolder.UnHideAd(); }

        private void OnAdImpression(object arg1, EventArgs arg2)
        {
            this.logService.Log($"Immersive Ads Impression: {arg1}\n{arg2}");

            this.signalBus.Fire(new NativeAdsShowSignal());
        }

        private void OnAdRequest()
        {
            if (this.IsRemoveAds())
            {
                return;
            }

            this.logService.Log("Immersive Ads Request");
            this.signalBus.Fire(new NativeAdsRequestSignal());
        }

        private void OnAdPaid(AdValue obj)
        {
            var adsRevenueEvent = new AdsRevenueEvent
            {
                AdsRevenueSourceId = AdRevenueConstants.ARSourceImmersiveAds,
                Revenue = obj.Value / 1e6,
                Currency = "USD",
                Placement = "ImmersiveAds",
                AdNetwork = "AdMob"
            };

            this.analyticServices.Track(adsRevenueEvent);
            this.signalBus.Fire(new AdRevenueSignal(adsRevenueEvent));
        }

        private void OnAdClicked(object arg1, EventArgs arg2) { this.signalBus.Fire(new NativeAdClickSignal()); }

        private void OnChangeScreen(IScreenPresenter screenPresenter)
        {
            if (screenPresenter == null)
            {
                return;
            }

            if (this.IsRemoveAds())
            {
                this.nativeAdHolder.DisableAd(true);

                return;
            }

            if (this.visibleScreen == null) return;

            var isShow = this.visibleScreen == screenPresenter && this.IsAdsLoaded;

            this.ShowAds(isShow);
        }

        private void OnAdFailed(object arg1, LoadAdError arg2)
        {
            this.logService.Log($"Immersive Ads Failed: {arg1}\nError: {arg2.GetResponseInfo()}");
            this.signalBus.Fire(new NativeAdsLoadFailedSignal());
            this.ShowAds(false);
            this.IsAdsLoaded = false;
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
            this.IsAdsLoaded = true;

            if (!this.IsRemoveAds())
            {
                this.ShowAds(true);
            }
        }

        private bool IsRemoveAds() { return PlayerPrefs.HasKey("ADMOB_REMOVE_ADS"); }

        private void OnDestroy() { this.changeScreenDisposable?.Dispose(); }
    }
#else
    public class ImmersiveAdController : MonoBehaviour
    {
        public async UniTaskVoid BindScreen(IScreenPresenter screenPresenter, string adsTag, ScreenManager screenManager, Canvas c = null) { this.gameObject.SetActive(false); }
    }
#endif
}