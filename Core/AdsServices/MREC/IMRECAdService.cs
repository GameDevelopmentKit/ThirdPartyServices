namespace Core.AdsServices
{
    public interface IMRECAdService
    {
        void ShowMREC(string                     placement, AdScreenPosition position, AdScreenPosition offset);
        bool IsMRECReady(string                  placement, AdScreenPosition position);
        void HideMREC(string                     placement, AdScreenPosition position);
        void HideAllMREC();
    }
}