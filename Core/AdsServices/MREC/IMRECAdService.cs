namespace Core.AdsServices
{
    using System;

    public interface IMRECAdService
    {
        void ShowMREC(AdViewPosition             adViewPosition);
        void HideMREC(AdViewPosition             adViewPosition);
        void StopMRECAutoRefresh(AdViewPosition  adViewPosition);
        void StartMRECAutoRefresh(AdViewPosition adViewPosition);
        void LoadMREC(AdViewPosition             adViewPosition);
        bool IsReady(AdViewPosition              adViewPosition);
        
        event Action<string, AdInfo>    OnAdLoadedEvent;
        event Action<string, ErrorInfo> OnAdLoadFailedEvent;
        event Action<string, AdInfo>    OnAdClickedEvent;
        event Action<string, AdInfo>    OnAdRevenuePaidEvent;
        event Action<string, AdInfo>    OnAdExpandedEvent;
        event Action<string, AdInfo>    OnAdCollapsedEvent;
    }
}