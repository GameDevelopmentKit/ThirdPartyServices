namespace Core.AdsServices
{
    using System;
    using GameFoundation.Scripts.Utilities.LogService;

    public class DummyMRECAdService : IMRECAdService
    {
        #region inject

        private readonly ILogService logService;

        #endregion

        public DummyMRECAdService(ILogService logService) { this.logService = logService; }
        
        public void ShowMREC(AdViewPosition adViewPosition)
        {
            this.logService.Log($"Show MREC - {adViewPosition}");
        }
        public void HideMREC(AdViewPosition adViewPosition)
        {
            this.logService.Log($"Hide MREC - {adViewPosition}");

        }
        public void StopMRECAutoRefresh(AdViewPosition adViewPosition)
        {
            this.logService.Log($"Stop auto refresh MREC - {adViewPosition}");

        }
        public void StartMRECAutoRefresh(AdViewPosition adViewPosition)
        {
            this.logService.Log($"Start auto refresh MREC - {adViewPosition}");
        }
        public void LoadMREC(AdViewPosition adViewPosition)
        {
            this.logService.Log($"Load MREC - {adViewPosition}");
        }
        
        public event Action<string, AdInfo>    OnAdLoadedEvent;
        public event Action<string, ErrorInfo> OnAdLoadFailedEvent;
        public event Action<string, AdInfo>    OnAdClickedEvent;
        public event Action<string, AdInfo>    OnAdRevenuePaidEvent;
        public event Action<string, AdInfo>    OnAdExpandedEvent;
        public event Action<string, AdInfo>    OnAdCollapsedEvent;
    }
}