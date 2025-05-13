namespace Core.AdsServices.Native
{
    public interface INativeAdsService
    {
        void RemoveAds();

        bool IsRemoveAds();
    }

    #if ADMOB_NATIVE_ADS
    public interface IAdMobNativeAdsService : INativeAdsService
    {
        void DrawNativeAds(NativeAdsView nativeAdsView);
    }
    #endif
}