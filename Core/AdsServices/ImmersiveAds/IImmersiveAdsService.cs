namespace Core.AdsServices.ImmersiveAds
{

    public interface IImmersiveAdsService
    {
        void InitNativeAdHolder(ImmersiveAdsView immersiveAdsView, string placement, bool worldSpace = false);
    }
}
