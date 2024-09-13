namespace ServiceImplementation.AdsServices.AdRevenueTracker
{
    using Core.AnalyticServices.CommonEvents;

    public class AdRevenueEventHelper
    {
#if APPLOVIN
        public static AdsRevenueEvent CreateAdsRevenueEvent(MaxSdkBase.AdInfo adInfo)
        {
            if (adInfo == null) return null;
            return new()
            {
                AdsRevenueSourceId = AdRevenueConstants.ARSourceAppLovinMAX,
                AdUnit             = adInfo.AdUnitIdentifier,
                Revenue            = adInfo.Revenue,
                Currency           = "USD",
                NetworkPlacement   = adInfo.NetworkPlacement,
                Placement          = adInfo.Placement,
                AdNetwork          = adInfo.NetworkName,
                AdFormat           = adInfo.AdFormat,
            };
        }
#endif
        
        public static AdsRevenueEvent CreateAdsRevenueEvent(string adFormat, string placement, long revenue)
        {
            return new()
            {
                AdsRevenueSourceId = AdRevenueConstants.ARSourceAdMob,
                Revenue            = revenue / 1e6,
                Currency           = "USD",
                Placement          = placement,
                AdNetwork          = "AdMob",
                AdFormat           = adFormat,
                AdUnit             = adFormat,
            };
        }
    }
}