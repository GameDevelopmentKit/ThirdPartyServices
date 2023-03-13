namespace Core.AnalyticServices.CommonEvents
{
    using Core.AnalyticServices.Data;

    #region Ads Revenue

    public class AdsRevenueEvent : IEvent
    {
        public string AdsRevenueSourceId;
        public string AdNetwork;
        public string AdUnit;
        public string Placement;
        public string Currency;
        public double Revenue;
    }

    #endregion
}