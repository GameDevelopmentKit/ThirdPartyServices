namespace Core.AdsServices.Signals
{
    public class InterstitialAdLoadedSignal : BaseAdsSignal
    {
        public InterstitialAdLoadedSignal(string placement) : base(placement)
        {
        }
    }
    
    public class InterstitialAdLoadFailedSignal : BaseAdsSignal
    {
        public string Message;
        public InterstitialAdLoadFailedSignal(string placement, string message) : base(placement)
        {
            this.Message = message;
        }
    }
    
    public class InterstitialAdClickedSignal : BaseAdsSignal
    {
        public InterstitialAdClickedSignal(string placement) : base(placement)
        {
        }
    }
}