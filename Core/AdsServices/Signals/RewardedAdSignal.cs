namespace Core.AdsServices.Signals
{
    public class RewardedAdLoadedSignal : BaseAdsSignal
    {
        public RewardedAdLoadedSignal(string placement) : base(placement)
        {
        }
    }
    
    public class RewardedAdLoadFailedSignal : BaseAdsSignal
    {
        public string Message;
        public RewardedAdLoadFailedSignal(string placement, string message) : base(placement)
        {
            this.Message = message;
        }
    }
    
    public class RewardedAdLoadClickedSignal : BaseAdsSignal
    {
        public RewardedAdLoadClickedSignal(string placement) : base(placement)
        {
        }
    }

    public class RewardedAdDisplayedSignal : BaseAdsSignal
    {
        public RewardedAdDisplayedSignal(string placement) : base(placement)
        {
        }
    }
}