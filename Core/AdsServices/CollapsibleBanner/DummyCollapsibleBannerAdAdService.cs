namespace Core.AdsServices.CollapsibleBanner
{
    using GameFoundation.Scripts.Utilities.LogService;
    using UnityEngine;

    public class DummyCollapsibleBannerAdAdService : ICollapsibleBannerAd
    {
        #region Inject

        private readonly ILogService logService;

        #endregion

        public DummyCollapsibleBannerAdAdService(ILogService logService) { this.logService = logService; }

        public void ShowCollapsibleBannerAd(bool useNewGuid, BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom) { this.logService.Log("Dummy show collapsible banner ad"); }

        public void LoadCollapsibleBannerAd(bool useNewGuid, BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom)
        {
            
        }

        public void HideCollapsibleBannerAd() { this.logService.Log("Dummy hide collapsible banner ad"); }

        public void DestroyCollapsibleBannerAd() { this.logService.Log("Dummy destroy collapsible banner ad"); }

        public bool IsHasCollapsibleBannerAd(BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom) { return Random.Range(0, 2) == 0; }
    }
}