namespace Core.AdsServices.ImmersiveAds
{
    using System;
    using System.Threading.Tasks;
    using GameFoundation.DI;
    using GameFoundation.Signals;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.Presenter;
    using GameFoundation.Scripts.UIModule.ScreenFlow.Managers;
    using GameFoundation.Scripts.UIModule.ScreenFlow.Signals;
    using GameFoundation.Scripts.Utilities.Extension;
    #if ADMOB_NATIVE_ADS
    using GoogleMobileAds.Api;
    #endif
    using R3;
    using UnityEngine;
    #if ADMOB_NATIVE_ADS && IMMERSIVE_ADS
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using PubScale.SdkOne.NativeAds;
    #endif

    #if ADMOB_NATIVE_ADS && IMMERSIVE_ADS
    [RequireComponent(typeof(NativeAdHolder))]
    #endif
    public class ImmersiveAdsView : MonoBehaviour
    {
        #if ADMOB_NATIVE_ADS && IMMERSIVE_ADS
        [SerializeField] private NativeAdHolder           nativeAdHolder;
        [SerializeField] private NativeAdStatusVisualiser nativeAdStatusVisualiser;

        public NativeAdStatusVisualiser NativeAdStatusVisualiser => this.nativeAdStatusVisualiser;
        public NativeAdHolder           NativeAdHolder           => this.nativeAdHolder;

        private bool                    isAdLoaded;
        private bool                    autoRefreshAd;
        private CancellationTokenSource source;
        private IDisposable             changeScreenDisposable;
        private IScreenManager          screenManager;
        private SignalBus               signalBus;
        private IScreenPresenter        visibleScreen;

        private void OnValidate()
        {
            this.ValidateField();
        }

        private void ValidateField()
        {
            this.nativeAdHolder ??= this.GetComponent<NativeAdHolder>();
            this.nativeAdStatusVisualiser ??= this.GetComponentInChildren<NativeAdStatusVisualiser>();
        }

        private void Awake()
        {
            this.ValidateField();
            this.screenManager = this.GetCurrentContainer().Resolve<IScreenManager>();
            this.signalBus = this.GetCurrentContainer().Resolve<SignalBus>();
            this.changeScreenDisposable = this.screenManager.CurrentActiveScreen.Subscribe(this.OnChangeScreen);
            this.signalBus.Subscribe<ScreenShowSignal>(this.OnScreenShow);
            this.signalBus.Subscribe<ScreenCloseSignal>(this.OnScreenClose);

            this.nativeAdHolder.AutoFetch = false;
            this.nativeAdHolder.DisableAd(true);
            this.nativeAdHolder.Event_AdLoaded += this.OnAdLoaded;
            this.nativeAdHolder.Event_AdFailed += this.OnAdFailed;
        }

        private void OnAdLoaded(object arg1, NativeAdEventArgs arg2)
        {
            this.isAdLoaded = true;
            this.nativeAdHolder.DisableAd(false);
        }

        private void OnAdFailed(object arg1, AdFailedToLoadEventArgs arg2)
        {
            this.nativeAdHolder.DisableAd(true);
            this.isAdLoaded = false;
        }

        private void OnDestroy()
        {
            this.StopRefreshAd();
            this.changeScreenDisposable?.Dispose();
            this.signalBus.Unsubscribe<ScreenCloseSignal>(this.OnScreenClose);
        }

        private void OnScreenShow(ScreenShowSignal obj)
        {
            if (this.visibleScreen == null) return;
            if (this.visibleScreen != obj.ScreenPresenter) return;
            this.StartRefreshAd();
        }

        private void OnScreenClose(ScreenCloseSignal obj)
        {
            if (this.visibleScreen == null) return;
            if (this.visibleScreen != obj.ScreenPresenter) return;
            this.StopRefreshAd();
        }

        private void StartRefreshAd()
        {
            this.autoRefreshAd = true;
            this.RefreshAd();
        }

        private void StopRefreshAd()
        {
            this.autoRefreshAd = false;
            this.source?.Cancel();
        }

        private async void RefreshAd()
        {
            const float refreshAdTime = 15f;

            if (!this.autoRefreshAd) return;
            this.nativeAdHolder.FetchAd();
            this.source = new CancellationTokenSource();
            Debug.Log($"Refresh Immersive Ads: {this.nativeAdHolder.adTag}");
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(refreshAdTime), DelayType.DeltaTime, cancellationToken: this.source.Token);
                this.RefreshAd();
            }
            catch (Exception)
            {
                Debug.Log($"Stop Refresh Immersive Ads: {this.nativeAdHolder.adTag}\"");
            }
        }

        private void OnChangeScreen(IScreenPresenter screenPresenter)
        {
            if (this.visibleScreen == null) return;
            if (!this.isAdLoaded) return;
            this.nativeAdHolder.DisableAd(this.visibleScreen != screenPresenter);
        }

        public void BindVisibleScreen(IScreenPresenter screenPresenter)
        {
            this.visibleScreen = screenPresenter;
        }
        #endif
    }
}