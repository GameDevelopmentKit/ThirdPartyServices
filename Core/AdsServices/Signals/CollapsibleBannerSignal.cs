namespace Core.AdsServices.Signals
{
    public class CollapsibleBannerAdPresentedSignal : BaseAdsSignal
    {
        public CollapsibleBannerAdPresentedSignal(string placement) : base(placement)
        {
        }
    }
    
    public class CollapsibleBannerAdDismissedSignal : BaseAdsSignal
    {
        public CollapsibleBannerAdDismissedSignal(string placement) : base(placement)
        {
        }
    }
    
    public class CollapsibleBannerAdLoadedSignal : BaseAdsSignal
    {
        public CollapsibleBannerAdLoadedSignal(string placement, AdInfo adInfo) : base(placement, adInfo)
        {
        }
    }
    
    public class CollapsibleBannerAdLoadFailedSignal : BaseAdsSignal
    {
        public string Message;
        public CollapsibleBannerAdLoadFailedSignal(string placement, string message) : base(placement)
        {
            this.Message = message;
        }
    }
    
    public class CollapsibleBannerAdClickedSignal : BaseAdsSignal
    {
        public CollapsibleBannerAdClickedSignal(string placement, AdInfo adInfo) : base(placement, adInfo)
        {
        }
    }
}