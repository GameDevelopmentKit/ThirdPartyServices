namespace Core.AdsServices.Signals
{
    public class BaseAdsSignal
    {
        public string Placement;
        public AdInfo AdInfo;

        protected BaseAdsSignal(string placement, AdInfo adInfo = null)
        {
            this.Placement = placement;
            this.AdInfo    = adInfo;
        }
    }

    public class AdRequestSignal : BaseAdsSignal
    {
        public AdRequestSignal(string placement, AdInfo adInfo) : base(placement, adInfo)
        {
        }
    }
}