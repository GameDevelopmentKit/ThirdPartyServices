namespace Core.AdsServices.Native
{
    using System.Collections.Generic;
    using GoogleMobileAds.Api;

#if ADMOB_NATIVE_ADS && !IMMERSIVE_ADS

    public interface INativeAdsService
    {
        void   DrawNativeAds(NativeAdsView nativeAdsView);
        List<NativeAd> GetNativeAds();
        void   RemoveNativeAd(NativeAd nativeAd);
    }
#endif 
}