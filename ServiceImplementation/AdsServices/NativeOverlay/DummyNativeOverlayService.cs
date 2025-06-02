#if !ADMOB
namespace ServiceImplementation.AdsServices.NativeOverlay
{
    using Core.AdsServices;
    using ServiceImplementation.Configs;
    using ServiceImplementation.Configs.Ads;
    using TheOne.Logging;
    using UnityEngine.Scripting;

    public class DummyNativeOverlayService : INativeOverlayService
    {
        private readonly ThirdPartiesConfig thirdPartiesConfig;
        private readonly ILogger            logger;

        [Preserve]
        public DummyNativeOverlayService(ThirdPartiesConfig thirdPartiesConfig, ILoggerManager loggerManager)
        {
            this.thirdPartiesConfig = thirdPartiesConfig;
            this.logger             = loggerManager.GetLogger(this);
        }

        public void LoadAd(string placement)
        {
            this.logger.Info($"LoadAd placement {placement}");
        }

        public bool IsAdReady(string placement)
        {
            var isAdReady = this.thirdPartiesConfig.AdSettings.AdMob.NativeOverlayAdIds.ContainsKey(AdPlacement.PlacementWithName(placement));
            this.logger.Info($"IsAdReady {isAdReady} - placement {placement}");
            return isAdReady;
        }

        public void ShowAd(string placement, AdViewPosition adViewPosition)
        {
            this.logger.Info($"ShowAd placement {placement}, position {adViewPosition}");
        }

        public void HideAd(string placement)
        {
            this.logger.Info($"HideAd placement {placement}");
        }

        public void DestroyAd(string placement)
        {
            this.logger.Info($"void DestroyAd placement {placement}");
        }

        public void DestroyAll()
        {
            this.logger.Info($"DestroyAll");
        }
    }
}
#endif