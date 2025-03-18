namespace Core.AdsServices
{
    public interface IMRECAdService
    {
        void ShowMREC(string    placement, AdScreenPosition position, AdScreenPosition offset);
        bool IsMRECReady(string placement, AdScreenPosition position, AdScreenPosition offset);
        void HideMREC(string    placement);
        void DestroyMREC(string placement);
        void HideAllMREC();

        void ShowMREC(string placement);
    }
}