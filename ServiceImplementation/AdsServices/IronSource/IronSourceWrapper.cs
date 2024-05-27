#if IRONSOURCE
namespace ServiceImplementation.AdsServices.EasyMobile
{
    using System;
    using System.Diagnostics;
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using Core.AnalyticServices.Signal;
    using Cysharp.Threading.Tasks;
    using ServiceImplementation.Configs;
    using ServiceImplementation.Configs.Ads;
    using UnityEngine;
    using Zenject;

    public class IronSourceWrapper : IMRECAdService, IAdServices, IInitializable, IDisposable, IAdLoadService
    {
        #region inject

        private readonly IAnalyticServices  analyticServices;
        private readonly AdServicesConfig   adServicesConfig;
        private readonly SignalBus          signalBus;
        private readonly ThirdPartiesConfig thirdPartiesConfig;

        #endregion


        public IronSourceWrapper(IAnalyticServices analyticServices, AdServicesConfig adServicesConfig, SignalBus signalBus, ThirdPartiesConfig thirdPartiesConfig)
        {
            this.analyticServices   = analyticServices;
            this.adServicesConfig   = adServicesConfig;
            this.signalBus          = signalBus;
            this.thirdPartiesConfig = thirdPartiesConfig;
        }

        private Action onRewardComplete;
        private Action onRewardFailed;
        
        private bool   isGotRewarded;
        private bool   isLoadedAdaptiveBanner;
        private string interstitialPlacement, rewardedPlacement;

        public void Initialize()
        {
            IronSource.Agent.init(this.thirdPartiesConfig.AdSettings.IronSource.AppId);

            IronSourceEvents.onImpressionDataReadyEvent += this.ImpressionDataReadyEvent;
            //Add AdInfo Rewarded Video Events
            IronSourceRewardedVideoEvents.onAdOpenedEvent      += this.RewardedVideoOnAdOpenedEvent;
            IronSourceRewardedVideoEvents.onAdClosedEvent      += this.RewardedVideoOnAdClosedEvent;
            IronSourceRewardedVideoEvents.onAdAvailableEvent   += this.RewardedVideoOnAdAvailable;
            IronSourceRewardedVideoEvents.onAdUnavailableEvent += this.RewardedVideoOnAdUnavailable;
            IronSourceRewardedVideoEvents.onAdShowFailedEvent  += this.RewardedVideoOnAdShowFailedEvent;
            IronSourceRewardedVideoEvents.onAdRewardedEvent    += this.RewardedVideoOnAdRewardedEvent;
            IronSourceRewardedVideoEvents.onAdClickedEvent     += this.RewardedVideoOnAdClickedEvent;


            //Add AdInfo Interstitial Events
            IronSourceInterstitialEvents.onAdReadyEvent         += this.InterstitialOnAdReadyEvent;
            IronSourceInterstitialEvents.onAdLoadFailedEvent    += this.InterstitialOnAdLoadFailed;
            IronSourceInterstitialEvents.onAdOpenedEvent        += this.InterstitialOnAdOpenedEvent;
            IronSourceInterstitialEvents.onAdClickedEvent       += this.InterstitialOnAdClickedEvent;
            IronSourceInterstitialEvents.onAdShowSucceededEvent += this.InterstitialOnAdShowSucceededEvent;
            IronSourceInterstitialEvents.onAdShowFailedEvent    += this.InterstitialOnAdShowFailedEvent;
            IronSourceInterstitialEvents.onAdClosedEvent        += this.InterstitialOnAdClosedEvent;

            //Add AdInfo Banner Events
            IronSourceBannerEvents.onAdLoadedEvent          += this.BannerOnAdLoadedEvent;
            IronSourceBannerEvents.onAdLoadFailedEvent      += this.BannerOnAdLoadFailedEvent;
            IronSourceBannerEvents.onAdClickedEvent         += this.BannerOnAdClickedEvent;
            IronSourceBannerEvents.onAdScreenPresentedEvent += this.BannerOnAdScreenPresentedEvent;
            IronSourceBannerEvents.onAdScreenDismissedEvent += this.BannerOnAdScreenDismissedEvent;
            IronSourceBannerEvents.onAdLeftApplicationEvent += this.BannerOnAdLeftApplicationEvent;
        }

        public void Dispose()
        {
            IronSourceEvents.onImpressionDataReadyEvent -= this.ImpressionDataReadyEvent;

            //Add AdInfo Rewarded Video Events
            IronSourceRewardedVideoEvents.onAdOpenedEvent      -= this.RewardedVideoOnAdOpenedEvent;
            IronSourceRewardedVideoEvents.onAdClosedEvent      -= this.RewardedVideoOnAdClosedEvent;
            IronSourceRewardedVideoEvents.onAdAvailableEvent   -= this.RewardedVideoOnAdAvailable;
            IronSourceRewardedVideoEvents.onAdUnavailableEvent -= this.RewardedVideoOnAdUnavailable;
            IronSourceRewardedVideoEvents.onAdShowFailedEvent  -= this.RewardedVideoOnAdShowFailedEvent;
            IronSourceRewardedVideoEvents.onAdRewardedEvent    -= this.RewardedVideoOnAdRewardedEvent;
            IronSourceRewardedVideoEvents.onAdClickedEvent     -= this.RewardedVideoOnAdClickedEvent;


            //Add AdInfo Interstitial Events
            IronSourceInterstitialEvents.onAdReadyEvent         -= this.InterstitialOnAdReadyEvent;
            IronSourceInterstitialEvents.onAdLoadFailedEvent    -= this.InterstitialOnAdLoadFailed;
            IronSourceInterstitialEvents.onAdOpenedEvent        -= this.InterstitialOnAdOpenedEvent;
            IronSourceInterstitialEvents.onAdClickedEvent       -= this.InterstitialOnAdClickedEvent;
            IronSourceInterstitialEvents.onAdShowSucceededEvent -= this.InterstitialOnAdShowSucceededEvent;
            IronSourceInterstitialEvents.onAdShowFailedEvent    -= this.InterstitialOnAdShowFailedEvent;
            IronSourceInterstitialEvents.onAdClosedEvent        -= this.InterstitialOnAdClosedEvent;

            //Add AdInfo Banner Events
            IronSourceBannerEvents.onAdLoadedEvent          -= this.BannerOnAdLoadedEvent;
            IronSourceBannerEvents.onAdLoadFailedEvent      -= this.BannerOnAdLoadFailedEvent;
            IronSourceBannerEvents.onAdClickedEvent         -= this.BannerOnAdClickedEvent;
            IronSourceBannerEvents.onAdScreenPresentedEvent -= this.BannerOnAdScreenPresentedEvent;
            IronSourceBannerEvents.onAdScreenDismissedEvent -= this.BannerOnAdScreenDismissedEvent;
            IronSourceBannerEvents.onAdLeftApplicationEvent -= this.BannerOnAdLeftApplicationEvent;
        }

        #region Rewarded

        private void RewardedVideoOnAdRewardedEvent(IronSourcePlacement arg1, IronSourceAdInfo arg2)
        {
            this.isGotRewarded = true;
            this.onRewardComplete?.Invoke();
            this.onRewardComplete = null;
            this.signalBus.Fire(new RewardedAdCompletedSignal(this.rewardedPlacement));
        }

        private Stopwatch rewardedStopwatch;

        private void RewardedVideoOnAdUnavailable()
        {
            this.rewardedStopwatch.Stop();
            this.signalBus.Fire(new RewardedAdLoadFailedSignal("", "", this.rewardedStopwatch.ElapsedMilliseconds));
        }
        private void RewardedVideoOnAdAvailable(IronSourceAdInfo obj)
        {
            this.rewardedStopwatch.Stop();
            this.signalBus.Fire(new RewardedAdLoadedSignal("", this.rewardedStopwatch.ElapsedMilliseconds));
        }

        private void RewardedVideoOnAdClosedEvent(IronSourceAdInfo obj)
        {
            if (!this.isGotRewarded)
            {
                this.onRewardFailed?.Invoke();
                this.onRewardFailed = null;
                this.signalBus.Fire(new RewardedSkippedSignal(this.rewardedPlacement));
            }
            this.signalBus.Fire(new RewardedAdClosedSignal(this.rewardedPlacement));
        }

        private void RewardedVideoOnAdShowFailedEvent(IronSourceError obj, IronSourceAdInfo info)
        {
            this.onRewardFailed?.Invoke();
            this.onRewardFailed = null;
            this.signalBus.Fire(new RewardedAdShowFailedSignal(this.rewardedPlacement));
        }

        private void RewardedVideoOnAdClickedEvent(IronSourcePlacement obj, IronSourceAdInfo info) { this.signalBus.Fire(new RewardedAdClickedSignal(this.rewardedPlacement)); }

        private void RewardedVideoOnAdOpenedEvent(IronSourceAdInfo info) { this.signalBus.Fire(new RewardedAdDisplayedSignal(this.rewardedPlacement)); }

        #endregion


        #region Interstitial

        private void InterstitialOnAdClosedEvent(IronSourceAdInfo obj)                            { this.signalBus.Fire(new InterstitialAdClosedSignal(this.interstitialPlacement)); }
        private void InterstitialOnAdShowFailedEvent(IronSourceError arg1, IronSourceAdInfo arg2) { this.signalBus.Fire(new InterstitialAdDisplayedFailedSignal(this.interstitialPlacement)); }
        private void InterstitialOnAdShowSucceededEvent(IronSourceAdInfo obj)                     { this.signalBus.Fire(new InterstitialAdDisplayedSignal(this.interstitialPlacement)); }

        private void InterstitialOnAdLoadFailed(IronSourceError obj) { this.signalBus.Fire(new InterstitialAdLoadFailedSignal("", obj.getDescription())); }

        private void InterstitialOnAdReadyEvent(IronSourceAdInfo info) { this.signalBus.Fire(new InterstitialAdDownloadedSignal("")); }

        private void InterstitialOnAdOpenedEvent(IronSourceAdInfo info) { this.signalBus.Fire(new InterstitialAdDisplayedSignal(this.interstitialPlacement)); }

        private void InterstitialOnAdClickedEvent(IronSourceAdInfo info) { this.signalBus.Fire(new InterstitialAdClickedSignal(this.interstitialPlacement)); }

        #endregion


        #region Banner

        private async void BannerOnAdLoadFailedEvent(IronSourceError obj)
        {
            this.signalBus.Fire(new BannerAdLoadFailedSignal("", $"{obj.getDescription()}"));
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            this.ShowBannerAd();
        }

        private void BannerOnAdLeftApplicationEvent(IronSourceAdInfo obj) { }
        private void BannerOnAdScreenDismissedEvent(IronSourceAdInfo obj) { this.signalBus.Fire(new BannerAdDismissedSignal("")); }
        private void BannerOnAdScreenPresentedEvent(IronSourceAdInfo obj) { this.signalBus.Fire(new BannerAdPresentedSignal("")); }

        private void BannerOnAdLoadedEvent(IronSourceAdInfo info)
        {
            this.signalBus.Fire(new BannerAdLoadedSignal(""));
            this.isLoadedBanner = true;
        }

        private void BannerOnAdClickedEvent(IronSourceAdInfo info) { this.signalBus.Fire(new BannerAdClickedSignal("")); }

        #endregion


        #region MREC

        public void ShowMREC(AdViewPosition adViewPosition)
        {
            if (!this.adServicesConfig.EnableMRECAd) return;
        }

        public void HideMREC(AdViewPosition adViewPosition)             { }
        public void StopMRECAutoRefresh(AdViewPosition adViewPosition)  { }
        public void StartMRECAutoRefresh(AdViewPosition adViewPosition) { }
        public void LoadMREC(AdViewPosition adViewPosition)             { }
        public bool IsMRECReady(AdViewPosition adViewPosition)          { return false; }
        public void HideAllMREC()                                       { }

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
        private bool isLoadedBanner;

        public void ShowBannerAd(BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom, int width = 320, int height = 50)
        {
            if (this.isLoadedBanner)
            {
                IronSource.Agent.displayBanner();
                return;
            }

            var position = bannerAdsPosition switch
            {
                BannerAdsPosition.Top => IronSourceBannerPosition.TOP,
                _                     => IronSourceBannerPosition.BOTTOM
            };
            IronSource.Agent.loadBanner(this.GetBannerSize(), position);
            this.isLoadedAdaptiveBanner = true;
        }

        private IronSourceBannerSize GetBannerSize()
        {
            var bannerSize = IronSourceBannerSize.BANNER;

#if ADMOB
            if (this.thirdPartiesConfig.AdSettings.IronSource.IsAdaptiveBanner && !this.isLoadedAdaptiveBanner)
            {
                var width = (int)(Screen.width / GoogleMobileAds.Api.MobileAds.Utils.GetDeviceScale());
                bannerSize = new IronSourceBannerSize(width, 60);
                bannerSize.SetAdaptive(true);
            }
#endif

            return bannerSize;
        }

        public void              HideBannedAd()                      { IronSource.Agent.hideBanner(); }
        public void              DestroyBannerAd()                   { IronSource.Agent.destroyBanner(); }
        public bool              IsInterstitialAdReady(string place) { return IronSource.Agent.isInterstitialReady(); }

        public void ShowInterstitialAd(string place)
        {
            this.interstitialPlacement = place;
            IronSource.Agent.showInterstitial(place);
        }
        public AdNetworkSettings AdNetworkSettings                   => this.thirdPartiesConfig.AdSettings.IronSource;
        public bool              IsRewardedAdReady(string place)     { return IronSource.Agent.isRewardedVideoAvailable(); }

        public void ShowRewardedAd(string place, Action onCompleted, Action onFailed)
        {
            this.rewardedPlacement = place;
            this.isGotRewarded = false;
            IronSource.Agent.showRewardedVideo(place);
            this.onRewardComplete = onCompleted;
            this.onRewardFailed   = onFailed;
        }

        public void RemoveAds(bool revokeConsent = false) { PlayerPrefs.SetInt("EM_REMOVE_ADS", -1); }

        public bool IsAdsInitialized() { return true; }

        public bool IsRemoveAds() { return PlayerPrefs.HasKey("EM_REMOVE_ADS"); }

        #endregion

        public void LoadRewardAds(string place)
        {
            this.rewardedStopwatch = Stopwatch.StartNew();
            IronSource.Agent.loadRewardedVideo();
        }
        public void LoadInterstitialAd(string place) { IronSource.Agent.loadInterstitial(); }
    }
}
#endif