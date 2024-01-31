namespace Core.AdsServices.CollapsibleBanner
{
    public interface ICollapsibleBannerAd
    {
        public void ShowCollapsibleBannerAd(bool useNewGuid, BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom);

        public void HideCollapsibleBannerAd();
        public void DestroyCollapsibleBannerAd();
    }
}