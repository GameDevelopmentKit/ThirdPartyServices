namespace Core.AdsServices.CollapsibleBanner
{
    public interface ICollapsibleBannerAd
    {
        void ShowCollapsibleBannerAd(bool useNewGuid, BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom);
        void LoadCollapsibleBannerAd(bool useNewGuid, BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom);

        void HideCollapsibleBannerAd();
        void DestroyCollapsibleBannerAd();
        bool        IsHasCollapsibleBannerAd(BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom);
    }
}