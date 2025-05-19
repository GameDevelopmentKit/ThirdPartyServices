namespace Core.AdsServices.Signals
{
    public class NativeOverlayClickedSignal : BaseAdsSignal
    {
        public NativeOverlayClickedSignal(string placement, AdInfo adInfo = null) : base(placement, adInfo)
        {
        }
    }

    public class NativeOverlayFullScreenContentClosedSignal : BaseAdsSignal
    {
        public NativeOverlayFullScreenContentClosedSignal(string placement, AdInfo adInfo = null) : base(placement, adInfo)
        {
        }
    }

    public class NativeOverlayFullScreenContentOpenedSignal : BaseAdsSignal
    {
        public NativeOverlayFullScreenContentOpenedSignal(string placement, AdInfo adInfo = null) : base(placement, adInfo)
        {
        }
    }
}