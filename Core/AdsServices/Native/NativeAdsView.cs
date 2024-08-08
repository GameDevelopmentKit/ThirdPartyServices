namespace Core.AdsServices.Native
{
    using System;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.UI;

    public class NativeAdsView : MonoBehaviour
    {
        public RawImage iconImage;
        public RawImage adChoicesImage;
        public Text     headlineText;
        public Text     advertiserText;
        public Text     callToActionText;

        private INativeAdsService nativeAdsService;
        private Collider[]        colliders;
        private bool              isEnable;
        private bool              isInit;

#if ADMOB_NATIVE_ADS && !IMMERSIVE_ADS

        private void Awake()
        {
            this.colliders = this.GetComponentsInChildren<Collider>();
        }
        
        public void ShowAds(bool isShow)
        {
            if (this.isInit && !this.isEnable && isShow)
            {
                this.isEnable = true;
                this.IntervalCall();
            }
            this.isEnable = isShow;
            foreach (var col in this.colliders)
            {
                col.enabled = isShow;
            }
        }

        public void Init(INativeAdsService nativeAdsService)
        {
            this.nativeAdsService = nativeAdsService;
            this.iconImage.gameObject.SetActive(false);
            this.adChoicesImage.gameObject.SetActive(false);
            this.isInit   = true;
            this.isEnable = true;
            this.IntervalCall();
        }

        private async void IntervalCall()
        {
            if (!this.isEnable) return;
            if (this == null) return;
            await UniTask.SwitchToMainThread();
            this.nativeAdsService?.DrawNativeAds(this);
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            this.IntervalCall();
        }
#endif
    }
}