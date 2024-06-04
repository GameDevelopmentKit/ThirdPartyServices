#if IMMERSIVE_ADS
namespace Core.AdsServices.ImmersiveAds
{
    using PubScale.SdkOne.NativeAds;

    public interface IImmersiveAdsService
    {
        void InitNativeAdHolder(NativeAdHolder nativeAdHolder, string placement, bool worldSpace = false);
    }
}
#endif