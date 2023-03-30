#if EM_IRONSOURCE
namespace ServiceImplementation.AdsServices.EasyMobile
{
    using System;
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using Core.AnalyticServices.Signal;
    using Zenject;

    public class IronSourceWrapper : IMRECAdService, IInitializable
    {
        private readonly IAnalyticServices analyticServices;
        private readonly SignalBus         signalBus;

        public IronSourceWrapper(IAnalyticServices analyticServices) { this.analyticServices = analyticServices; }

        public void Initialize()
        {
            IronSourceEvents.onImpressionDataReadyEvent += this.ImpressionDataReadyEvent;

            IronSourceEvents.onBannerAdClickedEvent         += this.OnBannerClicked;
            IronSourceEvents.onBannerAdLoadedEvent          += this.OnBannerLoaded;
            IronSourceEvents.onBannerAdLoadFailedEvent      += this.OnBannerLoadFailed;

            IronSourceEvents.onInterstitialAdClickedEvent    += this.OnInterstitialClicked;
            IronSourceEvents.onInterstitialAdOpenedEvent     += this.OnInterstitialOpened;
            IronSourceEvents.onInterstitialAdReadyEvent      += this.OnInterstitialReady;
            IronSourceEvents.onInterstitialAdLoadFailedEvent += this.OnInterstitialLoadFailed;

            IronSourceEvents.onRewardedVideoAdClickedEvent    += this.OnRewardedVideoClicked;
            
            IronSourceEvents.onRewardedVideoAdOpenedEvent     += this.OnRewardedVideoOpened;
            IronSourceEvents.onRewardedVideoAdReadyEvent      += this.OnRewardedVideoReady;
            IronSourceEvents.onRewardedVideoAdLoadFailedEvent += this.OnRewardedVideoLoadFailed;
        }

        private void OnRewardedVideoLoadFailed(IronSourceError obj) { this.signalBus.Fire(new RewardedAdLoadFailedSignal("", obj.getDescription())); }

        private void OnRewardedVideoReady() { this.signalBus.Fire(new RewardedAdLoadedSignal("")); }

        private void OnRewardedVideoClicked(IronSourcePlacement obj) { this.signalBus.Fire(new RewardedAdLoadClickedSignal(obj.getPlacementName())); }

        private void OnRewardedVideoOpened() { this.signalBus.Fire(new RewardedAdDisplayedSignal("")); }

        private void OnInterstitialLoadFailed(IronSourceError obj) { this.signalBus.Fire(new InterstitialAdLoadFailedSignal("", obj.getDescription())); }

        private void OnInterstitialReady() { this.signalBus.Fire(new InterstitialAdDownloadedSignal("")); }

        private void OnInterstitialOpened() { this.signalBus.Fire(new InterstitialAdDisplayedSignal("")); }

        private void OnInterstitialClicked() { this.signalBus.Fire(new InterstitialAdClickedSignal("")); }

        private void OnBannerLoadFailed(IronSourceError obj) { this.signalBus.Fire(new BannerAdLoadFailedSignal("", $"{obj.getDescription()}")); }

        private void OnBannerLoaded() { this.signalBus.Fire(new BannerAdLoadedSignal("")); }

        private void OnBannerClicked() { this.signalBus.Fire(new BannerAdClickedSignal("")); }

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
                AdsRevenueSourceId = AdRevenueConstants.ARSourceIronSource,
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