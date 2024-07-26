namespace Core.AdsServices.ImmersiveAds
{
    using System;
    using System.Threading.Tasks;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.Presenter;
    using GameFoundation.Scripts.UIModule.ScreenFlow.Managers;
    using GameFoundation.Scripts.Utilities.Extension;
    using R3;
    using UnityEngine;
#if ADMOB_NATIVE_ADS && IMMERSIVE_ADS
    using PubScale.SdkOne.NativeAds;
#endif
    using Zenject;

#if ADMOB_NATIVE_ADS && IMMERSIVE_ADS
    [RequireComponent(typeof(NativeAdHolder))]
#endif
    public class ImmersiveAdsView : MonoBehaviour
    {
#if ADMOB_NATIVE_ADS && IMMERSIVE_ADS
        [SerializeField] private NativeAdHolder nativeAdHolder;
        public NativeAdHolder NativeAdHolder => this.nativeAdHolder;
#endif

        private IDisposable changeScreenDisposable;

        private IScreenManager   screenManager;
        private IScreenPresenter visibleScreen;

#if ADMOB_NATIVE_ADS && IMMERSIVE_ADS && UNITY_EDITOR
        private void OnValidate()
        {
            this.nativeAdHolder ??= this.GetComponent<NativeAdHolder>();
        }
#endif

        private void Awake()
        {
#if ADMOB_NATIVE_ADS && IMMERSIVE_ADS
            this.nativeAdHolder         ??= this.GetComponent<NativeAdHolder>();
#endif
            this.screenManager          =   this.GetCurrentContainer().Resolve<IScreenManager>();
            this.changeScreenDisposable =   this.screenManager.CurrentActiveScreen.Subscribe(this.OnChangeScreen);
        }

        private void OnDestroy()
        {
            this.changeScreenDisposable?.Dispose();
        }

        private void OnChangeScreen(IScreenPresenter screenPresenter)
        {
#if ADMOB_NATIVE_ADS && IMMERSIVE_ADS
            this.nativeAdHolder.DisableAd(this.visibleScreen != screenPresenter);
#endif
        }

        public void BindVisibleScreen(IScreenPresenter screenPresenter)
        {
            this.visibleScreen = screenPresenter;
        }
    }
}