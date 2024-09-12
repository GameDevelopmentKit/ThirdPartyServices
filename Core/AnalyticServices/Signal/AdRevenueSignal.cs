namespace Core.AnalyticServices.Signal
{
    using Core.AnalyticServices.CommonEvents;

    public class AdRevenueSignal
    {
        public AdsRevenueEvent AdsRevenueEvent;

        public AdRevenueSignal(AdsRevenueEvent adsRevenueEvent) { this.AdsRevenueEvent = adsRevenueEvent; }
    }

    public class AdRevenueLoadedSignal : AdRevenueSignal
    {
        public AdRevenueLoadedSignal(AdsRevenueEvent adsRevenueEvent) : base(adsRevenueEvent)
        {
        }
    }

    public class AdRevenueClickedSignal : AdRevenueSignal
    {
        public AdRevenueClickedSignal(AdsRevenueEvent adsRevenueEvent) : base(adsRevenueEvent)
        {
        }
    }
}