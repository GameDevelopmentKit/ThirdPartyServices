#if EM_IRONSOURCE
namespace ServiceImplementation.AdsServices.EasyMobile
{
    using System;
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using Core.AnalyticServices.Signal;
    using UnityEngine;
    using Zenject;

    public class IronSourceWrapper : IMRECAdService, IInitializable, IDisposable, IAdServices
    {
        private readonly IAnalyticServices analyticServices;
        private readonly AdServicesConfig  adServicesConfig;
        private readonly SignalBus         signalBus;

        public IronSourceWrapper(IAnalyticServices analyticServices,AdServicesConfig adServicesConfig, SignalBus signalBus)
        {
            this.analyticServices = analyticServices;
            this.adServicesConfig = adServicesConfig;
            this.signalBus        = signalBus;
        }

        public void Initialize()
        {
            // IronSource.Agent.init();
            
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

            IronSourceEvents.onRewardedVideoAdClickedEvent    += this.OnRewardedVideoClicked;
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

            IronSourceEvents.onRewardedVideoAdClickedEvent    -= this.OnRewardedVideoClicked;
            IronSourceEvents.onRewardedVideoAdOpenedEvent     -= this.OnRewardedVideoOpened;
            IronSourceEvents.onRewardedVideoAdReadyEvent      -= this.OnRewardedVideoReady;
            IronSourceEvents.onRewardedVideoAdLoadFailedEvent -= this.OnRewardedVideoLoadFailed;
        }

        private void OnRewardedVideoLoadFailed(IronSourceError obj)
        {
            this.signalBus.Fire(new RewardedAdLoadFailedSignal("", obj.getDescription()));
        }

        private void OnRewardedVideoReady()
        {
            this.signalBus.Fire(new RewardedAdLoadedSignal(""));
        }

        private void OnRewardedVideoClicked(IronSourcePlacement obj)
        {
            this.signalBus.Fire(new RewardedAdLoadClickedSignal(obj.getPlacementName()));
        }

        private void OnRewardedVideoOpened()
        {
            this.signalBus.Fire(new RewardedAdDisplayedSignal(""));
        }

        private void OnInterstitialLoadFailed(IronSourceError obj)
        {
            this.signalBus.Fire(new InterstitialAdLoadFailedSignal("", obj.getDescription()));
        }

        private void OnInterstitialReady()
        {
            this.signalBus.Fire(new InterstitialAdDownloadedSignal(""));
        }

        private void OnInterstitialOpened()
        {
            this.signalBus.Fire(new InterstitialAdDisplayedSignal(""));
        }

        private void OnInterstitialClicked()
        {
            this.signalBus.Fire(new InterstitialAdClickedSignal(""));
        }

        private void OnBannerLoadFailed(IronSourceError obj)
        {
            this.signalBus.Fire(new BannerAdLoadFailedSignal("", $"{obj.getDescription()}"));
        }

        private void OnBannerScreenDismissed()
        {
            this.signalBus.Fire(new BannerAdDismissedSignal(""));
        }

        private void OnBannerScreenPresented()
        {
            this.signalBus.Fire(new BannerAdPresentedSignal(""));
        }

        private void OnBannerLoaded()
        {
            this.signalBus.Fire(new BannerAdLoadedSignal(""));
        }

        private void OnBannerClicked()
        {
            this.signalBus.Fire(new BannerAdClickedSignal(""));
        }

        #region MREC

        public void ShowMREC(AdViewPosition adViewPosition)
        {
            if(!this.adServicesConfig.EnableMRECAd)return;
        }
        public void HideMREC(AdViewPosition adViewPosition)             { }
        public void StopMRECAutoRefresh(AdViewPosition adViewPosition)  { }
        public void StartMRECAutoRefresh(AdViewPosition adViewPosition) { }
        public void LoadMREC(AdViewPosition adViewPosition)             { }
        public bool IsMRECReady(AdViewPosition adViewPosition)          { return false; }

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

        #region AdService

        //todo convert ads position
        public void ShowBannerAd(BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom, int width = 320, int height = 50)
        {
            IronSource.Agent.loadBanner(new IronSourceBannerSize(width, height), IronSourceBannerPosition.BOTTOM);
        }
        public void HideBannedAd()
        {
            IronSource.Agent.hideBanner();
        }
        public void DestroyBannerAd()
        {
            IronSource.Agent.destroyBanner();
        }
        public bool IsInterstitialAdReady(string place)
        {
            return IronSource.Agent.isInterstitialReady();
        }
        public void ShowInterstitialAd(string place)
        {
            IronSource.Agent.showInterstitial();
        }
        public bool IsRewardedAdReady(string place)
        {
            return IronSource.Agent.isRewardedVideoAvailable();
        }
        public void ShowRewardedAd(string place, Action onCompleted)
        {
            IronSource.Agent.showRewardedVideo();
        }

        public void RemoveAds(bool revokeConsent = false)
        {
            PlayerPrefs.SetInt("EM_REMOVE_ADS", -1);
        }

        public bool IsAdsInitialized() { return true; }

        public bool IsRemoveAds() { return PlayerPrefs.HasKey("EM_REMOVE_ADS"); }
        #endregion
    }
}
#endif