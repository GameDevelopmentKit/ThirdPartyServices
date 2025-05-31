#if !ADMOB
namespace ServiceImplementation.AdsServices.NativeOverlay
{
    using Core.AdsServices;
    using GameFoundation.Scripts.Utilities.LogService;
    using ServiceImplementation.Configs;
    using ServiceImplementation.Configs.Ads;
    using UnityEngine.Scripting;

    public class DummyNativeOverlayService : INativeOverlayService
    {
        private readonly ThirdPartiesConfig thirdPartiesConfig;
        private readonly ILogService        logService;

        [Preserve]
        public DummyNativeOverlayService(ThirdPartiesConfig thirdPartiesConfig, ILogService logService)
        {
            this.thirdPartiesConfig = thirdPartiesConfig;
            this.logService         = logService;
        }

        public void LoadAd(string placement)
        {
            this.logService.Log($"oneLog: DummyNativeOverlayService LoadAd placement {placement}");
        }

        public bool IsAdReady(string placement)
        {
            var isAdReady = this.thirdPartiesConfig.AdSettings.AdMob.NativeOverlayAdIds.ContainsKey(AdPlacement.PlacementWithName(placement));
            this.logService.Log($"oneLog: DummyNativeOverlayService IsAdReady {isAdReady} - placement {placement}");
            return isAdReady;
        }

        public void ShowAd(string placement, AdViewPosition adViewPosition)
        {
            this.logService.Log($"oneLog: DummyNativeOverlayService ShowAd placement {placement}, position {adViewPosition}");
        }

        public void HideAd(string placement)
        {
            this.logService.Log($"oneLog: DummyNativeOverlayService HideAd placement {placement}");
        }

        public void DestroyAd(string placement)
        {
            this.logService.Log($"oneLog: DummyNativeOverlayService void DestroyAd placement {placement}");
        }

        public void DestroyAll()
        {
            this.logService.Log($"oneLog: DummyNativeOverlayService DestroyAll");
        }
    }
}
#endif