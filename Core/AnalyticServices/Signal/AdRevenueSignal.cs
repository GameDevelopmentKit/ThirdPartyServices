namespace Core.AnalyticServices.Signal
{
    using Core.AnalyticServices.CommonEvents;

    public class AdRevenueSignal
    {
        public AdsRevenueEvent AdsRevenueEvent;

        public AdRevenueSignal(AdsRevenueEvent adsRevenueEvent) { this.AdsRevenueEvent = adsRevenueEvent; }
    }

    public class AdRevenueRequestSignal
    {
        public AdsRevenueEvent AdsRevenueRequestEvent;
        public AdRevenueRequestSignal(AdsRevenueEvent adsRevenueRequestEvent) { this.AdsRevenueRequestEvent = adsRevenueRequestEvent; }
    }
    public class AdRevenueLoadedSignal
    {
        public AdsRevenueEvent AdsRevenueLoadEvent;

        public AdRevenueLoadedSignal(AdsRevenueEvent adsRevenueLoadEvent) { this.AdsRevenueLoadEvent = adsRevenueLoadEvent; }
    }
    
    public class AdRevenueClickedSignal
    {
        public AdsRevenueEvent AdsRevenueClickedEvent;

        public AdRevenueClickedSignal(AdsRevenueEvent adsRevenueClickedEvent) { this.AdsRevenueClickedEvent = adsRevenueClickedEvent; }
    }
}