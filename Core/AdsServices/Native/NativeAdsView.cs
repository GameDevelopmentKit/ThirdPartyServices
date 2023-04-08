namespace Core.AdsServices.Native
{
    using System;
    using Cysharp.Threading.Tasks;
    using TMPro;
    using UnityEngine;

    public class NativeAdsView : MonoBehaviour
    {
        public GameObject icon;
        public GameObject adChoices;
        public TMP_Text   headlineText;
        public TMP_Text   advertiserText;


        private INativeAdsService nativeAdsService;

        public void Init(INativeAdsService nativeAdsService)
        {
            this.icon.gameObject.SetActive(false);
            this.adChoices.gameObject.SetActive(false);
            this.nativeAdsService = nativeAdsService;
            this.IntervalCall();
        }
        private async void IntervalCall()
        {
            await UniTask.SwitchToMainThread();
            this.nativeAdsService?.DrawNativeAds(this);
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            this.IntervalCall();
        }
    }
}