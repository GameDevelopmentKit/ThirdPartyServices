namespace Core.AdsServices.Native
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.Presenter;
    using GameFoundation.Scripts.UIModule.ScreenFlow.Managers;
    using TMPro;
    using UniRx;
    using UnityEngine;
    using UnityEngine.UI;

    public class NativeAdsView : MonoBehaviour
    {
        public RawImage  iconImage;
        public RawImage  adChoicesImage;
        public TMP_Text  headlineText;
        public TMP_Text  advertiserText;
        public TMP_Text  callToActionText;

        private INativeAdsService nativeAdsService;
        private IScreenManager    screenManager;
        private List<Type>        activeScreenList;

#if ADMOB_NATIVE_ADS
        public async void Init(INativeAdsService nativeAdsService,IScreenManager screenManager, List<Type> activeScreenList)
        {
            this.nativeAdsService = nativeAdsService;
            this.activeScreenList = activeScreenList;

            this.iconImage.gameObject.SetActive(false);
            this.adChoicesImage.gameObject.SetActive(false);
            this.IntervalCall();
            await UniTask.WaitUntil(() => screenManager.CurrentActiveScreen.HasValue);
            screenManager.CurrentActiveScreen.Subscribe(this.OnChangeScreenHandler);
        }

        private void OnChangeScreenHandler(IScreenPresenter obj)
        {
            if (obj == null)
            {
                this.gameObject.SetActive(false);
                return;
            }
            var isAdsActive = this.activeScreenList.Contains(obj.GetType());
            //TODO change to set active for ads elements
#if CREATIVE
            isAdsActive = false;
#endif
            this.gameObject.SetActive(isAdsActive);
        }

        private async void IntervalCall()
        {
            await UniTask.SwitchToMainThread();
            this.nativeAdsService?.DrawNativeAds(this);
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            this.IntervalCall();
        }
#endif
    }
}