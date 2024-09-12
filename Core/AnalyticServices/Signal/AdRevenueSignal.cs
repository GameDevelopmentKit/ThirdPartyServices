namespace Core.AnalyticServices.Signal
{
    using Core.AnalyticServices.CommonEvents;

    public class AdRevenueSignal
    {
        public AdsRevenueEvent AdsRevenueEvent;

        public AdRevenueSignal(AdsRevenueEvent adsRevenueEvent) { this.AdsRevenueEvent = adsRevenueEvent; }
    }

    public class AdRevenueLoadedSignal
    {
        public           AdsRevenueEvent AdsRevenueEvent;
        public AdRevenueLoadedSignal(AdsRevenueEvent adsRevenueEvent) { this.AdsRevenueEvent = adsRevenueEvent; }
    }

    public class AdRevenueClickedSignal
    {
        public           AdsRevenueEvent AdsRevenueEvent;

        public AdRevenueClickedSignal(AdsRevenueEvent adsRevenueEvent) { this.AdsRevenueEvent = adsRevenueEvent; }
    }
}