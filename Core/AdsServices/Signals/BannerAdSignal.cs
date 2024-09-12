namespace Core.AdsServices.Signals
{
    using Core.AnalyticServices.CommonEvents;

    public class BannerAdPresentedSignal : BaseAdsSignal
    {
        public BannerAdPresentedSignal(string placement, AdsRevenueEvent adsRevenueEvent) : base(placement, adsRevenueEvent)
        {
        }
    }
    
    public class BannerAdDismissedSignal : BaseAdsSignal
    {
        public BannerAdDismissedSignal(string placement, AdsRevenueEvent adsRevenueEvent) : base(placement, adsRevenueEvent)
        {
        }
    }
    
    public class BannerAdLoadedSignal : BaseAdsSignal
    {
        public BannerAdLoadedSignal(string placement, AdsRevenueEvent adsRevenueEvent) : base(placement, adsRevenueEvent)
        {
        }
    }
    
    public class BannerAdLoadFailedSignal : BaseAdsSignal
    {
        public string Message;
        public BannerAdLoadFailedSignal(string placement, string message) : base(placement)
        {
            this.Message = message;
        }
    }
    
    public class BannerAdClickedSignal : BaseAdsSignal
    {
        private readonly AdsRevenueEvent adsRevenueEvent;

        public BannerAdClickedSignal(string placement, AdsRevenueEvent adsRevenueEvent) : base(placement) { this.adsRevenueEvent = adsRevenueEvent; }
    }
}