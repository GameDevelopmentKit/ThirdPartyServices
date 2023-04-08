namespace Core.AdsServices.Native
{
    using TMPro;
    using UnityEngine;

    public class NativeAdsView : MonoBehaviour
    {
        public GameObject icon;
        public TMP_Text   headlineText;


        private INativeAdsService nativeAdsService;

        public void Init(INativeAdsService nativeAdsService) { this.nativeAdsService = nativeAdsService; }

        private void Update() { this.nativeAdsService?.DrawNativeAds(this); }
    }
}