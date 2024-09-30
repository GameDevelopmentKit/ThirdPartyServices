namespace Core.AdsServices.Native
{
    using System;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.Presenter;
    using GameFoundation.Scripts.UIModule.ScreenFlow.Managers;
    using GameFoundation.Scripts.Utilities.Extension;
    using R3;
    using UnityEngine;
    using UnityEngine.UI;
    using Zenject;

    public class NativeAdsView : MonoBehaviour
    {
        [SerializeField] private GameObject nonAdsHolder;
        [SerializeField] private GameObject adsHolder;

        public RawImage iconImage;
        public RawImage adChoicesImage;
        public Text     headlineText;
        public Text     advertiserText;
        public Text     callToActionText;

        private INativeAdsService nativeAdsService;
        private Collider[]        colliders;
        private bool              isEnable;
        private bool              isInit;

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

    public void ShowAds(bool isShow)
    {
        this.nonAdsHolder.SetActive(!isShow);
        this.adsHolder.SetActive(isShow);
        if (this.isInit && !this.isEnable && isShow)
        {
            this.isEnable = true;
            this.IntervalCall();
        }

        this.isEnable = isShow;
        foreach (var col in this.Colliders)
        {
            col.enabled = isShow;
        }
    }

    public void BindVisibleScreen(IScreenPresenter screenPresenter)
    {
        this.visibleScreen = screenPresenter;
    }

    public void Init(INativeAdsService nativeAdsService)
    {
        this.nativeAdsService = nativeAdsService;
        this.iconImage.gameObject.SetActive(false);
        this.adChoicesImage.gameObject.SetActive(false);
        this.isInit   = true;
        this.isEnable = true;
        this.IntervalCall();
        this.ShowAds(true);
    }

    private async void IntervalCall()
    {
        if (!this.isEnable) return;
        if (this == null) return;
        await UniTask.SwitchToMainThread();
        this.nativeAdsService?.DrawNativeAds(this);
        await UniTask.Delay(TimeSpan.FromSeconds(1));
        this.IntervalCall();
    }
#endif
}

}