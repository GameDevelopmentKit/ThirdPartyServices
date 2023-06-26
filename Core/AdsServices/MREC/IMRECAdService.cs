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
        bool IsMRECReady(AdViewPosition              adViewPosition);
    }
}