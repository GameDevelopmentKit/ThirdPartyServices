#if APPLOVIN
namespace ServiceImplementation.AdsServices.AdRevenueTracker
{
    using Core.AdsServices.Signals;
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using Core.AnalyticServices.Signal;
    using GameFoundation.DI;
    using GameFoundation.Signals;
    using UnityEngine.Scripting;

    public class ApplovinAdRevenueTracker : IAdRevenueTracker, IInitializable
    {
        private readonly IAnalyticServices analyticServices;
        private readonly SignalBus         signalBus;

        [Preserve]
        public ApplovinAdRevenueTracker(IAnalyticServices analyticServices, SignalBus signalBus)
        {
            this.analyticServices = analyticServices;
            this.signalBus = signalBus;
        }

        public void Initialize()
        {
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += this.OnOnAdRevenuePaidEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += this.OnOnAdRevenuePaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += this.OnOnAdRevenuePaidEvent;
            MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += this.OnOnAdRevenuePaidEvent;
            MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += this.OnOnAdRevenuePaidEvent;
            MaxSdkCallbacks.RewardedInterstitial.OnAdRevenuePaidEvent += this.OnOnAdRevenuePaidEvent;
        }

        private void OnOnAdRevenuePaidEvent(string adUnitIdentify, MaxSdkBase.AdInfo adInfo)
        {
            var adsRevenueEvent = this.CreateAdsRevenueEvent(adInfo);
            if (adsRevenueEvent == null) return;

            this.signalBus.Fire(new AdRevenueSignal(adsRevenueEvent));

            this.analyticServices.Track(adsRevenueEvent);
        }

        private AdsRevenueEvent CreateAdsRevenueEvent(MaxSdkBase.AdInfo adInfo)
        {
            if (adInfo == null) return null;
            return new()
            {
                AdsRevenueSourceId = AdRevenueConstants.ARSourceAppLovinMAX,
                AdUnit = adInfo.AdUnitIdentifier,
                Revenue = adInfo.Revenue,
                Currency = "USD",
                NetworkPlacement = adInfo.NetworkPlacement,
                Placement = adInfo.Placement,
                AdNetwork = adInfo.NetworkName,
                AdFormat = adInfo.AdFormat,
            };
        }
    }
}
#endif