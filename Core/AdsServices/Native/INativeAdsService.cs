namespace Core.AdsServices.Native
{
    public interface INativeAdsService
    {
        void ShowNativeAds(NativeAdsView nativeAdsView);
        void RenderNativeAd(string adsId);
        void HideNativeAds(string adsId);
        void DestroyNativeAds(string adsId);
    }
}