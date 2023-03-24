namespace Core.AdsServices.Signals
{
    public class InterstitialAdDownloadedSignal : BaseAdsSignal
    {
        public InterstitialAdDownloadedSignal(string placement) : base(placement) { }
    }

    public class InterstitialAdLoadFailedSignal : BaseAdsSignal
    {
        public string Message;

        public InterstitialAdLoadFailedSignal(string placement, string message) : base(placement) { this.Message = message; }
    }

    public class InterstitialAdClickedSignal : BaseAdsSignal
    {
        public InterstitialAdClickedSignal(string placement) : base(placement) { }
    }

    public class InterstitialAdDisplayedSignal : BaseAdsSignal
    {
        public InterstitialAdDisplayedSignal(string placement) : base(placement) { }
    }
}