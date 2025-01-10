namespace Core.AdsServices.Native
{
    public interface INativeAdsService
    {
        void   DrawNativeAds(NativeAdsView nativeAdsView);
        object GetNativeAd();
        void   RemoveNativeAd(object nativeAd);
    }
}