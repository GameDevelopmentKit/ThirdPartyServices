#if EM_APPLOVIN
namespace ServiceImplementation.AdsServices.AdRevenueTracker
{
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;

    public class ApplovinAdRevenueTracker : IAdRevenueTracker
    {
        private readonly IAnalyticServices analyticServices;

        public ApplovinAdRevenueTracker(IAnalyticServices analyticServices)
        {
            this.analyticServices = analyticServices;
            this.SubscribeAdPaidEvent();
        }

        private void SubscribeAdPaidEvent()
        {
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent               += this.OnOnAdRevenuePaidEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent         += this.OnOnAdRevenuePaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent             += this.OnOnAdRevenuePaidEvent;
            MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent              += this.OnOnAdRevenuePaidEvent;
            MaxSdkCallbacks.CrossPromo.OnAdRevenuePaidEvent           += this.OnOnAdRevenuePaidEvent;
            MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent                 += this.OnOnAdRevenuePaidEvent;
            MaxSdkCallbacks.RewardedInterstitial.OnAdRevenuePaidEvent += this.OnOnAdRevenuePaidEvent;
        }

        private void OnOnAdRevenuePaidEvent(string adUnitIdentify, MaxSdkBase.AdInfo adInfo)
        {
            this.analyticServices.Track(new AdsRevenueEvent()
            {
                AdsRevenueSourceId = AdRevenueConstants.ARSourceAppLovinMAX,
                AdUnit             = adUnitIdentify,
                Revenue            = adInfo.Revenue,
                Currency           = "USD",
                Placement          = adInfo.Placement,
                AdNetwork          = adInfo.NetworkName
            });
        }
    }
}
#endif