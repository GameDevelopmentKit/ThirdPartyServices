namespace Core.AdsServices.Native
{
    using System;
    using Cysharp.Threading.Tasks;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class NativeAdsView : MonoBehaviour
    {
        public RawImage iconImage;
        public RawImage adChoicesImage;
        public TMP_Text headlineText;
        public TMP_Text advertiserText;


        private INativeAdsService nativeAdsService;

        public void Init(INativeAdsService nativeAdsService)
        {
            this.iconImage.gameObject.SetActive(false);
            this.adChoicesImage.gameObject.SetActive(false);
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