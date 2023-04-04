namespace Core.AdsServices.Signals
{
    public class BannerAdPresentedSignal : BaseAdsSignal
    {
        public BannerAdPresentedSignal(string placement) : base(placement)
        {
        }
    }
    
    public class BannerAdDismissedSignal : BaseAdsSignal
    {
        public BannerAdDismissedSignal(string placement) : base(placement)
        {
        }
    }
    
    public class BannerAdLoadedSignal : BaseAdsSignal
    {
        public BannerAdLoadedSignal(string placement) : base(placement)
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
        public BannerAdClickedSignal(string placement) : base(placement)
        {
        }
    }
}