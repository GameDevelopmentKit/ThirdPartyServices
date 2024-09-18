namespace Core.AdsServices.Signals
{
    public class BannerAdPresentedSignal : BaseAdsSignal
    {
        public BannerAdPresentedSignal(string placement, AdInfo adInfo) : base(placement, adInfo) { }
    }

    public class BannerAdDismissedSignal : BaseAdsSignal
    {
        public BannerAdDismissedSignal(string placement, AdInfo adInfo) : base(placement, adInfo) { }
    }

    public class BannerAdLoadedSignal : BaseAdsSignal
    {
        public BannerAdLoadedSignal(string placement, AdInfo adInfo) : base(placement, adInfo) { }
    }

    public class BannerAdLoadFailedSignal : BaseAdsSignal
    {
        public string Message;
        public BannerAdLoadFailedSignal(string placement, string message) : base(placement) { this.Message = message; }
    }

    public class BannerAdClickedSignal : BaseAdsSignal
    {
        public BannerAdClickedSignal(string placement, AdInfo adInfo) : base(placement, adInfo) { }
    }
}