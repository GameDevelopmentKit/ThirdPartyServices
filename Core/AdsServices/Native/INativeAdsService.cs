namespace Core.AdsServices.Native
{
    public interface INativeAdsService
    {
    }

    #if ADMOB_NATIVE_ADS
    public interface IAdMobNativeAdsService : INativeAdsService
    {
        bool IsNativeAdsReady(string       placement);
        void CreateNativeAds(NativeAdsView nativeAdsView);
        void ReleaseNativeAds(string       placement);
    }
    #endif
}