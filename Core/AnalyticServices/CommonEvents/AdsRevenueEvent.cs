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
    
    public class AdRevenueConstants
    {
        public const string ARSourceAppLovinMAX      = "applovin_max_sdk";
        public const string ARSourceMopub            = "mopub";
        public const string ARSourceAdMob            = "admob_sdk";
        public const string ARSourceIronSource       = "ironARSourcesdk";
        public const string ARSourceAdmost           = "admost_sdk";
        public const string ARSourceUnity            = "unity_sdk";
        public const string ARSourceHeliumChartboost = "helium_chartboost_sdk";
        public const string ARSourcePublisher        = "publisher_sdk";
    }

    #endregion
}