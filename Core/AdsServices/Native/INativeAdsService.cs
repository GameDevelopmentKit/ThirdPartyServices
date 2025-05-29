namespace Core.AdsServices.Native
{
    public interface INativeAdsService
    {
    }

    #if ADMOB_NATIVE_ADS
    public interface IAdMobNativeAdsService : INativeAdsService
    {
        void DrawNativeAds(NativeAdsView nativeAdsView);
    }
    #endif
}