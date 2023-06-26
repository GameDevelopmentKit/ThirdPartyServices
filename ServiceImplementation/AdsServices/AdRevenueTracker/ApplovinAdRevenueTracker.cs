#if APPLOVIN
namespace ServiceImplementation.AdsServices.AdRevenueTracker
{
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using Core.AnalyticServices.Signal;
    using Zenject;

    public class ApplovinAdRevenueTracker : IAdRevenueTracker
    {
        private readonly IAnalyticServices analyticServices;
        private readonly SignalBus         signalBus;

        public ApplovinAdRevenueTracker(IAnalyticServices analyticServices, SignalBus signalBus)
        {
            this.analyticServices = analyticServices;
            this.signalBus        = signalBus;
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
            if (adInfo == null) return;

            var adsRevenueEvent = new AdsRevenueEvent()
            {
                AdsRevenueSourceId = AdRevenueConstants.ARSourceAppLovinMAX,
                AdUnit             = adUnitIdentify,
                Revenue            = adInfo.Revenue,
                Currency           = "USD",
                Placement          = adInfo.Placement,
                AdNetwork          = adInfo.NetworkName,
                AdFormat           = adInfo.AdFormat
            };

            this.signalBus.Fire(new AdRevenueSignal(adsRevenueEvent));

            this.analyticServices.Track(adsRevenueEvent);
        }
    }
}
#endif