namespace Core.AdsServices.ImmersiveAds
{
    using System;
    using System.Threading.Tasks;
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
    using System.Collections;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using PubScale.SdkOne.NativeAds;
    using TMPro;
#endif
    using Zenject;

#if ADMOB_NATIVE_ADS && IMMERSIVE_ADS
    [RequireComponent(typeof(NativeAdHolder))]
#endif
    public class ImmersiveAdsView : MonoBehaviour
    {
#if ADMOB_NATIVE_ADS && IMMERSIVE_ADS
        [SerializeField] private NativeAdHolder nativeAdHolder;
        [SerializeField] private GameObject     nativeAdStatusVisualiser;

        public GameObject      NativeAdStatusVisualiser => this.nativeAdStatusVisualiser;
        public NativeAdHolder  NativeAdHolder           => this.nativeAdHolder;
        public TextMeshProUGUI AdTagDisplay;

        private bool                    isAdLoaded;
        private bool                    autoRefreshAd;
        private CancellationTokenSource source;
        private IDisposable             changeScreenDisposable;
        private IScreenManager          screenManager;
        private ISignalBus              signalBus;
        private IScreenPresenter        visibleScreen;
        public  bool                    isAwake;
        private void                    OnValidate() { this.ValidateField(); }

        private void ValidateField()
        {
            this.nativeAdHolder ??= this.GetComponent<NativeAdHolder>();
            // this.nativeAdStatusVisualiser ??= this.GetComponentInChildren<NativeAdStatusVisualiser>();
        }

        private void Awake()
        {
            this.ValidateField();
            this.StartCoroutine(this.Initialize());
        }

        private IEnumerator Initialize()
        {
            while (this.GetCurrentContainer() == null)
            {
                yield return null;
            }

            var container = this.GetCurrentContainer();

            this.screenManager = container.Resolve<IScreenManager>();
            this.signalBus     = container.Resolve<ISignalBus>();

            this.changeScreenDisposable = this.screenManager.CurrentActiveScreen
                .Subscribe(this.OnChangeScreen);

            this.signalBus.Subscribe<ScreenShowSignal>(this.OnScreenShow);
            this.signalBus.Subscribe<ScreenCloseSignal>(this.OnScreenClose);

            this.nativeAdHolder.AutoFetch = false;
            this.nativeAdHolder.DisableAd(true);
            this.nativeAdHolder.Event_AdLoaded += this.OnAdLoaded;
            this.nativeAdHolder.Event_AdFailed += this.OnAdFailed;
            this.isAwake                       =  true;
        }

        private void OnAdLoaded(object arg1, NativeAdEventArgs arg2)
        {
            this.isAdLoaded = true;
            this.nativeAdHolder.DisableAd(false);
        }

        private void OnAdFailed(object arg1, LoadAdError arg2)
        {
            this.nativeAdHolder.DisableAd(true);
            this.isAdLoaded = false;
        }

        private void OnDestroy()
        {
            this.StopRefreshAd();
            this.changeScreenDisposable?.Dispose();
            this.signalBus.TrySubscribe<ScreenShowSignal>(this.OnScreenShow);
            this.signalBus.TrySubscribe<ScreenCloseSignal>(this.OnScreenClose);
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

            if (this.IsRemoveAds())
            {
                this.nativeAdHolder.DisableAd(true);
            }
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
            if (this.IsRemoveAds())
            {
                this.nativeAdHolder.DisableAd(true);

                return;
            }

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
            if (this.IsRemoveAds())
            {
                this.nativeAdHolder.DisableAd(true);

                return;
            }

            if (this.visibleScreen == null) return;
            if (!this.isAdLoaded) return;
            this.nativeAdHolder.DisableAd(this.visibleScreen != screenPresenter);
        }

        private bool IsRemoveAds()                                       { return PlayerPrefs.HasKey("ADMOB_REMOVE_ADS"); }
        public  void BindVisibleScreen(IScreenPresenter screenPresenter) { this.visibleScreen = screenPresenter; }
#endif
    }
}