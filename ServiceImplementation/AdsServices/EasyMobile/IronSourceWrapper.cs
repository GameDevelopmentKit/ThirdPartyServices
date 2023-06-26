#if IRONSOURCE
namespace ServiceImplementation.AdsServices.EasyMobile
{
    using System;
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using Core.AnalyticServices.Signal;
    using ServiceImplementation.Configs;
    using UnityEngine;
    using Zenject;

    public class IronSourceWrapper : IMRECAdService, IAdServices, IInitializable, IDisposable
    {
        #region inject

        private readonly IAnalyticServices  analyticServices;
        private readonly AdServicesConfig   adServicesConfig;
        private readonly SignalBus          signalBus;
        private readonly ThirdPartiesConfig thirdPartiesConfig;

        #endregion
        

        public IronSourceWrapper(IAnalyticServices analyticServices,AdServicesConfig adServicesConfig, SignalBus signalBus, ThirdPartiesConfig thirdPartiesConfig)
        {
            this.analyticServices   = analyticServices;
            this.adServicesConfig   = adServicesConfig;
            this.signalBus          = signalBus;
            this.thirdPartiesConfig = thirdPartiesConfig;
        }

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
            IronSourceRewardedVideoEvents.onAdReadyEvent       += this.RewardedVideoOnAdReadyEvent;


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
            IronSourceRewardedVideoEvents.onAdReadyEvent       -= this.RewardedVideoOnAdReadyEvent;


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
            this.signalBus.Fire(new RewardedAdCompletedSignal(""));
        }
        private void RewardedVideoOnAdUnavailable()
        {
            
        }
        private void RewardedVideoOnAdAvailable(IronSourceAdInfo obj)
        {
            
        }
        private void RewardedVideoOnAdClosedEvent(IronSourceAdInfo obj)
        {
            this.signalBus.Fire(new RewardedSkippedSignal(""));
        }
        private void RewardedVideoOnAdShowFailedEvent(IronSourceError obj, IronSourceAdInfo info)
        {
            this.signalBus.Fire(new RewardedAdLoadFailedSignal("", obj.getDescription()));
        }

        private void RewardedVideoOnAdReadyEvent(IronSourceAdInfo info)
        {
            this.signalBus.Fire(new RewardedAdLoadedSignal(""));
        }

        private void RewardedVideoOnAdClickedEvent(IronSourcePlacement obj, IronSourceAdInfo info)
        {
            this.signalBus.Fire(new RewardedAdLoadClickedSignal(obj.getPlacementName()));
        }

        private void RewardedVideoOnAdOpenedEvent(IronSourceAdInfo info)
        {
            this.signalBus.Fire(new RewardedAdDisplayedSignal(""));
        }

        #endregion


        #region Interstitial

        private void InterstitialOnAdClosedEvent(IronSourceAdInfo obj)
        {
            this.signalBus.Fire(new InterstitialAdClosedSignal(""));
        }
        private void InterstitialOnAdShowFailedEvent(IronSourceError arg1, IronSourceAdInfo arg2)
        {
            this.signalBus.Fire(new InterstitialAdDisplayedFailedSignal(""));
        }
        private void InterstitialOnAdShowSucceededEvent(IronSourceAdInfo obj)
        {
            this.signalBus.Fire(new InterstitialAdDisplayedSignal(""));
        }
        
        private void InterstitialOnAdLoadFailed(IronSourceError obj)
        {
            this.signalBus.Fire(new InterstitialAdLoadFailedSignal("", obj.getDescription()));
        }

        private void InterstitialOnAdReadyEvent(IronSourceAdInfo info)
        {
            this.signalBus.Fire(new InterstitialAdDownloadedSignal(""));
        }

        private void InterstitialOnAdOpenedEvent(IronSourceAdInfo info)
        {
            this.signalBus.Fire(new InterstitialAdDisplayedSignal(""));
        }

        private void InterstitialOnAdClickedEvent(IronSourceAdInfo info)
        {
            this.signalBus.Fire(new InterstitialAdClickedSignal(""));
        }

        #endregion
        
       

        #region Banner

        private void BannerOnAdLoadFailedEvent(IronSourceError obj)
        {
            this.signalBus.Fire(new BannerAdLoadFailedSignal("", $"{obj.getDescription()}"));
        }
        private void BannerOnAdLeftApplicationEvent(IronSourceAdInfo obj)
        {
        }
        private void BannerOnAdScreenDismissedEvent(IronSourceAdInfo obj)
        {
            this.signalBus.Fire(new BannerAdDismissedSignal(""));
        }
        private void BannerOnAdScreenPresentedEvent(IronSourceAdInfo obj)
        {
            this.signalBus.Fire(new BannerAdPresentedSignal(""));
        }
        private void BannerOnAdLoadedEvent(IronSourceAdInfo info)
        {
            this.signalBus.Fire(new BannerAdLoadedSignal(""));
        }
        private void BannerOnAdClickedEvent(IronSourceAdInfo info)
        {
            this.signalBus.Fire(new BannerAdClickedSignal(""));
        }

        #endregion

        

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