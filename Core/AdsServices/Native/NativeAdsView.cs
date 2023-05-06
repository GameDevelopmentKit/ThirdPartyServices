namespace Core.AdsServices.Native
{
    using System;
    using System.Collections.Generic;
    using Core.AdsServices.Signals;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.UIModule.ScreenFlow.Signals;
    using GameFoundation.Scripts.Utilities.LogService;
    using GoogleMobileAds.Api;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    using Zenject;

    public class NativeAdsView : MonoBehaviour
    {
        #region Inject

        protected SignalBus         signalBus;
        protected INativeAdsService nativeAdsService;
        protected List<Type>        activeScreenList;
        protected ILogService       logService;

        #endregion

        public RawImage iconImage;
        public RawImage adChoicesImage;
        public TMP_Text headlineText;
        public TMP_Text callToActionText;

#if ADMOB_NATIVE_ADS
        public void Init(INativeAdsService nativeAdsService, SignalBus signalBus, List<Type> activeScreenList, ILogService logService)
        {
            this.nativeAdsService = nativeAdsService;
            this.signalBus        = signalBus;
            this.activeScreenList = activeScreenList;
            this.logService       = logService;

            this.iconImage.gameObject.SetActive(false);
            this.adChoicesImage.gameObject.SetActive(false);
            this.IntervalCall();
            this.signalBus.Subscribe<ScreenShowSignal>(this.OnScreenShowHandler);
        }

        private void OnDestroy()
        {
            this.signalBus.Unsubscribe<ScreenShowSignal>(this.OnScreenShowHandler);
        }

        private void OnScreenShowHandler(ScreenShowSignal obj)
        {
            var isAdsActive = this.activeScreenList.Contains(obj.ScreenPresenter.GetType());
            //TODO change to set active for ads elements
#if CREATIVE
            isAdsActive = false;
#endif
            this.gameObject.SetActive(isAdsActive);
        }

        private async void IntervalCall()
        {
            await UniTask.SwitchToMainThread();
            this.DrawNativeAds();
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            this.IntervalCall();
        }

        public virtual void BindData(NativeAd nativeAd)
        {
            this.logService.Log($"Start set native ad: {this.name}");
            
            
            if (nativeAd.GetHeadlineText() != null)
            {
                this.logService.Log($"native headline: {nativeAd.GetHeadlineText()}");
                
                this.headlineText.text = nativeAd.GetHeadlineText();
                
                if (!nativeAd.RegisterHeadlineTextGameObject(this.headlineText.gameObject))
                {
                    // Handle failure to register ad asset.
                    this.logService.Log($"Failed to register Headline text for native ad: {this.name}");
                }
            }

            if (nativeAd.GetIconTexture() != null)
            {
                this.logService.Log($"native icon: {nativeAd.GetIconTexture().texelSize}");
                
                this.iconImage.gameObject.SetActive(true);
                this.iconImage.texture = nativeAd.GetIconTexture();

                // Register GameObject that will display icon asset of native ad.
                if (!nativeAd.RegisterIconImageGameObject(this.iconImage.gameObject))
                {
                    // Handle failure to register ad asset.
                    this.logService.Log($"Failed to register icon image for native ad: {this.name}");
                }
            }

            if (nativeAd.GetAdChoicesLogoTexture() != null)
            {
                this.logService.Log($"native ad choice: {nativeAd.GetAdChoicesLogoTexture().texelSize}");
                
                this.adChoicesImage.gameObject.SetActive(true);
                this.adChoicesImage.texture = nativeAd.GetAdChoicesLogoTexture();

                if (!nativeAd.RegisterAdChoicesLogoGameObject(this.adChoicesImage.gameObject))
                {
                    // Handle failure to register ad asset.
                    this.logService.Log($"Failed to register ad choices image for native ad: {this.name}");
                }
            }

            if (nativeAd.GetCallToActionText() != null)
            {
                this.logService.Log($"native call to action text: {nativeAd.GetCallToActionText()}");
                
                this.callToActionText.text = nativeAd.GetCallToActionText();
                if (!nativeAd.RegisterCallToActionGameObject(this.callToActionText.gameObject))
                {
                    // Handle failure to register ad asset
                    this.logService.Log($"Failed to register call to action text for native ad: {this.name}");
                }
            }
        }

        private void DrawNativeAds()
        {
            this.signalBus.Fire(new DrawNativeAdRequestSignal(this));
        }
#endif
    }
}