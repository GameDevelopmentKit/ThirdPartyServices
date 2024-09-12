namespace Core.AnalyticServices.Signal
{
    using Core.AnalyticServices.CommonEvents;

    public class AdRevenueSignal
    {
        public string          NetworkPlacement;
        public AdsRevenueEvent AdsRevenueEvent;

        public AdRevenueSignal(AdsRevenueEvent adsRevenueEvent, string networkPlacement = null)
        {
            this.AdsRevenueEvent = adsRevenueEvent;
            this.NetworkPlacement       = networkPlacement;
        }
    }

    public class AdRevenueLoadedSignal
    {
        public string NetworkPlacement;
        public           AdsRevenueEvent AdsRevenueEvent;
        public AdRevenueLoadedSignal(AdsRevenueEvent adsRevenueEvent, string networkPlacement = null)
        {
            this.AdsRevenueEvent = adsRevenueEvent;
            this.NetworkPlacement       = networkPlacement;
        }
    }

    public class AdRevenueClickedSignal
    {
        public string          NetworkPlacement;
        public AdsRevenueEvent AdsRevenueEvent;

        public AdRevenueClickedSignal(AdsRevenueEvent adsRevenueEvent, string networkPlacement = null)
        {
            this.AdsRevenueEvent = adsRevenueEvent;
            this.NetworkPlacement       = networkPlacement;
        }
    }
}