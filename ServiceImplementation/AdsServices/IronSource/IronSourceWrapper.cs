#if !IRONSOURCE
namespace ServiceImplementation.AdsServices.EasyMobile
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using com.unity3d.mediation;
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using Core.AnalyticServices.Signal;
    using Cysharp.Threading.Tasks;
    using GameFoundation.DI;
    using GameFoundation.Signals;
    using ServiceImplementation.Configs;
    using ServiceImplementation.Configs.Ads;
    using TheOne.Logging;
    using UnityEngine.Scripting;
    using ILogger = TheOne.Logging.ILogger;

    public class IronSourceWrapper : IMRECAdService, IAdServices, IInitializable, IDisposable, IAdLoadService
    {
        #region inject

        private readonly IAnalyticServices  analyticServices;
        private readonly AdServicesConfig   adServicesConfig;
        private readonly SignalBus          signalBus;
        private readonly ThirdPartiesConfig thirdPartiesConfig;
        private readonly ILogger            logger;
        private readonly IronSourceSettings ironSourceSettings;

        #endregion

        [Preserve]
        public IronSourceWrapper(IAnalyticServices analyticServices, AdServicesConfig adServicesConfig, SignalBus signalBus, ThirdPartiesConfig thirdPartiesConfig, ILoggerManager loggerManager)
        {
            this.analyticServices   = analyticServices;
            this.adServicesConfig   = adServicesConfig;
            this.signalBus          = signalBus;
            this.thirdPartiesConfig = thirdPartiesConfig;
            this.logger             = loggerManager.GetLogger(this);
            this.ironSourceSettings = this.thirdPartiesConfig.AdSettings.IronSource;
        }

        public string AdPlatform => AdRevenueConstants.ARSourceIronSource;

        private Action onRewardComplete;
        private Action onRewardFailed;

        private bool   isGotRewarded;
        private string interstitialPlacement, rewardedPlacement;

        private bool                    isLevelPlayInitialized;
        private CancellationTokenSource levelPlayInitializedCts;

        public void Initialize()
        {
            this.logger.Info("Initialize");
            IronSourceEvents.onImpressionDataReadyEvent        += this.ImpressionDataReadyEvent;
            IronSourceEvents.onSdkInitializationCompletedEvent += this.OnSdkInitializationCompleted;

            //Add AdInfo Rewarded Video Events
            IronSourceRewardedVideoEvents.onAdOpenedEvent      += this.RewardedVideoOnAdOpenedEvent;
            IronSourceRewardedVideoEvents.onAdClosedEvent      += this.RewardedVideoOnAdClosedEvent;
            IronSourceRewardedVideoEvents.onAdAvailableEvent   += this.RewardedVideoOnAdAvailable;
            IronSourceRewardedVideoEvents.onAdUnavailableEvent += this.RewardedVideoOnAdUnavailable;
            IronSourceRewardedVideoEvents.onAdShowFailedEvent  += this.RewardedVideoOnAdShowFailedEvent;
            IronSourceRewardedVideoEvents.onAdRewardedEvent    += this.RewardedVideoOnAdRewardedEvent;
            IronSourceRewardedVideoEvents.onAdClickedEvent     += this.RewardedVideoOnAdClickedEvent;

            //Add AdInfo Interstitial Events
            IronSourceInterstitialEvents.onAdReadyEvent      += this.InterstitialOnAdReadyEvent;
            IronSourceInterstitialEvents.onAdLoadFailedEvent += this.InterstitialOnAdLoadFailed;
            IronSourceInterstitialEvents.onAdOpenedEvent     += this.InterstitialOnAdOpenedEvent;
            IronSourceInterstitialEvents.onAdClickedEvent    += this.InterstitialOnAdClickedEvent;
            IronSourceInterstitialEvents.onAdShowFailedEvent += this.InterstitialOnAdShowFailedEvent;
            IronSourceInterstitialEvents.onAdClosedEvent     += this.InterstitialOnAdClosedEvent;

            #if THEONE_ADS_DEBUG
            IronSource.Agent.setMetaData("is_test_suite", "enable");
            #endif
            LevelPlay.SetPauseGame(true);
            IronSource.Agent.init(this.thirdPartiesConfig.AdSettings.IronSource.AppId);

            LevelPlay.OnInitSuccess += this.OnLevelPlayInitSuccess;
            LevelPlay.OnInitFailed  += this.OnLevelPlayInitFailed;
            this.InitLevelPlaySdk();
            this.InitMRECAds();

            #if THEONE_ADS_DEBUG
            IronSource.Agent.setAdaptersDebug(true);
            IronSource.Agent.validateIntegration();
            #endif
            this.InitAdQuality();
        }

        private void OnLevelPlayInitSuccess(LevelPlayConfiguration obj)
        {
            this.logger.Info($"IsAdQualityEnabled {obj.IsAdQualityEnabled}");
            this.isLevelPlayInitialized = true;
        }

        private void OnLevelPlayInitFailed(LevelPlayInitError obj)
        {
            this.logger.Info($"{obj}");
            this.InitLevelPlaySdk();
        }

        private void InitLevelPlaySdk()
        {
            this.logger.Info($"");
            LevelPlay.Init(this.thirdPartiesConfig.AdSettings.IronSource.AppId, adFormats: new[] { LevelPlayAdFormat.BANNER });
        }

        public void Dispose()
        {
            IronSourceEvents.onImpressionDataReadyEvent        -= this.ImpressionDataReadyEvent;
            IronSourceEvents.onSdkInitializationCompletedEvent -= this.OnSdkInitializationCompleted;

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
            IronSourceInterstitialEvents.onAdOpenedEvent     -= this.InterstitialOnAdOpenedEvent;
            IronSourceInterstitialEvents.onAdClickedEvent    -= this.InterstitialOnAdClickedEvent;
            IronSourceInterstitialEvents.onAdShowFailedEvent -= this.InterstitialOnAdShowFailedEvent;
            IronSourceInterstitialEvents.onAdClosedEvent     -= this.InterstitialOnAdClosedEvent;
        }

        private void OnSdkInitializationCompleted()
        {
            #if THEONE_ADS_DEBUG
            this.logger.Info($"IronSource Sdk initialized!");
            IronSource.Agent.launchTestSuite();
            #endif
        }

        #region Rewarded

        private void RewardedVideoOnAdRewardedEvent(IronSourcePlacement arg1, IronSourceAdInfo arg2)
        {
            this.isGotRewarded = true;
            this.onRewardComplete?.Invoke();
            this.onRewardComplete = null;
            var adInfo = new AdInfo(this.AdPlatform, arg2.adUnit, AdFormatConstants.Rewarded, arg2.adNetwork, value: arg2.revenue ?? 0, currency: "USD");
            this.signalBus.Fire(new RewardedAdCompletedSignal(this.rewardedPlacement, adInfo));
        }

        private Stopwatch rewardedStopwatch;

        private void RewardedVideoOnAdUnavailable()
        {
            this.logger.Info($"");
            this.rewardedStopwatch.Stop();
            this.signalBus.Fire(new RewardedAdLoadFailedSignal("", "", this.rewardedStopwatch.ElapsedMilliseconds));
        }

        private void RewardedVideoOnAdAvailable(IronSourceAdInfo arg1)
        {
            this.rewardedStopwatch.Stop();
            var adInfo = new AdInfo(this.AdPlatform, arg1.adUnit, AdFormatConstants.Rewarded, arg1.adNetwork, value: arg1.revenue ?? 0, currency: "USD");
            this.signalBus.Fire(new RewardedAdLoadedSignal("", this.rewardedStopwatch.ElapsedMilliseconds, adInfo));
        }

        private void RewardedVideoOnAdClosedEvent(IronSourceAdInfo obj)
        {
            var adInfo = new AdInfo(this.AdPlatform, obj.adUnit, AdFormatConstants.Rewarded, obj.adNetwork, value: obj.revenue ?? 0, currency: "USD");
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
            this.logger.Info($"{obj.getDescription()}");
            var adInfo = new AdInfo(this.AdPlatform, info.adUnit, AdFormatConstants.Rewarded, info.adNetwork, value: info.revenue ?? 0, currency: "USD");
            this.signalBus.Fire(new RewardedAdShowFailedSignal(this.rewardedPlacement, obj.getDescription(), adInfo));
        }

        private void RewardedVideoOnAdClickedEvent(IronSourcePlacement obj, IronSourceAdInfo info)
        {
            var adInfo = new AdInfo(this.AdPlatform, info.adUnit, AdFormatConstants.Rewarded, info.adNetwork, value: info.revenue ?? 0, currency: "USD");
            this.signalBus.Fire(new RewardedAdClickedSignal(this.rewardedPlacement, adInfo));
        }

        private void RewardedVideoOnAdOpenedEvent(IronSourceAdInfo info)
        {
            var adInfo = new AdInfo(this.AdPlatform, info.adUnit, AdFormatConstants.Rewarded, info.adNetwork, value: info.revenue ?? 0, currency: "USD");
            this.signalBus.Fire(new RewardedAdDisplayedSignal(this.rewardedPlacement, adInfo));
        }

        #endregion

        #region Interstitial

        private void InterstitialOnAdClosedEvent(IronSourceAdInfo obj)
        {
            var adInfo = new AdInfo(this.AdPlatform, obj.adUnit, AdFormatConstants.Interstitial, obj.adNetwork, value: obj.revenue ?? 0, currency: "USD");
            this.signalBus.Fire(new InterstitialAdClosedSignal(this.interstitialPlacement, adInfo));
        }

        private void InterstitialOnAdShowFailedEvent(IronSourceError arg1, IronSourceAdInfo arg2)
        {
            this.logger.Info($"{arg1.getDescription()}");
            this.signalBus.Fire(new InterstitialAdDisplayedFailedSignal(this.interstitialPlacement));
        }

        private Stopwatch stopwatchInterstitial;

        private void InterstitialOnAdLoadFailed(IronSourceError obj)
        {
            this.stopwatchInterstitial.Stop();
            this.logger.Info($"{obj.getDescription()}");
            this.signalBus.Fire(new InterstitialAdLoadFailedSignal("", obj.getDescription(), this.stopwatchInterstitial.ElapsedMilliseconds));
        }

        private void InterstitialOnAdReadyEvent(IronSourceAdInfo info)
        {
            this.stopwatchInterstitial.Stop();
            var adInfo = new AdInfo(this.AdPlatform, info.adUnit, AdFormatConstants.Interstitial, info.adNetwork, value: info.revenue ?? 0, currency: "USD");
            this.signalBus.Fire(new InterstitialAdLoadedSignal("", this.stopwatchInterstitial.ElapsedMilliseconds, adInfo));
        }

        private void InterstitialOnAdOpenedEvent(IronSourceAdInfo info)
        {
            var adInfo = new AdInfo(this.AdPlatform, info.adUnit, AdFormatConstants.Interstitial, info.adNetwork, value: info.revenue ?? 0, currency: "USD");
            this.signalBus.Fire(new InterstitialAdDisplayedSignal(this.interstitialPlacement, adInfo));
        }

        private void InterstitialOnAdClickedEvent(IronSourceAdInfo info)
        {
            var adInfo = new AdInfo(this.AdPlatform, info.adUnit, AdFormatConstants.Interstitial, info.adNetwork, value: info.revenue ?? 0, currency: "USD");
            this.signalBus.Fire(new InterstitialAdClickedSignal(this.interstitialPlacement, adInfo));
        }

        #endregion

        #region Banner

        private void OnBannerLoaded(LevelPlayAdInfo info)
        {
            this.logger.Info($"{info}");
            var adInfo = new AdInfo(this.AdPlatform, info.adUnitId, AdFormatConstants.Banner, info.adNetwork, value: info.revenue ?? 0, currency: "USD");
            this.signalBus.Fire(new BannerAdLoadedSignal("", adInfo));
            this.isLoadedBanner = true;
        }

        private void OnBannerLoadFailed(LevelPlayAdError info)
        {
            this.logger.Info($"{info}");
            this.signalBus.Fire(new BannerAdLoadFailedSignal("", $"{info}"));
        }

        private void OnBannerClicked(LevelPlayAdInfo info)
        {
            this.logger.Info($"{info}");
            var adInfo = new AdInfo(this.AdPlatform, info.adUnitId, AdFormatConstants.Banner, info.adNetwork, value: info.revenue ?? 0, currency: "USD");
            this.signalBus.Fire(new BannerAdClickedSignal("", adInfo));
        }

        private void BannerOnAdScreenDismissedEvent(LevelPlayAdInfo levelPlayAdInfo)
        {
            this.logger.Info($"");
            this.signalBus.Fire(new BannerAdDismissedSignal(""));
        }

        private void BannerOnAdScreenPresentedEvent(LevelPlayAdInfo levelPlayAdInfo)
        {
            this.logger.Info($"");
            this.signalBus.Fire(new BannerAdPresentedSignal(""));
        }

        #endregion

        #region MREC

        private readonly Dictionary<string, LevelPlayBannerAd> idToMRECAd     = new();
        private readonly List<string>                          mrecPlacements = new();

        private void InitMRECAds()
        {
            if (this.ironSourceSettings.MRECAdIds.Count == 0) return;
            foreach (var mrecAdId in this.ironSourceSettings.MRECAdIds)
            {
                this.mrecPlacements.Add(mrecAdId.Key.Name);
            }
        }

        public void ShowMREC(string placement, AdScreenPosition position, AdScreenPosition offset)
        {
            if (!this.isLevelPlayInitialized) return; //todo: handle wait to show when not initialized
            if (!this.idToMRECAd.TryGetValue(placement, out var mrecAd))
            {
                var mrecPosition = position == AdScreenPosition.BottomCenter ? LevelPlayBannerPosition.BottomCenter : LevelPlayBannerPosition.TopCenter;
                var adsId        = this.ironSourceSettings.MRECAdIds[AdPlacement.PlacementWithName(placement)].DefaultValue;
                mrecAd = new(adsId, LevelPlayAdSize.MEDIUM_RECTANGLE, mrecPosition, placement);
                this.idToMRECAd.Add(placement, mrecAd);

                mrecAd.OnAdLoaded     += this.OnMrecLoaded;
                mrecAd.OnAdLoadFailed += this.OnMrecLoadFailed;
                mrecAd.OnAdClicked    += this.OnMrecClicked;
                mrecAd.OnAdDisplayed  += this.OnMrecPresentedEvent;
                mrecAd.OnAdCollapsed  += this.OnMrecDismissedEvent;

                mrecAd.LoadAd();
            }

            mrecAd.ShowAd();
        }

        public bool IsMRECReady(string placement, AdScreenPosition position, AdScreenPosition offset)
        {
            return this.mrecPlacements.Contains(placement);
        }

        public void HideMREC(string placement)
        {
            if (!this.isLevelPlayInitialized) return;
            if (!this.idToMRECAd.TryGetValue(placement, out var mrecAd)) return;
            mrecAd.HideAd();
        }

        public void HideAllMREC()
        {
            foreach (var (_, mrecAd) in this.idToMRECAd)
            {
                mrecAd.HideAd();
            }
        }

        public void DestroyMREC(string placement)
        {
            if (!this.isLevelPlayInitialized) return;
            if (!this.idToMRECAd.TryGetValue(placement, out var mrecAd)) return;
            mrecAd.DestroyAd();
            this.idToMRECAd.Remove(placement);
        }

        private void OnMrecLoaded(LevelPlayAdInfo info)
        {
            this.logger.Info($"{info}");
            var revenue = info.revenue ?? 0;
            var adInfo  = new AdInfo(this.AdPlatform, info.adUnitId, AdFormatConstants.MREC, info.adNetwork, info.instanceName, revenue);
            this.signalBus.Fire(new MRecAdLoadedSignal(info.adUnitId, adInfo));
        }

        private void OnMrecLoadFailed(LevelPlayAdError info)
        {
            this.logger.Info($"{info}");
            this.signalBus.Fire(new MRecAdLoadFailedSignal(info.AdUnitId));
        }

        private void OnMrecClicked(LevelPlayAdInfo info)
        {
            this.logger.Info($"{info}");
            var revenue = info.revenue ?? 0;
            var ad      = new AdInfo(this.AdPlatform, info.adUnitId, AdFormatConstants.MREC, info.adNetwork, info.instanceName, revenue);
            this.signalBus.Fire(new MRecAdClickedSignal(info.adUnitId, ad));
        }

        private void OnMrecPresentedEvent(LevelPlayAdInfo info)
        {
            this.logger.Info($"{info}");
            var adInfo = new AdInfo(this.AdPlatform, info.adUnitId, AdFormatConstants.MREC, info.adNetwork, info.instanceName, info.revenue ?? 0);
            this.signalBus.Fire(new MRecAdDisplayedSignal(info.placementName, adInfo));
        }

        private void OnMrecDismissedEvent(LevelPlayAdInfo info)
        {
            this.logger.Info($"{info}");
            this.signalBus.Fire(new MRecAdDismissedSignal(info.placementName));
        }

        #endregion

        private void ImpressionDataReadyEvent(IronSourceImpressionData impressionData)
        {
            if (impressionData.revenue == null) return;

            var adsRevenueEvent = new AdsRevenueEvent()
            {
                AdsRevenueSourceId = AdRevenueConstants.ARSourceIronSource,
                AdUnit             = impressionData.adFormat,
                Revenue            = impressionData.revenue.Value,
                Currency           = "USD",
                Placement          = impressionData.placement,
                AdNetwork          = impressionData.adNetwork,
                AdFormat           = this.ConvertAdFormat(impressionData),
            };

            this.signalBus.Fire(new AdRevenueSignal(adsRevenueEvent));
            this.analyticServices.Track(adsRevenueEvent);
        }

        private string ConvertAdFormat(IronSourceImpressionData impressionData)
        {
            return impressionData.adFormat switch
            {
                "interstitial"   => AdFormatConstants.Interstitial,
                "rewarded_video" => AdFormatConstants.Rewarded,
                "banner"         => AdFormatConstants.Banner,
                _                => impressionData.adFormat,
            };
        }

        #region AdService

        //todo convert ads position
        private bool isLoadedBanner;

        private LevelPlayBannerAd bannerAd;

        public void ShowBannerAd(BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom, int width = 320, int height = 50)
        {
            this.logger.Info("");
            this.ResetLevelPlayInitializedCts();
            this.levelPlayInitializedCts = new();
            UniTask.WaitUntil(() => this.isLevelPlayInitialized, cancellationToken: this.levelPlayInitializedCts.Token).ContinueWith(InternalShowBannerAd).Forget();
            return;

            void InternalShowBannerAd()
            {
                if (this.isLoadedBanner)
                {
                    this.logger.Info("show banner loaded");
                    this.bannerAd.ShowAd();

                    return;
                }

                this.DestroyBannerAd(); // unlisten old banner

                var position = bannerAdsPosition switch
                {
                    BannerAdsPosition.Top => LevelPlayBannerPosition.TopCenter,
                    _                     => LevelPlayBannerPosition.BottomCenter
                };
                this.bannerAd = new(this.ironSourceSettings.BannerId.DefaultValue, this.BannerSize(), position);

                this.bannerAd.OnAdLoaded     += this.OnBannerLoaded;
                this.bannerAd.OnAdLoadFailed += this.OnBannerLoadFailed;
                this.bannerAd.OnAdClicked    += this.OnBannerClicked;
                this.bannerAd.OnAdDisplayed  += this.BannerOnAdScreenPresentedEvent;
                this.bannerAd.OnAdCollapsed  += this.BannerOnAdScreenDismissedEvent;

                this.bannerAd.LoadAd();
                this.bannerAd.ShowAd();
                this.logger.Info("show new banner");
            }
        }

        private void ResetLevelPlayInitializedCts()
        {
            this.levelPlayInitializedCts?.Cancel();
            this.levelPlayInitializedCts?.Dispose();
            this.levelPlayInitializedCts = null;
        }

        private LevelPlayAdSize BannerSize()
        {
            return this.thirdPartiesConfig.AdSettings.IronSource.IsAdaptiveBanner ? LevelPlayAdSize.CreateAdaptiveAdSize() : LevelPlayAdSize.BANNER;
        }

        public void HideBannedAd()
        {
            this.ResetLevelPlayInitializedCts();
            if (!this.isLevelPlayInitialized) return;
            this.bannerAd?.HideAd();
        }

        public void DestroyBannerAd()
        {
            this.isLoadedBanner = false;
            if (this.bannerAd == null) return;
            this.bannerAd.OnAdLoaded     -= this.OnBannerLoaded;
            this.bannerAd.OnAdLoadFailed -= this.OnBannerLoadFailed;
            this.bannerAd.OnAdClicked    -= this.OnBannerClicked;
            this.bannerAd.OnAdDisplayed  -= this.BannerOnAdScreenPresentedEvent;
            this.bannerAd.OnAdCollapsed  -= this.BannerOnAdScreenDismissedEvent;
            this.bannerAd.DestroyAd();
        }

        public bool IsInterstitialAdReady(string place)
        {
            return IronSource.Agent.isInterstitialReady();
        }

        public void ShowInterstitialAd(string place)
        {
            this.interstitialPlacement = place;
            IronSource.Agent.showInterstitial(place);
        }

        public AdNetworkSettings AdNetworkSettings => this.thirdPartiesConfig.AdSettings.IronSource;

        public bool IsRewardedAdReady(string place)
        {
            return IronSource.Agent.isRewardedVideoAvailable();
        }

        public void ShowRewardedAd(string place, Action onCompleted, Action onFailed)
        {
            this.rewardedPlacement = place;
            this.isGotRewarded     = false;
            IronSource.Agent.showRewardedVideo(place);
            this.onRewardComplete = onCompleted;
            this.onRewardFailed   = onFailed;
        }

        public bool IsAdsInitialized()
        {
            return true;
        }

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
            this.logger.Info("IronSourceAdQuality debug initialize");
            #elif IRONSOURCE_AD_QUALITY
            IronSourceAdQuality.Initialize(this.thirdPartiesConfig.AdSettings.IronSource.AppId);
            this.logger.Info("IronSourceAdQuality initialize");
            #endif
        }
    }
}
#endif