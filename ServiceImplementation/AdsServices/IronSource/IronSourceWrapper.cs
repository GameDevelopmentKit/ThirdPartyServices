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
    using GameFoundation.Scripts.Utilities.LogService;
    using ServiceImplementation.Configs;
    using ServiceImplementation.Configs.Ads;
    using UnityEngine;
    using Zenject;
    using Debug = UnityEngine.Debug;

    public class IronSourceWrapper : IMRECAdService, IAdServices, IInitializable, IDisposable, IAdLoadService
    {
        #region inject

        private readonly IAnalyticServices  analyticServices;
        private readonly AdServicesConfig   adServicesConfig;
        private readonly SignalBus          signalBus;
        private readonly ThirdPartiesConfig thirdPartiesConfig;
        private readonly ILogService        logService;

        #endregion


        public IronSourceWrapper(IAnalyticServices analyticServices, AdServicesConfig adServicesConfig, SignalBus signalBus, ThirdPartiesConfig thirdPartiesConfig, ILogService logService)
        {
            this.analyticServices   = analyticServices;
            this.adServicesConfig   = adServicesConfig;
            this.signalBus          = signalBus;
            this.thirdPartiesConfig = thirdPartiesConfig;
            this.logService         = logService;
        }

        public string AdPlatform => AdRevenueConstants.ARSourceIronSource;

        private Action onRewardComplete;
        private Action onRewardFailed;
        
        private bool   isGotRewarded;
        private bool   isLoadedAdaptiveBanner;
        private string interstitialPlacement, rewardedPlacement;

        public void Initialize()
        {
            this.logService.Log("oneLog: IronSourceWrapper Initialize");
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
            
            IronSource.Agent.init(this.thirdPartiesConfig.AdSettings.IronSource.AppId);
#if THEONE_ADS_DEBUG
            IronSource.Agent.setAdaptersDebug(true);
            IronSource.Agent.validateIntegration();
#endif
            this.InitAdQuality();
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
            IronSourceInterstitialEvents.onAdReadyEvent      -= this.InterstitialOnAdReadyEvent;
            IronSourceInterstitialEvents.onAdLoadFailedEvent -= this.InterstitialOnAdLoadFailed;
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
            var adInfo = new AdInfo(this.AdPlatform, arg2.adUnit, arg2.adUnit, arg2.adNetwork, value:arg2.revenue ?? 0, currency:"USD");
            this.signalBus.Fire(new RewardedAdCompletedSignal(this.rewardedPlacement, adInfo));
        }

        private Stopwatch rewardedStopwatch;

        private void RewardedVideoOnAdUnavailable()
        {
            this.logService.Log($"oneLog: IronSourceWrapper RewardedVideoOnAdUnavailable");
            this.rewardedStopwatch.Stop();
            this.signalBus.Fire(new RewardedAdLoadFailedSignal("", "", this.rewardedStopwatch.ElapsedMilliseconds));
        }
        private void RewardedVideoOnAdAvailable(IronSourceAdInfo arg1)
        {
            this.rewardedStopwatch.Stop();
            var adInfo = new AdInfo(this.AdPlatform, arg1.adUnit, arg1.adUnit, arg1.adNetwork, value:arg1.revenue ?? 0, currency:"USD");
            this.signalBus.Fire(new RewardedAdLoadedSignal("", this.rewardedStopwatch.ElapsedMilliseconds, adInfo));
        }

        private void RewardedVideoOnAdClosedEvent(IronSourceAdInfo obj)
        {
            var adInfo = new AdInfo(this.AdPlatform, obj.adUnit, obj.adUnit, obj.adNetwork, value:obj.revenue ?? 0, currency:"USD");
            if (!this.isGotRewarded)
            {
                this.onRewardFailed?.Invoke();
                this.onRewardFailed = null;
                this.signalBus.Fire(new RewardedSkippedSignal(this.rewardedPlacement, adInfo));
            }
            this.signalBus.Fire(new RewardedAdClosedSignal(this.rewardedPlacement, adInfo));
        }

        private void RewardedVideoOnAdShowFailedEvent(IronSourceError obj, IronSourceAdInfo info)
        {
            this.onRewardFailed?.Invoke();
            this.onRewardFailed = null;
            this.logService.Log($"oneLog: IronSourceWrapper RewardedVideoOnAdShowFailedEvent. Message: {obj.getDescription()}");
            var adInfo = new AdInfo(this.AdPlatform, info.adUnit, info.adUnit, info.adNetwork, value:info.revenue ?? 0, currency:"USD");
            this.signalBus.Fire(new RewardedAdDisplayFailedSignal(this.rewardedPlacement, obj.getDescription(),adInfo));
        }

        private void RewardedVideoOnAdClickedEvent(IronSourcePlacement obj, IronSourceAdInfo info)
        {
            var adInfo = new AdInfo(this.AdPlatform, info.adUnit, info.adUnit, info.adNetwork, value:info.revenue ?? 0, currency:"USD");
            this.signalBus.Fire(new RewardedAdClickedSignal(this.rewardedPlacement, adInfo));
        }

        private void RewardedVideoOnAdOpenedEvent(IronSourceAdInfo info)
        {
            var adInfo = new AdInfo(this.AdPlatform, info.adUnit, info.adUnit, info.adNetwork, value:info.revenue ?? 0, currency:"USD");
            this.signalBus.Fire(new RewardedAdDisplayedSignal(this.rewardedPlacement, adInfo));
        }

        #endregion


        #region Interstitial

        private void InterstitialOnAdClosedEvent(IronSourceAdInfo obj)
        {
            var adInfo = new AdInfo(this.AdPlatform, obj.adUnit, obj.adUnit, obj.adNetwork, value:obj.revenue ?? 0, currency:"USD");
            this.signalBus.Fire(new InterstitialAdClosedSignal(this.interstitialPlacement, adInfo));
        }

        private void InterstitialOnAdShowFailedEvent(IronSourceError arg1, IronSourceAdInfo arg2)
        {
            this.logService.Log($"oneLog: IronSourceWrapper InterstitialOnAdShowFailedEvent, Message: {arg1.getDescription()}");
            this.signalBus.Fire(new InterstitialAdDisplayedFailedSignal(this.interstitialPlacement));
        }
        private void InterstitialOnAdShowSucceededEvent(IronSourceAdInfo obj)                     { }

        private Stopwatch stopwatchInterstitial;
        private void InterstitialOnAdLoadFailed(IronSourceError obj)
        {
            this.stopwatchInterstitial.Stop();
            this.logService.Log($"oneLog: IronSourceWrapper InterstitialOnAdLoadFailed, Message: {obj.getDescription()}");
            this.signalBus.Fire(new InterstitialAdLoadFailedSignal("", obj.getDescription(), this.stopwatchInterstitial.ElapsedMilliseconds));
        }

        private void InterstitialOnAdReadyEvent(IronSourceAdInfo info)
        {
            this.stopwatchInterstitial.Stop();
            var adInfo = new AdInfo(this.AdPlatform, info.adUnit, info.adUnit, info.adNetwork, value:info.revenue ?? 0, currency:"USD");
            this.signalBus.Fire(new InterstitialAdLoadedSignal("",  this.stopwatchInterstitial.ElapsedMilliseconds, adInfo));
        }

        private void InterstitialOnAdOpenedEvent(IronSourceAdInfo info)
        {
            var adInfo = new AdInfo(this.AdPlatform, info.adUnit, info.adUnit, info.adNetwork, value:info.revenue ?? 0, currency:"USD");
            this.signalBus.Fire(new InterstitialAdDisplayedSignal(this.interstitialPlacement, adInfo));
        }

        private void InterstitialOnAdClickedEvent(IronSourceAdInfo info)
        {
            var adInfo = new AdInfo(this.AdPlatform, info.adUnit, info.adUnit, info.adNetwork, value:info.revenue ?? 0, currency:"USD");
            this.signalBus.Fire(new InterstitialAdClickedSignal(this.interstitialPlacement, adInfo));
        }

        #endregion


        #region Banner

        private async void BannerOnAdLoadFailedEvent(IronSourceError obj)
        {
            this.logService.Log($"oneLog: IronSourceWrapper BannerOnAdLoadFailedEvent, Message: {obj.getDescription()}");
            this.signalBus.Fire(new BannerAdLoadFailedSignal("", $"{obj.getDescription()}"));
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            this.ShowBannerAd();
        }

        private void BannerOnAdLeftApplicationEvent(IronSourceAdInfo obj) { }

        private void BannerOnAdScreenDismissedEvent(IronSourceAdInfo obj)
        {
            var adInfo = new AdInfo(this.AdPlatform, obj.adUnit, obj.adUnit, obj.adNetwork, value:obj.revenue ?? 0, currency:"USD");
            this.signalBus.Fire(new BannerAdDismissedSignal("", adInfo));
        }

        private void BannerOnAdScreenPresentedEvent(IronSourceAdInfo obj)
        {
            var adInfo = new AdInfo(this.AdPlatform, obj.adUnit, obj.adUnit, obj.adNetwork, value:obj.revenue ?? 0, currency:"USD");
            this.signalBus.Fire(new BannerAdPresentedSignal("", adInfo));
        }

        private void BannerOnAdLoadedEvent(IronSourceAdInfo info)
        {
            var adInfo = new AdInfo(this.AdPlatform, info.adUnit, info.adUnit, info.adNetwork, value:info.revenue ?? 0, currency:"USD");
            this.signalBus.Fire(new BannerAdLoadedSignal("", adInfo));
            this.isLoadedBanner = true;
        }

        private void BannerOnAdClickedEvent(IronSourceAdInfo info)
        {
            var adInfo = new AdInfo(this.AdPlatform, info.adUnit, info.adUnit, info.adNetwork, value:info.revenue ?? 0, currency:"USD");
            this.signalBus.Fire(new BannerAdClickedSignal("", adInfo));
        }

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
                AdNetwork          = impressionData.adNetwork,
                AdFormat           = impressionData.adUnit,
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

        public bool TryGetRewardPlacementId(string placement, out string id)
        {
            id = default;
            return false;
        }

        public void LoadInterstitialAd(string place)
        {
            this.stopwatchInterstitial = Stopwatch.StartNew();
            IronSource.Agent.loadInterstitial();
        }

        public bool TryGetInterstitialPlacementId(string placement, out string id)
        {
            id = default;
            return false;
        }

        private void InitAdQuality()
        {
#if IRONSOURCE_AD_QUALITY && IRONSOURCE_AD_QUALITY_DEBUG
            var adQualityConfig = new ISAdQualityConfig
            {
                TestMode = true
            };

            IronSourceAdQuality.Initialize(this.thirdPartiesConfig.AdSettings.IronSource.AppId, adQualityConfig);
            this.logService.Log("onelog: IronSourceAdQuality debug initialize");
#elif IRONSOURCE_AD_QUALITY
            IronSourceAdQuality.Initialize(this.thirdPartiesConfig.AdSettings.IronSource.AppId);
            this.logService.Log("onelog: IronSourceAdQuality initialize");
#endif
        }
    }
}
#endif