namespace Core.AdsServices.Native
{
    using System;
    using Cysharp.Threading.Tasks;
    using TMPro;
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

#if ADMOB_NATIVE_ADS
        public void Init(INativeAdsService nativeAdsService)
        {
            this.nativeAdsService = nativeAdsService;

            this.iconImage.gameObject.SetActive(false);
            this.adChoicesImage.gameObject.SetActive(false);
            this.IntervalCall();
        }

        private async void IntervalCall()
        {
            if (this == null) return;
            await UniTask.SwitchToMainThread();
            this.nativeAdsService?.DrawNativeAds(this);
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            this.IntervalCall();
        }
#endif
    }
}