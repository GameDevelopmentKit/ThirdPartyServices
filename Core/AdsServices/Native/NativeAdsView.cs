namespace Core.AdsServices.Native
{
    using System;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.Presenter;
    using GameFoundation.Scripts.UIModule.ScreenFlow.Managers;
    using GameFoundation.DI;
    using R3;
    using UnityEngine;
    using UnityEngine.UI;

    public class NativeAdsView : MonoBehaviour
    {
        [SerializeField] [NativePlacement] private string     placement;
        [SerializeField] [Range(1, 300)]   private float      refreshSeconds = 10f;
        [SerializeField]                   private GameObject nonAdsHolder;
        [SerializeField]                   private GameObject adsHolder;

        public RawImage iconImage;
        public RawImage adChoicesImage;
        public Text     headlineText;
        public Text     advertiserText;
        public Text     callToActionText;

        public string Placement => this.placement;

        #if ADMOB_NATIVE_ADS && !IMMERSIVE_ADS

        private IAdMobNativeAdsService nativeAdsService;
        #endif

        private const float RefreshTimeout = 5f;

        private Collider[]              colliders;
        private CancellationTokenSource refreshCts;
        private bool                    refreshing;

        private IDisposable      changeScreenDisposable;
        private IScreenPresenter visibleScreen;
        private IScreenManager   screenManager;

        private Collider[] Colliders
        {
            get
            {
                this.colliders ??= this.GetComponentsInChildren<Collider>();

                return this.colliders;
            }
        }

        #if ADMOB_NATIVE_ADS && !IMMERSIVE_ADS
        private void Awake()
        {
            this.screenManager          = this.GetCurrentContainer().Resolve<IScreenManager>();
            this.changeScreenDisposable = this.screenManager.CurrentActiveScreen.Subscribe(this.OnChangeScreen);
        }

        private void OnDestroy()
        {
            this.ShowAds(false);
            this.changeScreenDisposable?.Dispose();
        }

        private void OnChangeScreen(IScreenPresenter screenPresenter)
        {
            if (this.visibleScreen == null) return;
            this.ShowAds(this.visibleScreen == screenPresenter);
        }

        private void ShowAds(bool isShow)
        {
            this.nonAdsHolder.SetActive(!isShow);
            this.adsHolder.SetActive(isShow);
            foreach (var col in this.Colliders)
            {
                col.enabled = isShow;
            }
        }

        private async UniTaskVoid CreateNativeAdsAsync()
        {
            this.refreshing = true;
            await UniTask.SwitchToMainThread();
            this.nativeAdsService?.CreateNativeAds(this);
        }

        private async UniTaskVoid RefreshAsync()
        {
            if (!this.refreshing) return;
            this.refreshCts = new CancellationTokenSource();
            await UniTask.Delay(TimeSpan.FromSeconds(this.refreshSeconds), DelayType.DeltaTime, cancellationToken: this.refreshCts.Token);
            this.nativeAdsService.ReleaseNativeAds(this.placement);
            UniTask.WaitUntil(() => this.nativeAdsService.IsNativeAdsReady(this.placement))
               .TimeoutWithoutException(TimeSpan.FromSeconds(RefreshTimeout))
               .ContinueWith(_ =>
                             {
                                 this.CreateNativeAdsAsync().Forget();
                                 this.RefreshAsync().Forget();
                             }).Forget();
        }

        public void BindVisibleScreen(IScreenPresenter screenPresenter)
        {
            this.visibleScreen = screenPresenter;
        }

        public void Init(INativeAdsService service)
        {
            this.nativeAdsService = (IAdMobNativeAdsService)service;
            this.iconImage.gameObject.SetActive(false);
            this.adChoicesImage.gameObject.SetActive(false);
            this.CreateNativeAdsAsync().Forget();
            this.RefreshAsync().Forget();
            this.ShowAds(true);
        }

        public void Release()
        {
            this.refreshing = false;
            this.refreshCts?.Cancel();
            this.nativeAdsService.ReleaseNativeAds(this.placement);
        }

        #endif
    }
}