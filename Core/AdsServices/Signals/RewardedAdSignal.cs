namespace Core.AdsServices.Signals
{
    using Core.AnalyticServices.CommonEvents;

    public class RewardedAdLoadedSignal : BaseAdsSignal
    {
        public long LoadingTime;
        public RewardedAdLoadedSignal(string placement, long loadingTime, AdInfo adInfo) : base(placement, adInfo)
        {
            this.LoadingTime = loadingTime;
        }
    }

    public class RewardedAdLoadFailedSignal : BaseAdsSignal
    {
        public float LoadingTime;
        public string Message;
        public RewardedAdLoadFailedSignal(string placement, string message, float loadingTime) : base(placement)
        {
            this.Message     = message;
            this.LoadingTime = loadingTime;
        }
    }

    public class RewardedAdClickedSignal : BaseAdsSignal
    {
        public RewardedAdClickedSignal(string placement, AdInfo adInfo) : base(placement, adInfo) { }
    }

    public class RewardedAdDisplayedSignal : BaseAdsSignal
    {
        public RewardedAdDisplayedSignal(string placement, AdInfo adInfo) : base(placement, adInfo) { }
    }

    public class RewardedAdCompletedSignal : BaseAdsSignal
    {
        public RewardedAdCompletedSignal(string placement, AdInfo adInfo) : base(placement, adInfo)
        {
        }
    }

    public class RewardedSkippedSignal : BaseAdsSignal
    {
        public RewardedSkippedSignal(string placement, AdInfo adInfo) : base(placement, adInfo) { }
    }

    public class RewardedAdEligibleSignal : BaseAdsSignal
    {
        public RewardedAdEligibleSignal(string placement) : base(placement) { }
    }

    public class RewardedAdCalledSignal : BaseAdsSignal
    {
        public RewardedAdCalledSignal(string placement, AdInfo adInfo) : base(placement, adInfo) { }
    }

    public class RewardedAdOfferSignal : BaseAdsSignal
    {
        public RewardedAdOfferSignal(string placement) : base(placement) { }
    }

    public class RewardedAdClosedSignal : BaseAdsSignal
    {
        public RewardedAdClosedSignal(string placement, AdInfo adInfo) : base(placement, adInfo) { }
    }

    public class RewardedAdDisplayFailedSignal : BaseAdsSignal
    {
        public string Message { get; private set; }
        public RewardedAdDisplayFailedSignal(string placement, string message, AdInfo adInfo) : base(placement, adInfo) { this.Message = message; }
    }
}