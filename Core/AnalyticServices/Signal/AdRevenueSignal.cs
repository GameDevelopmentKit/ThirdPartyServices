namespace Core.AnalyticServices.Signal
{
    using Core.AnalyticServices.CommonEvents;

    public class AdRevenueSignal
    {
        public AdsRevenueEvent AdsRevenueEvent;

        public AdRevenueSignal(AdsRevenueEvent adsRevenueEvent) { this.AdsRevenueEvent = adsRevenueEvent; }
    }
}