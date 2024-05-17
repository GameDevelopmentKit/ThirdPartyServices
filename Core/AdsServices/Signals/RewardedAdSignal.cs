namespace Core.AdsServices.Signals
{
    public class RewardedAdLoadedSignal : BaseAdsSignal
    {
        public RewardedAdLoadedSignal(string placement) : base(placement) { }
    }

    public class RewardedAdLoadFailedSignal : BaseAdsSignal
    {
        public string Message;
        public RewardedAdLoadFailedSignal(string placement, string message) : base(placement) { this.Message = message; }
    }

    public class RewardedAdClickedSignal : BaseAdsSignal
    {
        public RewardedAdClickedSignal(string placement) : base(placement) { }
    }

    public class RewardedAdDisplayedSignal : BaseAdsSignal
    {
        public RewardedAdDisplayedSignal(string placement) : base(placement) { }
    }

    public class RewardedAdCompletedSignal : BaseAdsSignal
    {
        public RewardedAdCompletedSignal(string placement) : base(placement) { }
    }

    public class RewardedSkippedSignal : BaseAdsSignal
    {
        public RewardedSkippedSignal(string placement) : base(placement) { }
    }

    public class RewardedAdEligibleSignal : BaseAdsSignal
    {
        public RewardedAdEligibleSignal(string placement) : base(placement) { }
    }

    public class RewardedAdCalledSignal : BaseAdsSignal
    {
        public RewardedAdCalledSignal(string placement) : base(placement) { }
    }

    public class RewardedAdOfferSignal : BaseAdsSignal
    {
        public RewardedAdOfferSignal(string placement) : base(placement) { }
    }

    public class RewardedAdClosedSignal : BaseAdsSignal
    {
        public RewardedAdClosedSignal(string placement) : base(placement) { }
    }

    public class RewardedAdShowFailedSignal : BaseAdsSignal
    {
        public RewardedAdShowFailedSignal(string placement) : base(placement) { }
    }
}