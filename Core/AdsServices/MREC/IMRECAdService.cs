namespace Core.AdsServices
{
    public interface IMRECAdService
    {
        void ShowMREC(string                     placement, AdScreenPosition position);
        bool IsMRECReady(string                  placement, AdScreenPosition position);
        void ShowMREC(AdViewPosition             adViewPosition);
        void HideMREC(AdViewPosition             adViewPosition);
        void StopMRECAutoRefresh(AdViewPosition  adViewPosition);
        void StartMRECAutoRefresh(AdViewPosition adViewPosition);
        void LoadMREC(AdViewPosition             adViewPosition);
        bool IsMRECReady(AdViewPosition          adViewPosition);
        void HideAllMREC();
    }
}