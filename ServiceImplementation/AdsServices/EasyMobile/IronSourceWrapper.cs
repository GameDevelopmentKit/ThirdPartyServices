#if EM_IRONSOURCE
namespace ServiceImplementation.AdsServices.EasyMobile
{
    using System;
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using Core.AnalyticServices.Signal;
    using Cysharp.Threading.Tasks;
    using Zenject;

    public class IronSourceWrapper : IMRECAdService, IInitializable, IDisposable
    {
        private readonly IAnalyticServices analyticServices;
        private readonly SignalBus         signalBus;

        public IronSourceWrapper(IAnalyticServices analyticServices, SignalBus signalBus)
        {
            this.analyticServices = analyticServices;
            this.signalBus        = signalBus;
        }

        public void Initialize()
        {
            IronSourceEvents.onImpressionDataReadyEvent += this.ImpressionDataReadyEvent;

            IronSourceEvents.onBannerAdClickedEvent         += this.OnBannerClicked;
            IronSourceEvents.onBannerAdLoadedEvent          += this.OnBannerLoaded;
            IronSourceEvents.onBannerAdLoadFailedEvent      += this.OnBannerLoadFailed;
            IronSourceEvents.onBannerAdScreenPresentedEvent += this.OnBannerScreenPresented;
            IronSourceEvents.onBannerAdScreenDismissedEvent += this.OnBannerScreenDismissed;

            IronSourceEvents.onInterstitialAdClickedEvent    += this.OnInterstitialClicked;
            IronSourceEvents.onInterstitialAdOpenedEvent     += this.OnInterstitialOpened;
            IronSourceEvents.onInterstitialAdReadyEvent      += this.OnInterstitialReady;
            IronSourceEvents.onInterstitialAdLoadFailedEvent += this.OnInterstitialLoadFailed;

            IronSourceEvents.onRewardedVideoAdClickedEvent += this.OnRewardedVideoClicked;

            IronSourceEvents.onRewardedVideoAdOpenedEvent     += this.OnRewardedVideoOpened;
            IronSourceEvents.onRewardedVideoAdReadyEvent      += this.OnRewardedVideoReady;
            IronSourceEvents.onRewardedVideoAdLoadFailedEvent += this.OnRewardedVideoLoadFailed;
        }

        public void Dispose()
        {
            IronSourceEvents.onImpressionDataReadyEvent -= this.ImpressionDataReadyEvent;

            IronSourceEvents.onBannerAdClickedEvent    -= this.OnBannerClicked;
            IronSourceEvents.onBannerAdLoadedEvent     -= this.OnBannerLoaded;
            IronSourceEvents.onBannerAdLoadFailedEvent -= this.OnBannerLoadFailed;

            IronSourceEvents.onInterstitialAdClickedEvent    -= this.OnInterstitialClicked;
            IronSourceEvents.onInterstitialAdOpenedEvent     -= this.OnInterstitialOpened;
            IronSourceEvents.onInterstitialAdReadyEvent      -= this.OnInterstitialReady;
            IronSourceEvents.onInterstitialAdLoadFailedEvent -= this.OnInterstitialLoadFailed;

            IronSourceEvents.onRewardedVideoAdClickedEvent -= this.OnRewardedVideoClicked;

            IronSourceEvents.onRewardedVideoAdOpenedEvent     -= this.OnRewardedVideoOpened;
            IronSourceEvents.onRewardedVideoAdReadyEvent      -= this.OnRewardedVideoReady;
            IronSourceEvents.onRewardedVideoAdLoadFailedEvent -= this.OnRewardedVideoLoadFailed;
        }

        private async void OnRewardedVideoLoadFailed(IronSourceError obj)
        {
            await UniTask.SwitchToMainThread();
            this.signalBus.Fire(new RewardedAdLoadFailedSignal("", obj.getDescription()));
        }

        private async void OnRewardedVideoReady()
        {
            await UniTask.SwitchToMainThread();
            this.signalBus.Fire(new RewardedAdLoadedSignal(""));
        }

        private async void OnRewardedVideoClicked(IronSourcePlacement obj)
        {
            await UniTask.SwitchToMainThread();
            this.signalBus.Fire(new RewardedAdLoadClickedSignal(obj.getPlacementName()));
        }

        private async void OnRewardedVideoOpened()
        {
            await UniTask.SwitchToMainThread();
            this.signalBus.Fire(new RewardedAdDisplayedSignal(""));
        }

        private async void OnInterstitialLoadFailed(IronSourceError obj)
        {
            await UniTask.SwitchToMainThread();
            this.signalBus.Fire(new InterstitialAdLoadFailedSignal("", obj.getDescription()));
        }

        private async void OnInterstitialReady()
        {
            await UniTask.SwitchToMainThread();
            this.signalBus.Fire(new InterstitialAdDownloadedSignal(""));
        }

        private async void OnInterstitialOpened()
        {
            await UniTask.SwitchToMainThread();
            this.signalBus.Fire(new InterstitialAdDisplayedSignal(""));
        }

        private async void OnInterstitialClicked()
        {
            await UniTask.SwitchToMainThread();
            this.signalBus.Fire(new InterstitialAdClickedSignal(""));
        }

        private async void OnBannerLoadFailed(IronSourceError obj)
        {
            await UniTask.SwitchToMainThread();
            this.signalBus.Fire(new BannerAdLoadFailedSignal("", $"{obj.getDescription()}"));
        }

        private async void OnBannerScreenDismissed()
        {
            await UniTask.SwitchToMainThread();
            this.signalBus.Fire(new BannerAdDismissedSignal(""));
        }
        private async void OnBannerScreenPresented()
        {
            await UniTask.SwitchToMainThread();
            this.signalBus.Fire(new BannerAdDismissedSignal(""));
        }

        private async void OnBannerLoaded()
        {
            await UniTask.SwitchToMainThread();
            this.signalBus.Fire(new BannerAdLoadedSignal(""));
        }

        private async void OnBannerClicked()
        {
            await UniTask.SwitchToMainThread();
            this.signalBus.Fire(new BannerAdClickedSignal(""));
        }

        #region MREC

        public void ShowMREC(AdViewPosition             adViewPosition) { }
        public void HideMREC(AdViewPosition             adViewPosition) { }
        public void StopMRECAutoRefresh(AdViewPosition  adViewPosition) { }
        public void StartMRECAutoRefresh(AdViewPosition adViewPosition) { }
        public void LoadMREC(AdViewPosition             adViewPosition) { }
        public bool IsMRECReady(AdViewPosition              adViewPosition) { return false; }

        public event Action<string, AdInfo>    OnAdLoadedEvent;
        public event Action<string, ErrorInfo> OnAdLoadFailedEvent;
        public event Action<string, AdInfo>    OnAdClickedEvent;
        public event Action<string, AdInfo>    OnAdRevenuePaidEvent;
        public event Action<string, AdInfo>    OnAdExpandedEvent;
        public event Action<string, AdInfo>    OnAdCollapsedEvent;

        #endregion

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