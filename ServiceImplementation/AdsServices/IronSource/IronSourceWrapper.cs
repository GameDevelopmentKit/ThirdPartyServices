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
            Debug.Log($"oneLog: IronSourceWrapper RewardedVideoOnAdRewardedEvent, " +
                      $"Reward name: {arg1.getRewardName()}, Placement name: {arg1.getPlacementName()}, Reward amount: {arg1.getRewardAmount()}," +
                      $"Ad unit: {arg2.adUnit}, Ad network: {arg2.adNetwork}, Country: {arg2.country}, Segment name: {arg2.segmentName}, " +
                      $"Revenue: {arg2.revenue}, Precision: {arg2.precision}, Lifetime revenue: {arg2.lifetimeRevenue}, Encrypted CPM: {arg2.encryptedCPM}, " +
                      $"Instance name: {arg2.instanceName}, Instance ID: {arg2.instanceId}, Ab: {arg2.ab}, Auction ID: {arg2.auctionId}");
            // var adInfo = new AdInfo(AdPlatform, arg2.adUnit, arg2.adUnit, arg2.adNetwork, arg1.);
            this.signalBus.Fire(new RewardedAdCompletedSignal(this.rewardedPlacement, null));
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
            Debug.Log($"oneLog: IronSourceWrapper RewardedVideoOnAdAvailable, " +
                      $"Ad unit: {obj.adUnit}, Ad network: {obj.adNetwork}, Country: {obj.country}, Segment name: {obj.segmentName}, " +
                      $"Revenue: {obj.revenue}, Precision: {obj.precision}, Lifetime revenue: {obj.lifetimeRevenue}, Encrypted CPM: {obj.encryptedCPM}, " +
                      $"Instance name: {obj.instanceName}, Instance ID: {obj.instanceId}, Ab: {obj.ab}, Auction ID: {obj.auctionId}");
            
            this.signalBus.Fire(new RewardedAdLoadedSignal("", this.rewardedStopwatch.ElapsedMilliseconds, null));
        }

        private void RewardedVideoOnAdClosedEvent(IronSourceAdInfo obj)
        {
            Debug.Log($"oneLog: IronSourceWrapper RewardedVideoOnAdClosedEvent, " +
                      $"Ad unit: {obj.adUnit}, Ad network: {obj.adNetwork}, Country: {obj.country}, Segment name: {obj.segmentName}, " +
                      $"Revenue: {obj.revenue}, Precision: {obj.precision}, Lifetime revenue: {obj.lifetimeRevenue}, Encrypted CPM: {obj.encryptedCPM}, " +
                      $"Instance name: {obj.instanceName}, Instance ID: {obj.instanceId}, Ab: {obj.ab}, Auction ID: {obj.auctionId}");
            if (!this.isGotRewarded)
            {
                this.onRewardFailed?.Invoke();
                this.onRewardFailed = null;
                this.signalBus.Fire(new RewardedSkippedSignal(this.rewardedPlacement, null));
            }
            this.signalBus.Fire(new RewardedAdClosedSignal(this.rewardedPlacement, null));
        }

        private void RewardedVideoOnAdShowFailedEvent(IronSourceError obj, IronSourceAdInfo info)
        {
            this.onRewardFailed?.Invoke();
            this.onRewardFailed = null;
            this.signalBus.Fire(new RewardedAdDisplayFailedSignal(this.rewardedPlacement, obj.getDescription(),null));
        }

        private void RewardedVideoOnAdClickedEvent(IronSourcePlacement obj, IronSourceAdInfo info)
        {
            Debug.Log($"oneLog: IronSourceWrapper RewardedVideoOnAdClickedEvent, " +
                      $"Ad unit: {info.adUnit}, Ad network: {info.adNetwork}, Country: {info.country}, Segment name: {info.segmentName}, " +
                      $"Revenue: {info.revenue}, Precision: {info.precision}, Lifetime revenue: {info.lifetimeRevenue}, Encrypted CPM: {info.encryptedCPM}, " +
                      $"Instance name: {info.instanceName}, Instance ID: {info.instanceId}, Ab: {info.ab}, Auction ID: {info.auctionId}");
            this.signalBus.Fire(new RewardedAdClickedSignal(this.rewardedPlacement, null));
        }

        private void RewardedVideoOnAdOpenedEvent(IronSourceAdInfo info)
        {
            Debug.Log($"oneLog: IronSourceWrapper RewardedVideoOnAdOpenedEvent, " +
                      $"Ad unit: {info.adUnit}, Ad network: {info.adNetwork}, Country: {info.country}, Segment name: {info.segmentName}, " +
                      $"Revenue: {info.revenue}, Precision: {info.precision}, Lifetime revenue: {info.lifetimeRevenue}, Encrypted CPM: {info.encryptedCPM}, " +
                      $"Instance name: {info.instanceName}, Instance ID: {info.instanceId}, Ab: {info.ab}, Auction ID: {info.auctionId}");
            this.signalBus.Fire(new RewardedAdDisplayedSignal(this.rewardedPlacement, null));
        }

        #endregion


        #region Interstitial

        private void InterstitialOnAdClosedEvent(IronSourceAdInfo obj)
        {
            Debug.Log($"oneLog: IronSourceWrapper InterstitialOnAdClosedEvent, " +
                      $"Ad unit: {obj.adUnit}, Ad network: {obj.adNetwork}, Country: {obj.country}, Segment name: {obj.segmentName}, " +
                      $"Revenue: {obj.revenue}, Precision: {obj.precision}, Lifetime revenue: {obj.lifetimeRevenue}, Encrypted CPM: {obj.encryptedCPM}, " +
                      $"Instance name: {obj.instanceName}, Instance ID: {obj.instanceId}, Ab: {obj.ab}, Auction ID: {obj.auctionId}");
            this.signalBus.Fire(new InterstitialAdClosedSignal(this.interstitialPlacement, null));
        }
        private void InterstitialOnAdShowFailedEvent(IronSourceError arg1, IronSourceAdInfo arg2) { this.signalBus.Fire(new InterstitialAdDisplayedFailedSignal(this.interstitialPlacement)); }
        private void InterstitialOnAdShowSucceededEvent(IronSourceAdInfo obj)                     { }

        private Stopwatch stopwatchInterstitial;
        private void InterstitialOnAdLoadFailed(IronSourceError obj)
        {
            this.stopwatchInterstitial.Stop();
            this.signalBus.Fire(new InterstitialAdLoadFailedSignal("", obj.getDescription(), this.stopwatchInterstitial.ElapsedMilliseconds));
        }

        private void InterstitialOnAdReadyEvent(IronSourceAdInfo info)
        {
            this.stopwatchInterstitial.Stop();
            Debug.Log($"oneLog: IronSourceWrapper InterstitialOnAdReadyEvent, " +
                      $"Ad unit: {info.adUnit}, Ad network: {info.adNetwork}, Country: {info.country}, Segment name: {info.segmentName}, " +
                      $"Revenue: {info.revenue}, Precision: {info.precision}, Lifetime revenue: {info.lifetimeRevenue}, Encrypted CPM: {info.encryptedCPM}, " +
                      $"Instance name: {info.instanceName}, Instance ID: {info.instanceId}, Ab: {info.ab}, Auction ID: {info.auctionId}");
            this.signalBus.Fire(new InterstitialAdLoadedSignal("",  this.stopwatchInterstitial.ElapsedMilliseconds, null));
        }

        private void InterstitialOnAdOpenedEvent(IronSourceAdInfo info)
        {
            Debug.Log($"oneLog: IronSourceWrapper InterstitialOnAdOpenedEvent, " +
                      $"Ad unit: {info.adUnit}, Ad network: {info.adNetwork}, Country: {info.country}, Segment name: {info.segmentName}, " +
                      $"Revenue: {info.revenue}, Precision: {info.precision}, Lifetime revenue: {info.lifetimeRevenue}, Encrypted CPM: {info.encryptedCPM}, " +
                      $"Instance name: {info.instanceName}, Instance ID: {info.instanceId}, Ab: {info.ab}, Auction ID: {info.auctionId}");
            this.signalBus.Fire(new InterstitialAdDisplayedSignal(this.interstitialPlacement, null));
        }

        private void InterstitialOnAdClickedEvent(IronSourceAdInfo info)
        {
            Debug.Log($"oneLog: IronSourceWrapper InterstitialOnAdClickedEvent, " +
                      $"Ad unit: {info.adUnit}, Ad network: {info.adNetwork}, Country: {info.country}, Segment name: {info.segmentName}, " +
                      $"Revenue: {info.revenue}, Precision: {info.precision}, Lifetime revenue: {info.lifetimeRevenue}, Encrypted CPM: {info.encryptedCPM}, " +
                      $"Instance name: {info.instanceName}, Instance ID: {info.instanceId}, Ab: {info.ab}, Auction ID: {info.auctionId}");
            this.signalBus.Fire(new InterstitialAdClickedSignal(this.interstitialPlacement, null));
        }

        #endregion


        #region Banner

        private async void BannerOnAdLoadFailedEvent(IronSourceError obj)
        {
            this.signalBus.Fire(new BannerAdLoadFailedSignal("", $"{obj.getDescription()}"));
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            this.ShowBannerAd();
        }

        private void BannerOnAdLeftApplicationEvent(IronSourceAdInfo obj) { }

        private void BannerOnAdScreenDismissedEvent(IronSourceAdInfo obj)
        {
            Debug.Log($"oneLog: IronSourceWrapper BannerOnAdScreenDismissedEvent, " +
                      $"Ad unit: {obj.adUnit}, Ad network: {obj.adNetwork}, Country: {obj.country}, Segment name: {obj.segmentName}, " +
                      $"Revenue: {obj.revenue}, Precision: {obj.precision}, Lifetime revenue: {obj.lifetimeRevenue}, Encrypted CPM: {obj.encryptedCPM}, " +
                      $"Instance name: {obj.instanceName}, Instance ID: {obj.instanceId}, Ab: {obj.ab}, Auction ID: {obj.auctionId}");
            this.signalBus.Fire(new BannerAdDismissedSignal("", null));
        }

        private void BannerOnAdScreenPresentedEvent(IronSourceAdInfo obj)
        {
            Debug.Log($"oneLog: IronSourceWrapper BannerOnAdScreenPresentedEvent, " +
                      $"Ad unit: {obj.adUnit}, Ad network: {obj.adNetwork}, Country: {obj.country}, Segment name: {obj.segmentName}, " +
                      $"Revenue: {obj.revenue}, Precision: {obj.precision}, Lifetime revenue: {obj.lifetimeRevenue}, Encrypted CPM: {obj.encryptedCPM}, " +
                      $"Instance name: {obj.instanceName}, Instance ID: {obj.instanceId}, Ab: {obj.ab}, Auction ID: {obj.auctionId}");
            this.signalBus.Fire(new BannerAdPresentedSignal("", null));
        }

        private void BannerOnAdLoadedEvent(IronSourceAdInfo info)
        {
            Debug.Log($"oneLog: IronSourceWrapper BannerOnAdLoadedEvent, " +
                      $"Ad unit: {info.adUnit}, Ad network: {info.adNetwork}, Country: {info.country}, Segment name: {info.segmentName}, " +
                      $"Revenue: {info.revenue}, Precision: {info.precision}, Lifetime revenue: {info.lifetimeRevenue}, Encrypted CPM: {info.encryptedCPM}, " +
                      $"Instance name: {info.instanceName}, Instance ID: {info.instanceId}, Ab: {info.ab}, Auction ID: {info.auctionId}");
            this.signalBus.Fire(new BannerAdLoadedSignal("", null));
            this.isLoadedBanner = true;
        }

        private void BannerOnAdClickedEvent(IronSourceAdInfo info)
        {
            Debug.Log($"oneLog: IronSourceWrapper BannerOnAdClickedEvent, " +
                      $"Ad unit: {info.adUnit}, Ad network: {info.adNetwork}, Country: {info.country}, Segment name: {info.segmentName}, " +
                      $"Revenue: {info.revenue}, Precision: {info.precision}, Lifetime revenue: {info.lifetimeRevenue}, Encrypted CPM: {info.encryptedCPM}, " +
                      $"Instance name: {info.instanceName}, Instance ID: {info.instanceId}, Ab: {info.ab}, Auction ID: {info.auctionId}");
            this.signalBus.Fire(new BannerAdClickedSignal("", null));
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
                AdFormat           = impressionData.adUnit
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
            id = "";
            return true;
        }

        public void LoadInterstitialAd(string place)
        {
            this.stopwatchInterstitial = Stopwatch.StartNew();
            IronSource.Agent.loadInterstitial();
        }

        public bool TryGetInterstitialPlacementId(string placement, out string id)
        {
            id = "";
            return true;
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