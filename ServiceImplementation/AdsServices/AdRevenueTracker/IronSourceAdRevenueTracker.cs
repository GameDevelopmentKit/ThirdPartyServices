#if EM_IRONSOURCE

namespace ServiceImplementation.AdsServices.AdRevenueTracker
{
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using Core.AnalyticServices.Signal;
    using Zenject;

    public class IronSourceAdRevenueTracker : IAdRevenueTracker
    {
        private readonly IAnalyticServices analyticServices;
        private readonly SignalBus         signalBus;

        public IronSourceAdRevenueTracker(IAnalyticServices analyticServices, SignalBus signalBus)
        {
            this.analyticServices = analyticServices;
            this.signalBus        = signalBus;
            this.SubscribeAdPaidEvent();
        }

        private void SubscribeAdPaidEvent() { IronSourceEvents.onImpressionDataReadyEvent += OnImpressionDataReadyEvent; }

        private void OnImpressionDataReadyEvent(IronSourceImpressionData impressionData)
        {
            if (impressionData.revenue == null) return;

            var adsRevenueEvent = new AdsRevenueEvent()
            {
                AdsRevenueSourceId = AdRevenueConstants.ARSourceAppLovinMAX,
                AdUnit             = impressionData.adUnit,
                Revenue            = impressionData.revenue.Value,
                Currency           = "USD",
                Placement          = impressionData.placement,
                AdNetwork          = impressionData.adNetwork
            };

            this.signalBus.Fire(new AdRevenueSignal(adsRevenueEvent));
            this.analyticServices.Track(adsRevenueEvent);
        }
    }
}
#endif