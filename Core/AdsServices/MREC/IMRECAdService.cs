namespace Core.AdsServices
{
    public interface IMRECAdService
    {
        void ShowMREC(string                     placement, AdScreenPosition position, AdScreenPosition offset);
        bool IsMRECReady(string                  placement, AdScreenPosition position);
        void ShowMREC(AdViewPosition             adViewPosition);
        void HideMREC(AdViewPosition             adViewPosition);
        void HideMREC(string                     placement, AdScreenPosition position);
        void StopMRECAutoRefresh(AdViewPosition  adViewPosition);
        void StartMRECAutoRefresh(AdViewPosition adViewPosition);
        void LoadMREC(AdViewPosition             adViewPosition);
        bool IsMRECReady(AdViewPosition          adViewPosition);
        void HideAllMREC();
    }
}