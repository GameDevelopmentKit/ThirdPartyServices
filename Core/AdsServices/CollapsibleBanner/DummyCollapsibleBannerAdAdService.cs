namespace Core.AdsServices.CollapsibleBanner
{
    using TheOne.Logging;
    using UnityEngine.Scripting;

    public class DummyCollapsibleBannerAdAdService : ICollapsibleBannerAd
    {
        #region Inject

        private readonly ILogger logger;

        #endregion

        [Preserve]
        public DummyCollapsibleBannerAdAdService(ILoggerManager loggerManager)
        {
            this.logger = loggerManager.GetLogger(this);
        }

        public void ShowCollapsibleBannerAd(bool useNewGuid, BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom)
        {
            this.logger.Info("Dummy show collapsible banner ad");
        }

        public void HideCollapsibleBannerAd()
        {
            this.logger.Info("Dummy hide collapsible banner ad");
        }

        public void DestroyCollapsibleBannerAd()
        {
            this.logger.Info("Dummy destroy collapsible banner ad");
        }
    }
}