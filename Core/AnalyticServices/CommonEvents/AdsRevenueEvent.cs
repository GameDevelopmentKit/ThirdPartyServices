namespace Core.AnalyticServices.CommonEvents
{
    using Core.AnalyticServices.Data;

    #region Ads Revenue

    public class AdsRevenueEvent : IEvent
    {
        public string AdsRevenueSourceId;
        public string AdNetwork;
        public string AdUnit;
        public string AdFormat;
        public string NetworkPlacement;
        public string Placement;
        public string Currency;
        public double Revenue;
    }

    public class AdRevenueConstants
    {
        public const string AdjustUrlStrategyChina = "china";
        public const string AdjustUrlStrategyIndia = "india";
        public const string AdjustUrlStrategyCn    = "cn";
        public const string AdjustDataResidencyEU  = "data-residency-eu";
        public const string AdjustDataResidencyTR  = "data-residency-tr";
        public const string AdjustDataResidencyUS  = "data-residency-us";

        public const string ARSourceAppLovinMAX      = "max";
        public const string ARSourceMopub            = "mopub";
        public const string ARSourceAdMob            = "admob";
        public const string ARSourceYandex           = "yandex";
        public const string ARSourceIronSource       = "levelplay";
        public const string ARSourceAdmost           = "admost";
        public const string ARSourceUnity            = "unity";
        public const string ARSourceHeliumChartboost = "helium_chartboost";
        public const string ARSourcePublisher        = "publisher";
        public const string ARSourceImmersiveAds     = "immersive_ads";
        public const string ARSourceGadsmeAds        = "gadsme_ads";
    }

    #endregion
}