namespace Core.AdsServices.Signals
{
    public class InterstitialAdCalledSignal : BaseAdsSignal
    {
        public InterstitialAdCalledSignal(string placement) : base(placement) { }
    }

    public class InterstitialAdLoadedSignal : BaseAdsSignal
    {
        public long LoadingMilis;
        public InterstitialAdLoadedSignal(string placement, long loadingMilis) : base(placement) { this.LoadingMilis = loadingMilis; }
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
        public InterstitialAdClickedSignal(string placement) : base(placement) { }
    }

    public class InterstitialAdDisplayedSignal : BaseAdsSignal
    {
        public InterstitialAdDisplayedSignal(string placement) : base(placement) { }
    }

    public class InterstitialAdDisplayedFailedSignal : BaseAdsSignal
    {
        public InterstitialAdDisplayedFailedSignal(string placement) : base(placement) { }
    }

    public class InterstitialAdClosedSignal : BaseAdsSignal
    {
        public InterstitialAdClosedSignal(string placement) : base(placement) { }
    }
}