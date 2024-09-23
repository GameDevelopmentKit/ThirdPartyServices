namespace Core.AdsServices.Signals
{
    public class MRecAdLoadedSignal : BaseAdsSignal
    {
        public MRecAdLoadedSignal(string placement, AdInfo adInfo) : base(placement, adInfo) { }
    }

    public class MRecAdLoadFailedSignal : BaseAdsSignal
    {
        public MRecAdLoadFailedSignal(string placement) : base(placement) { }
    }

    public class MRecAdClickedSignal : BaseAdsSignal
    {
        public MRecAdClickedSignal(string placement, AdInfo adInfo) : base(placement, adInfo) { }
    }

    public class MRecAdDisplayedSignal : BaseAdsSignal
    {
        public MRecAdDisplayedSignal(string placement, AdInfo adInfo) : base(placement, adInfo) { }
    }

    public class MRecAdDismissedSignal : BaseAdsSignal
    {
        public MRecAdDismissedSignal(string placement) : base(placement) { }
    }
}