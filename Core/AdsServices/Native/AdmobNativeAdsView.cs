namespace Core.AdsServices.Native
{
    using System;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.UI;

    public class AdmobNativeAdsView : NativeAdsView
    {
        public RawImage iconImage;
        public RawImage adChoicesImage;
        public Text     headlineText;
        public Text     advertiserText;
        public Text     callToActionText;

        private Collider[] colliders;
        private bool       isEnable;
        private bool       isInit;

        private Collider[] Colliders
        {
            get
            {
                this.colliders ??= this.GetComponentsInChildren<Collider>();

                return this.colliders;
            }
        }

        protected override bool ShowOnStart { get; set; } = false;

        #if ADMOB_NATIVE_ADS && !IMMERSIVE_ADS

        public override void ShowAds(bool isShow)
        {
            base.ShowAds(isShow);
            if (this.isInit && !this.isEnable && isShow)
            {
                this.isEnable = true;
                this.IntervalCall();
            }

            this.isEnable = isShow;
            foreach (var col in this.Colliders)
            {
                col.enabled = isShow;
            }
        }

        public void Init()
        {
            this.iconImage.gameObject.SetActive(false);
            this.adChoicesImage.gameObject.SetActive(false);
            this.isInit   = true;
            this.isEnable = true;
            this.IntervalCall();
            this.ShowAds(true);
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