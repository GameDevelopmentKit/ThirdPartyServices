#if APPLOVIN
namespace ServiceImplementation.AdsServices.AdRevenueTracker
{
    using Core.AdsServices.Signals;
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
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += this.OnOnAdRevenuePaidEvent;
            // MaxSdkCallbacks.Banner.OnAdLoadedEvent      += this.OnAdRevenueLoadedEvent;
            // MaxSdkCallbacks.Banner.OnAdClickedEvent     += this.OnAdRevenueClickedEvent;

            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += this.OnOnAdRevenuePaidEvent;
            // MaxSdkCallbacks.Interstitial.OnAdLoadedEvent      += this.OnAdRevenueLoadedEvent;
            // MaxSdkCallbacks.Interstitial.OnAdClickedEvent     += this.OnAdRevenueClickedEvent;

            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += this.OnOnAdRevenuePaidEvent;
            // MaxSdkCallbacks.Rewarded.OnAdLoadedEvent      += this.OnAdRevenueLoadedEvent;
            // MaxSdkCallbacks.Rewarded.OnAdClickedEvent     += this.OnAdRevenueClickedEvent;

            MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += this.OnOnAdRevenuePaidEvent;
            // MaxSdkCallbacks.AppOpen.OnAdLoadedEvent      += this.OnAdRevenueLoadedEvent;
            // MaxSdkCallbacks.AppOpen.OnAdClickedEvent     += this.OnAdRevenueClickedEvent;

            MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += this.OnOnAdRevenuePaidEvent;
            // MaxSdkCallbacks.MRec.OnAdLoadedEvent      += this.OnAdRevenueLoadedEvent;
            // MaxSdkCallbacks.MRec.OnAdClickedEvent     += this.OnAdRevenueClickedEvent;

            MaxSdkCallbacks.RewardedInterstitial.OnAdRevenuePaidEvent += this.OnOnAdRevenuePaidEvent;
            // MaxSdkCallbacks.RewardedInterstitial.OnAdLoadedEvent      += this.OnAdRevenueLoadedEvent;
            // MaxSdkCallbacks.RewardedInterstitial.OnAdClickedEvent     += this.OnAdRevenueClickedEvent;
        }
        
        private void OnAdRevenueClickedEvent(string adUnitIdentify, MaxSdkBase.AdInfo adInfo)
        {
            var adsRevenueEvent = this.CreateAdsRevenueEvent(adInfo);
            if (adsRevenueEvent == null) return;

            this.signalBus.Fire(new AdRevenueClickedSignal(adsRevenueEvent, adInfo.NetworkPlacement));
        }

        private void OnAdRevenueLoadedEvent(string adUnitIdentify, MaxSdkBase.AdInfo adInfo)
        {
            var adsRevenueEvent = this.CreateAdsRevenueEvent(adInfo);
            if (adsRevenueEvent == null) return;

            this.signalBus.Fire(new AdRevenueLoadedSignal(adsRevenueEvent, adInfo.NetworkPlacement));
        }

        private void OnOnAdRevenuePaidEvent(string adUnitIdentify, MaxSdkBase.AdInfo adInfo)
        {
            var adsRevenueEvent = this.CreateAdsRevenueEvent(adInfo);
            if (adsRevenueEvent == null) return;

            this.signalBus.Fire(new AdRevenueSignal(adsRevenueEvent, adInfo.NetworkPlacement));

            this.analyticServices.Track(adsRevenueEvent);
        }

        private AdsRevenueEvent CreateAdsRevenueEvent(MaxSdkBase.AdInfo adInfo)
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
    }
}
#endif