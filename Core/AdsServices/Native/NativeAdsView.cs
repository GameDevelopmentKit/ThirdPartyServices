namespace Core.AdsServices.Native
{
    using UnityEngine;

    public class NativeAdsView : MonoBehaviour
    {
        public  GameObject        originalIcon;
        public  GameObject        icon;
        private INativeAdsService nativeAdsService;

        public void Init(INativeAdsService nativeAdsService) { this.nativeAdsService = nativeAdsService; }

        private void Update()
        {
            this.nativeAdsService?.DrawNativeAds(this);
        }
    }
}