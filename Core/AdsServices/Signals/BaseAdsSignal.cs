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

        public AdRequestSignal(string placement, AdsRevenueEvent adsRevenueEvent) : base(placement, adsRevenueEvent)
        {
        }
    }
}