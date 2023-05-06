namespace Core.AdsServices.Signals
{
    using Core.AdsServices.Native;

    public class DrawNativeAdRequestSignal
    {
        public NativeAdsView NativeAdsView;

        public DrawNativeAdRequestSignal(NativeAdsView nativeAdsView)
        {
            this.NativeAdsView = nativeAdsView;
        }
    }
}