#if EM_IRONSOURCE
namespace ServiceImplementation.AdsServices.AdRevenueTracker
{
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;

    public class IronSourceAdRevenueTracker : IAdRevenueTracker
    {
        private readonly IAnalyticServices analyticServices;

        public IronSourceAdRevenueTracker(IAnalyticServices analyticServices)
        {
            this.analyticServices = analyticServices;
            this.SubscribeAdPaidEvent();
        }

        private void SubscribeAdPaidEvent() { IronSourceEvents.onImpressionDataReadyEvent += OnImpressionDataReadyEvent; }

        private void OnImpressionDataReadyEvent(IronSourceImpressionData impressionData)
        {
            if (impressionData.revenue != null)
                this.analyticServices.Track(new AdsRevenueEvent()
                {
                    AdsRevenueSourceId = AdRevenueConstants.ARSourceIronSource,
                    AdUnit             = impressionData.adUnit,
                    Revenue            = impressionData.revenue.Value,
                    Currency           = "USD",
                    Placement          = impressionData.placement,
                    AdNetwork          = impressionData.adNetwork
                });
        }
    }
}
#endif