namespace Core.AdsServices.Native
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.UIModule.ScreenFlow.Signals;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    using Zenject;

    public class NativeAdsView : MonoBehaviour
    {
        public RawImage iconImage;
        public RawImage adChoicesImage;
        public TMP_Text headlineText;
        public TMP_Text advertiserText;


        private SignalBus         signalBus;
        private INativeAdsService nativeAdsService;
        private List<Type>        activeScreenList;
        private List<Collider>    colliders;

        public void Init(INativeAdsService nativeAdsService, SignalBus signalBus, List<Type> activeScreenList)
        {
            this.nativeAdsService = nativeAdsService;
            this.signalBus        = signalBus;
            this.activeScreenList = activeScreenList;

            this.iconImage.gameObject.SetActive(false);
            this.adChoicesImage.gameObject.SetActive(false);
            this.IntervalCall();
            this.signalBus.Subscribe<ScreenShowSignal>(this.OnScreenShowHandler);
        }

        private void OnDestroy() { this.signalBus.Unsubscribe<ScreenShowSignal>(this.OnScreenShowHandler); }

        private void OnScreenShowHandler(ScreenShowSignal obj)
        {
            var isAdsActive = this.activeScreenList.Contains(obj.ScreenPresenter.GetType());
            foreach (var collider in this.colliders)
            {
                collider.enabled = isAdsActive;
            }
        }

        private async void IntervalCall()
        {
            await UniTask.SwitchToMainThread();
            this.nativeAdsService?.DrawNativeAds(this);
            this.colliders = this.GetComponentsInChildren<Collider>(true).ToList();
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            this.IntervalCall();
        }
    }
}