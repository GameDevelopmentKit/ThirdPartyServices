namespace Core.AdsServices.Signals
{
    using Core.AnalyticServices.CommonEvents;
    using Core.AnalyticServices.Signal;

    public class InterstitialAdCalledSignal : BaseAdsSignal
    {
        public InterstitialAdCalledSignal(string placement) : base(placement) { }
    }

    public class InterstitialAdLoadedSignal : BaseAdsSignal
    {
        public long            LoadingMilis;
        public InterstitialAdLoadedSignal(string placement, long loadingMilis, AdsRevenueEvent adsRevenueEvent) : base(placement, adsRevenueEvent) { this.LoadingMilis = loadingMilis; }
    }

    public class InterstitialAdEligibleSignal : BaseAdsSignal
    {
        public InterstitialAdEligibleSignal(string placement) : base(placement) { }
    }

    public class InterstitialAdLoadFailedSignal : BaseAdsSignal
    {
        public string Message;
        public long   LoadingMilis;

        public InterstitialAdLoadFailedSignal(string placement, string message, long loadingMilis) : base(placement)
        {
            this.Message      = message;
            this.LoadingMilis = loadingMilis;
        }
    }

    public class InterstitialAdClickedSignal : BaseAdsSignal
    {
        public InterstitialAdClickedSignal(string placement, AdsRevenueEvent adsRevenueEvent) : base(placement, adsRevenueEvent) { }
    }

    public class InterstitialAdDisplayedSignal : BaseAdsSignal
    {
        public InterstitialAdDisplayedSignal(string placement, AdsRevenueEvent adsRevenueEvent) : base(placement, adsRevenueEvent) { }
    }

    public class InterstitialAdDisplayedFailedSignal : BaseAdsSignal
    {
        public InterstitialAdDisplayedFailedSignal(string placement) : base(placement) { }
    }

    public class InterstitialAdClosedSignal : BaseAdsSignal
    {
        public InterstitialAdClosedSignal(string placement, AdsRevenueEvent adsRevenueEvent) : base(placement, adsRevenueEvent) { }
    }
}