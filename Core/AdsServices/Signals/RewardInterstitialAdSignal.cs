namespace Core.AdsServices.Signals
{
    public class RewardedInterstitialAdCompletedSignal : BaseAdsSignal
    {
        public RewardedInterstitialAdCompletedSignal(string placement) : base(placement) { }
    }

    public class RewardInterstitialAdSkippedSignal : BaseAdsSignal
    {
        public RewardInterstitialAdSkippedSignal(string placement) : base(placement) { }
    }

    public class RewardInterstitialAdCalledSignal : BaseAdsSignal
    {
        public RewardInterstitialAdCalledSignal(string placement) : base(placement) { }
    }

    public class RewardInterstitialAdClosedSignal : BaseAdsSignal
    {
        public RewardInterstitialAdClosedSignal(string placement) : base(placement) { }
    }
}