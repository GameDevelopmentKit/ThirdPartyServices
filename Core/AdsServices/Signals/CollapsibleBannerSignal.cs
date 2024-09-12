namespace Core.AdsServices.Signals
{
    using Core.AnalyticServices.CommonEvents;

    public class CollapsibleBannerAdPresentedSignal : BaseAdsSignal
    {
        public CollapsibleBannerAdPresentedSignal(string placement, AdsRevenueEvent adsRevenueEvent) : base(placement, adsRevenueEvent)
        {
        }
    }
    
    public class CollapsibleBannerAdDismissedSignal : BaseAdsSignal
    {
        public CollapsibleBannerAdDismissedSignal(string placement, AdsRevenueEvent adsRevenueEvent) : base(placement, adsRevenueEvent)
        {
        }
    }
    
    public class CollapsibleBannerAdLoadedSignal : BaseAdsSignal
    {
        public CollapsibleBannerAdLoadedSignal(string placement, AdsRevenueEvent adsRevenueEvent) : base(placement, adsRevenueEvent)
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
        public CollapsibleBannerAdClickedSignal(string placement, AdsRevenueEvent adsRevenueEvent) : base(placement, adsRevenueEvent)
        {
        }
    }
}