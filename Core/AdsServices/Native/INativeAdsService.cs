namespace Core.AdsServices.Native
{
    public interface INativeAdsService
    {
        void DrawNativeAds(NativeAdsView admobNativeAdsView);

        void RemoveAds();

        bool IsRemoveAds();
    }
}