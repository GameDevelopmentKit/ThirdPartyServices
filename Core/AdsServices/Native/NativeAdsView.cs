namespace Core.AdsServices.Native
{
    using System;
    using Cysharp.Threading.Tasks;
    using TMPro;
    using UnityEngine;

    public class NativeAdsView : MonoBehaviour
    {
        public GameObject icon;
        public TMP_Text   headlineText;


        private INativeAdsService nativeAdsService;

        public void Init(INativeAdsService nativeAdsService)
        {
            this.nativeAdsService = nativeAdsService;
            this.IntervalCall();
        }
        private async void IntervalCall()
        {
            this.nativeAdsService?.DrawNativeAds(this);
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            this.IntervalCall();
        }
    }
}