namespace Core.AdsServices.ImmersiveAds
{
    using System;
    using System.Threading.Tasks;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.Presenter;
    using GameFoundation.Scripts.UIModule.ScreenFlow.Managers;
    using PubScale.SdkOne.NativeAds;
    using R3;
    using UnityEngine;
    using Zenject;

    [RequireComponent(typeof(NativeAdHolder))]
    public class ImmersiveAdsView : MonoBehaviour
    {
        [SerializeField] private NativeAdHolder nativeAdHolder;

        private TaskCompletionSource<bool> isInjected = new(false);
        private IDisposable                changeScreenDisposable;

        private IScreenManager   screenManager;
        private IScreenPresenter visibleScreen;

        [Inject]
        private void Init(IScreenManager screenManager)
        {
            this.screenManager = screenManager;
            this.isInjected.TrySetResult(true);
        }

        private void OnValidate()
        {
            this.nativeAdHolder ??= this.GetComponent<NativeAdHolder>();
        }

        private async void Awake()
        {
            this.nativeAdHolder ??= this.GetComponent<NativeAdHolder>();
            await this.isInjected.Task;
            this.changeScreenDisposable = this.screenManager.CurrentActiveScreen.Subscribe(this.OnChangeScreen);
        }

        private void OnDestroy()
        {
            this.changeScreenDisposable?.Dispose();
        }

        private void OnChangeScreen(IScreenPresenter screenPresenter)
        {
            this.nativeAdHolder.DisableAd(this.visibleScreen != screenPresenter);
        }

        public NativeAdHolder NativeAdHolder => this.nativeAdHolder;

        public void BindVisibleScreen(IScreenPresenter screenPresenter)
        {
            this.visibleScreen = screenPresenter;
        }
    }
}