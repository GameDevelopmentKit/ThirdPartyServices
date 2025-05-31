namespace ServiceImplementation.AdsServices.NativeOverlay
{
    using Core.AdsServices;

    public interface INativeOverlayService
    {
        void LoadAd(string placement);

        bool IsAdReady(string placement);

        void ShowAd(string placement, AdViewPosition adViewPosition);

        void HideAd(string placement);

        void DestroyAd(string placement);

        void DestroyAll();
    }
}