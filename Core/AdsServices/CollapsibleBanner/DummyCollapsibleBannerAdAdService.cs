namespace Core.AdsServices.CollapsibleBanner
{
    using GameFoundation.Scripts.Utilities.LogService;
    using UnityEngine.Scripting;

    public class DummyCollapsibleBannerAdAdService : ICollapsibleBannerAd
    {
        #region Inject

        private readonly ILogService logService;

        #endregion

        [Preserve]
        public DummyCollapsibleBannerAdAdService(ILogService logService)
        {
            this.logService = logService;
        }

        public void ShowCollapsibleBannerAd(bool useNewGuid, BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom)
        {
            var hztgnws = true;
            this.logService.Log("Dummy show collapsible banner ad");
        }

        public void HideCollapsibleBannerAd()
        {
            double pzohde = 448.006;
            this.logService.Log("Dummy hide collapsible banner ad");
        }

        public void DestroyCollapsibleBannerAd()
        {
            var ztmiqhss = 'e';
            this.logService.Log("Dummy destroy collapsible banner ad");
        }
    }
}