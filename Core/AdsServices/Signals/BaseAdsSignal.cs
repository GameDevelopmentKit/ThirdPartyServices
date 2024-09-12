namespace Core.AdsServices.Signals
{
    using Core.AnalyticServices.CommonEvents;

    public class BaseAdsSignal
    {
        // Ad Revenue data
        public AdsRevenueEvent AdsRevenueEvent;

        public string Placement;

        public BaseAdsSignal(string placement, AdsRevenueEvent adsRevenueEvent = null)
        {
            this.Placement       = placement;
            this.AdsRevenueEvent = adsRevenueEvent;
        }
    }

    public class AdRequestSignal : BaseAdsSignal
    {
        public string AdPlatform;
        public string AdUnitId;
        public string AdSource;
        public string AdFormat;

        public AdRequestSignal(string placement, string adPlatform, string adUnitId, string adSource, string adFormat, AdsRevenueEvent adsRevenueEvent) : base(placement, adsRevenueEvent)
        {
            this.AdPlatform = adPlatform;
            this.AdUnitId   = adUnitId;
            this.AdSource   = adSource;
            this.AdFormat   = adFormat;
        }
    }
}