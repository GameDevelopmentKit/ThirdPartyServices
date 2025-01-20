namespace Core.AdsServices.Native
{
    using System;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.Presenter;
    using GameFoundation.Scripts.UIModule.ScreenFlow.Managers;
    using GameFoundation.DI;
    using R3;
    using UnityEngine;

    public class NativeAdsView : MonoBehaviour
    {
        [SerializeField] protected GameObject nonAdsHolder;
        [SerializeField] protected GameObject adsHolder;

        protected IDisposable       changeScreenDisposable;
        protected IScreenPresenter  visibleScreen;
        protected IScreenManager    screenManager;
        protected INativeAdsService nativeAdsService;

        protected virtual bool ShowOnStart { get; set; } = true;

        private void Awake()
        {
            this.screenManager          = this.GetCurrentContainer().Resolve<IScreenManager>();
            this.nativeAdsService       = this.GetCurrentContainer().Resolve<INativeAdsService>();
            this.changeScreenDisposable = this.screenManager.CurrentActiveScreen.Subscribe(this.OnChangeScreen);

            if (this.ShowOnStart)
            {
                this.ShowAds(true);
            }
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

        public virtual void ShowAds(bool isShow)
        {
            var enable = isShow && !this.nativeAdsService.IsRemoveAds();
            this.nonAdsHolder.SetActive(!enable);
            this.adsHolder.SetActive(enable);
        }

        public void BindVisibleScreen(IScreenPresenter screenPresenter)
        {
            this.visibleScreen = screenPresenter;
        }
    }
}