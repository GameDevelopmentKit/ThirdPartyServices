namespace Core.AdsServices.Native
{
    using System.Collections.Generic;

    public interface INativeAdsService
    {
        void                          DrawNativeAds(NativeAdsView nativeAdsView);
        List<NativeAdInstanceWrapper> GetNativeAds(string adsId = "");
        void                          RemoveNativeAd(NativeAdInstanceWrapper nativeAd);
    }
}