namespace Core.AdsServices.Native
{
    public interface INativeAdsService
    {
        void DrawNativeAds(AdmobNativeAdsView admobNativeAdsView);

        void RemoveAds();

        bool IsRemoveAds();
    }
}