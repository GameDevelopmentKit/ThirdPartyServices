namespace Core.AdsServices.Signals
{
    public class AppOpenFullScreenContentOpenedSignal : BaseAdsSignal
    {
        public AppOpenFullScreenContentOpenedSignal(string placement, AdInfo adInfo) : base(placement, adInfo)
        {
        }
    }

    public class AppOpenFullScreenContentFailedSignal : BaseAdsSignal
    {
        public string Message { get; private set; }

        public AppOpenFullScreenContentFailedSignal(string placement, string message) : base(placement)
        {
            this.Message = message;
        }
    }

    public class AppOpenFullScreenContentClosedSignal : BaseAdsSignal
    {
        public AppOpenFullScreenContentClosedSignal(string placement, AdInfo adInfo) : base(placement, adInfo)
        {
        }
    }

    public class AppOpenLoadedSignal : BaseAdsSignal
    {
        public AppOpenLoadedSignal(string placement, AdInfo adInfo) : base(placement, adInfo)
        {
        }
    }

    public class AppOpenLoadFailedSignal : BaseAdsSignal
    {
        public AppOpenLoadFailedSignal(string placement) : base(placement)
        {
        }
    }

    public class AppOpenEligibleSignal : BaseAdsSignal
    {
        public AppOpenEligibleSignal(string placement) : base(placement)
        {
        }
    }

    public class AppOpenCalledSignal : BaseAdsSignal
    {
        public AppOpenCalledSignal(string placement) : base(placement)
        {
        }
    }

    public class AppOpenClickedSignal : BaseAdsSignal
    {
        public AppOpenClickedSignal(string placement, AdInfo adInfo) : base(placement, adInfo)
        {
        }
    }
}