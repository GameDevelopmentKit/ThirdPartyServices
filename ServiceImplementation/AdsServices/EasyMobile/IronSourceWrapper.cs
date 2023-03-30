#if EM_IRONSOURCE
namespace ServiceImplementation.AdsServices.EasyMobile
{
    using System;
    using Core.AdsServices;
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using Core.AnalyticServices.Signal;
    using Zenject;

    public class IronSourceWrapper : IMRECAdService, IInitializable
    {
        private readonly IAnalyticServices analyticServices;
        private readonly SignalBus         signalBus;

        public IronSourceWrapper(IAnalyticServices analyticServices) { this.analyticServices =  analyticServices; }
        public void Initialize() { IronSourceEvents.onImpressionDataReadyEvent               += this.ImpressionDataReadyEvent; }

        public void ShowMREC(AdViewPosition adViewPosition)             { }
        public void HideMREC(AdViewPosition adViewPosition)             { }
        public void StopMRECAutoRefresh(AdViewPosition adViewPosition)  { }
        public void StartMRECAutoRefresh(AdViewPosition adViewPosition) { }
        public void LoadMREC(AdViewPosition adViewPosition)             { }

        public event Action<string, AdInfo>    OnAdLoadedEvent;
        public event Action<string, ErrorInfo> OnAdLoadFailedEvent;
        public event Action<string, AdInfo>    OnAdClickedEvent;
        public event Action<string, AdInfo>    OnAdRevenuePaidEvent;
        public event Action<string, AdInfo>    OnAdExpandedEvent;
        public event Action<string, AdInfo>    OnAdCollapsedEvent;

        private void ImpressionDataReadyEvent(IronSourceImpressionData impressionData)
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