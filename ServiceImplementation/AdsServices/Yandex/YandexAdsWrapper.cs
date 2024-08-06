#if YANDEX
namespace ServiceImplementation.AdsServices.Yandex
{
    using System;
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using Core.AnalyticServices;
    using GameFoundation.Scripts.Utilities.LogService;
    using ServiceImplementation.Configs;
    using ServiceImplementation.Configs.Ads;
    using UnityEngine;
    using YandexMobileAds;
    using YandexMobileAds.Base;
    using Zenject;

    public class YandexAdsWrapper : IMRECAdService, IAdServices, IInitializable, IDisposable, IAdLoadService, IAOAAdService
    {
        #region inject

        private readonly IAnalyticServices  analyticServices;
        private readonly AdServicesConfig   adServicesConfig;
        private readonly SignalBus          signalBus;
        private readonly ThirdPartiesConfig thirdPartiesConfig;
        private readonly ILogService        logService;

        public YandexAdsWrapper(IAnalyticServices analyticServices, AdServicesConfig adServicesConfig, SignalBus signalBus, ThirdPartiesConfig thirdPartiesConfig, ILogService logService)
        {
            this.analyticServices   = analyticServices;
            this.adServicesConfig   = adServicesConfig;
            this.signalBus          = signalBus;
            this.thirdPartiesConfig = thirdPartiesConfig;
            this.logService         = logService;
        }

        #endregion

        public AdNetworkSettings AdNetworkSettings => this.thirdPartiesConfig.AdSettings.Yandex;
        public YandexSettings    YandexSettings    => this.thirdPartiesConfig.AdSettings.Yandex;

        public bool IsAdsInitialized() => true;

        public bool IsShowingAoaAd { get; set; }

        private AppOpenAdLoader appOpenAdLoader;
        private AppOpenAd       appOpenAd;

        public void Initialize()
        {
            MobileAds.SetAgeRestrictedUser(true);
            this.InitAoaAd();
        }

        public void Dispose() { throw new NotImplementedException(); }

        #region MREC

        public void ShowMREC(AdViewPosition adViewPosition) { throw new NotImplementedException(); }

        public void HideMREC(AdViewPosition adViewPosition) { throw new NotImplementedException(); }

        public void StopMRECAutoRefresh(AdViewPosition adViewPosition) { throw new NotImplementedException(); }

        public void StartMRECAutoRefresh(AdViewPosition adViewPosition) { throw new NotImplementedException(); }

        public void LoadMREC(AdViewPosition adViewPosition) { throw new NotImplementedException(); }

        public bool IsMRECReady(AdViewPosition adViewPosition) { throw new NotImplementedException(); }

        public void HideAllMREC() { throw new NotImplementedException(); }

        #endregion

        #region Banner

        public void ShowBannerAd(BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom, int width = 320, int height = 50) { throw new NotImplementedException(); }

        public void HideBannedAd() { throw new NotImplementedException(); }

        public void DestroyBannerAd() { throw new NotImplementedException(); }

        #endregion

        #region Rewarded

        bool IAdLoadService.IsRewardedAdReady(string place) { throw new NotImplementedException(); }

        public void LoadRewardAds(string place = "") { throw new NotImplementedException(); }

        bool IAdServices.IsRewardedAdReady(string place) { throw new NotImplementedException(); }

        public void ShowRewardedAd(string place, Action onCompleted, Action onFailed) { throw new NotImplementedException(); }

        #endregion

        #region Interstitial

        bool IAdLoadService.IsInterstitialAdReady(string place) { throw new NotImplementedException(); }

        public void LoadInterstitialAd(string place = "") { throw new NotImplementedException(); }

        bool IAdServices.IsInterstitialAdReady(string place) { throw new NotImplementedException(); }

        public void ShowInterstitialAd(string place) { throw new NotImplementedException(); }

        #endregion

        #region AOA

        private void InitAoaAd()
        {
            this.appOpenAdLoader                  =  new AppOpenAdLoader();
            this.appOpenAdLoader.OnAdLoaded       += this.HandleAoaAdLoaded;
            this.appOpenAdLoader.OnAdFailedToLoad += this.HandleAoaAdFailedToLoad;

            this.LoadAoaAd();
        }

        public void LoadAoaAd()
        {
            if (this.IsRemoveAds()) return;

            this.appOpenAdLoader.LoadAd(new AdRequestConfiguration.Builder(this.YandexSettings.AoaAdId.Id).Build());
        }

        public void DestroyAoaAd()
        {
            this.appOpenAd?.Destroy();
            this.appOpenAd = null;
        }

        public bool IsAOAReady() => this.appOpenAd != null && !this.IsShowingAoaAd;

        public void ShowAOAAds()
        {
            if (this.IsRemoveAds()) return;

            this.appOpenAd?.Show();
        }

        public void HandleAoaAdLoaded(object sender, AppOpenAdLoadedEventArgs args)
        {
            this.logService.Log("onelog: Yandex Aoa: HandleAdLoaded event received");
            this.appOpenAd = args.AppOpenAd;

            this.appOpenAd.OnAdClicked      += this.HandleAdClicked;
            this.appOpenAd.OnAdShown        += this.HandleAdShown;
            this.appOpenAd.OnAdFailedToShow += this.HandleAdFailedToShow;
            this.appOpenAd.OnAdDismissed    += this.HandleAdDismissed;
            this.appOpenAd.OnAdImpression   += this.HandleImpression;

            this.signalBus.Fire(new AppOpenLoadedSignal(""));
        }

        public void HandleAoaAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        {
            this.logService.Log("onelog: Yandex Aoa: HandleAdFailedToLoad event received with message: " + args.Message);
            this.signalBus.Fire(new AppOpenLoadFailedSignal(""));
        }

        public void HandleAdClicked(object sender, EventArgs args)
        {
            this.logService.Log("onelog: Yandex Aoa: HandleAdClicked event received");
            this.signalBus.Fire(new AppOpenClickedSignal(""));
        }

        public void HandleAdShown(object sender, EventArgs args)
        {
            this.logService.Log("onelog: Yandex Aoa: Displayed app open ad");
            this.signalBus.Fire(new AppOpenFullScreenContentOpenedSignal(""));
            this.IsShowingAoaAd = true;
        }

        public void HandleAdDismissed(object sender, EventArgs args)
        {
            this.signalBus.Fire(new AppOpenFullScreenContentClosedSignal(""));
            this.logService.Log("onelog: Yandex Aoa: HandleAdDismissed event received");
            this.DestroyAoaAd();
            this.LoadAoaAd();
            this.IsShowingAoaAd = false;
        }

        public void HandleAdFailedToShow(object sender, AdFailureEventArgs args)
        {
            this.signalBus.Fire(new AppOpenFullScreenContentFailedSignal(""));
            this.logService.Log($"onelog: Yandex Aoa: HandleAdFailedToShow event received with message: {args.Message}");
            this.DestroyAoaAd();
            this.LoadAoaAd();
        }

        #endregion

        public void HandleImpression(object sender, ImpressionData impressionData)
        {
            var data = impressionData == null ? "null" : impressionData.rawData;
            this.logService.Log($"onelog: Yandex Aoa: HandleImpression event received with data: {data}");
            // this.HandlePaidEvent(impressionData, "AppOpenAd");
        }

        #region Remove Ads

        public void RemoveAds(bool revokeConsent = false) { PlayerPrefs.SetInt("EM_REMOVE_ADS", -1); }

        public bool IsRemoveAds() => PlayerPrefs.HasKey("EM_REMOVE_ADS");

        #endregion

        #region Ads Revenue

        private void HandlePaidEvent(ImpressionData data, string adFormat)
        {
            this.logService.Log($"HandleImpression event received with data: {data}");
            // var adsRevenueEvent = new AdsRevenueEvent()
            // {
            //     AdsRevenueSourceId = AdRevenueConstants.ARSourceAdMob,
            //     Revenue            = args.Value / 1e6,
            //     Currency           = "USD",
            //     Placement          = adFormat,
            //     AdNetwork          = "AdMob",
            //     AdFormat           = adFormat,
            //     AdUnit             = adFormat
            // };
            //
            // this.analyticService.Track(adsRevenueEvent);
            // this.signalBus.Fire(new AdRevenueSignal(adsRevenueEvent));
        }

        #endregion
    }
}
#endif