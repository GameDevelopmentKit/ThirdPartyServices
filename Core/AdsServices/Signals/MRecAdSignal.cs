namespace Core.AdsServices.Signals
{
    using Core.AnalyticServices.CommonEvents;

    public class MRecAdLoadedSignal : BaseAdsSignal
    {
        public MRecAdLoadedSignal(string placement, AdsRevenueEvent adsRevenueEvent) : base(placement, adsRevenueEvent) { }
    }

    public class MRecAdLoadFailedSignal : BaseAdsSignal
    {
        public MRecAdLoadFailedSignal(string placement) : base(placement) { }
    }

    public class MRecAdClickedSignal : BaseAdsSignal
    {
        public MRecAdClickedSignal(string placement) : base(placement) { }
    }

    public class MRecAdDisplayedSignal : BaseAdsSignal
    {
        public MRecAdDisplayedSignal(string placement) : base(placement) { }
    }

    public class MRecAdDismissedSignal : BaseAdsSignal
    {
        public MRecAdDismissedSignal(string placement) : base(placement) { }
    }
}