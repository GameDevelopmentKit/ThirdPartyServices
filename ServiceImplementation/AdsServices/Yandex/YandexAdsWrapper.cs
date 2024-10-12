#if YANDEX
namespace ServiceImplementation.AdsServices.Yandex
{
    using System;
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using Core.AnalyticServices.Signal;
    using GameFoundation.DI;
    using GameFoundation.Scripts.Utilities.LogService;
    using Newtonsoft.Json;
    using ServiceImplementation.Configs;
    using ServiceImplementation.Configs.Ads;
    using ServiceImplementation.Configs.Ads.Yandex;
    using UnityEngine;
    using YandexMobileAds;
    using YandexMobileAds.Base;
    using GameFoundation.Signals;
    using AdInfo = Core.AdsServices.AdInfo;
    using UnityEngine.Scripting;

    public class YandexAdsWrapper : IAdServices, IInitializable, IAdLoadService, IAOAAdService
    {
        #region Inject

        private readonly IAnalyticServices  analyticServices;
        private readonly AdServicesConfig   adServicesConfig;
        private readonly SignalBus          signalBus;
        private readonly ThirdPartiesConfig thirdPartiesConfig;
        private readonly ILogService        logService;

        [Preserve]
        public YandexAdsWrapper(IAnalyticServices analyticServices, AdServicesConfig adServicesConfig, SignalBus signalBus, ThirdPartiesConfig thirdPartiesConfig, ILogService logService)
        {
            this.analyticServices = analyticServices;
            this.adServicesConfig = adServicesConfig;
            this.signalBus = signalBus;
            this.thirdPartiesConfig = thirdPartiesConfig;
            this.logService = logService;
        }

        #endregion

        #region Variables

        public  AdNetworkSettings AdNetworkSettings => this.thirdPartiesConfig.AdSettings.Yandex;
        private YandexSettings    YandexSettings    => this.thirdPartiesConfig.AdSettings.Yandex;

        public bool IsAdsInitialized() => true;

        private bool   IsShowingAoaAd                 { get; set; }
        private bool   IsBannerAdLoaded               { get; set; }
        private string CurrentInterstitialAdPlacement { get; set; }
        private string CurrentRewardedAdPlacement     { get; set; }
        private Action OnRewardedAdCompleted          { get; set; }
        private Action OnRewardedAdFailed             { get; set; }
        private bool   IsRewardedAdReward             { get; set; }
        public string AdPlatform => AdRevenueConstants.ARSourceYandex;

        private Banner               banner;
        private AppOpenAdLoader      appOpenAdLoader;
        private AppOpenAd            appOpenAd;
        private InterstitialAdLoader interstitialAdLoader;
        private Interstitial         interstitialAd;
        private RewardedAdLoader     rewardedAdLoader;
        private RewardedAd           rewardedAd;

        #endregion


        public void Initialize()
        {
            MobileAds.SetAgeRestrictedUser(true);
            this.InitInterstitialAd();
            this.InitRewardedAd();
            this.InitAoaAd();

#if THEONE_ADS_DEBUG
            MobileAds.ShowDebugPanel();
#endif

            this.logService.Log($"onelog: Yandex: Initialize SDK");
        }

        #region Banner

        private static int GetScreenWidthDp() => ScreenUtils.ConvertPixelsToDp((int)Screen.safeArea.width);

        private void LoadNewBanner(BannerAdSize bannerSize)
        {
            this.IsBannerAdLoaded = false;

            this.banner = new Banner(this.YandexSettings.BannerAdId.Id, bannerSize, AdPosition.BottomCenter);

            this.banner.OnAdLoaded += this.HandleBannerAdLoaded;
            this.banner.OnAdFailedToLoad += this.HandleBannerAdFailedToLoad;
            this.banner.OnAdClicked += this.HandleBannerAdClicked;
            this.banner.OnImpression += this.HandleImpression;

            this.banner.LoadAd(new AdRequest.Builder().Build());
        }

        #region Events

        private void HandleBannerAdLoaded(object sender, EventArgs args)
        {
            this.logService.Log($"onelog: Yandex: HandleBannerAdLoaded");
            var adInfo = new AdInfo(this.AdPlatform, this.YandexSettings.BannerAdId.Id, AdFormatConstants.Banner, AdFormatConstants.Banner);
            this.signalBus.Fire(new BannerAdLoadedSignal("", adInfo));
            this.IsBannerAdLoaded = true;
        }

        private void HandleBannerAdFailedToLoad(object sender, AdFailureEventArgs args)
        {
            this.logService.Log($"onelog: Yandex: HandleBannerAdFailedToLoad {args.Message}");

            this.signalBus.Fire(new BannerAdLoadFailedSignal("", args.Message));
        }

        private void HandleBannerAdClicked(object sender, EventArgs args)
        {
            this.logService.Log($"onelog: Yandex: HandleBannerAdClicked");
            var adInfo = new AdInfo(this.AdPlatform, this.YandexSettings.BannerAdId.Id, AdFormatConstants.Banner, AdFormatConstants.Banner);
            this.signalBus.Fire(new BannerAdClickedSignal("", adInfo));
        }

        #endregion

        #region Public


        public void ShowBannerAd(BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom, int width = 320, int height = 50)
        {
            if (this.IsBannerAdLoaded && this.banner != null)
            {
                this.banner.Show();
            }
            else
            {
                this.DestroyBannerAd();
                this.LoadNewBanner(BannerAdSize.StickySize(GetScreenWidthDp()));
            }
        }

        public void HideBannedAd() => this.banner?.Hide();

        public void DestroyBannerAd() => this.banner?.Destroy();

        #endregion

        #endregion

        #region Rewarded

        private void InitRewardedAd()
        {
            this.rewardedAdLoader = new RewardedAdLoader();
            this.rewardedAdLoader.OnAdLoaded += this.HandleRewardedAdLoaded;
            this.rewardedAdLoader.OnAdFailedToLoad += this.HandleRewardedAdFailedToLoad;

            this.LoadRewardAds();
        }

        private void DestroyRewardedAd()
        {
            this.rewardedAd?.Destroy();
            this.rewardedAd = null;
        }

        #region Events

        private void HandleRewardedAdLoaded(object sender, RewardedAdLoadedEventArgs args)
        {
            this.rewardedAd = args.RewardedAd;

            this.rewardedAd.OnAdClicked += this.HandleRewardedAdClicked;
            this.rewardedAd.OnAdShown += this.HandleRewardedAdShown;
            this.rewardedAd.OnAdFailedToShow += this.HandleRewardedAdFailedToShow;
            this.rewardedAd.OnAdImpression += this.HandleImpression;
            this.rewardedAd.OnAdDismissed += this.HandleRewardedAdDismissed;
            this.rewardedAd.OnRewarded += this.HandleRewardedAdReward;
        }

        private void HandleRewardedAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        {
            this.logService.Log($"onelog: Yandex: HandleRewardedAdFailedToLoad: {args.Message}");
            this.signalBus.Fire(new RewardedAdLoadFailedSignal("", args.Message, 0));
        }

        private void HandleRewardedAdDismissed(object sender, EventArgs args)
        {
            this.logService.Log($"onelog: Yandex: HandleRewardedAdDismissed");
            if (this.IsRewardedAdReward)
            {
                this.OnRewardedAdCompleted?.Invoke();
            }
            else
            {
                this.OnRewardedAdFailed?.Invoke();
            }
            var adInfo = new AdInfo(this.AdPlatform, this.YandexSettings.RewardedAdId.Id, AdFormatConstants.Rewarded, AdFormatConstants.Rewarded);
            this.signalBus.Fire(new RewardedAdClosedSignal(this.CurrentRewardedAdPlacement, adInfo));

            this.DestroyRewardedAd();
            this.LoadRewardAds();
        }

        private void HandleRewardedAdFailedToShow(object sender, AdFailureEventArgs args)
        {
            this.logService.Log($"onelog: Yandex: HandleRewardedAdFailedToShow: {args.Message}");
            var adInfo = new AdInfo(this.AdPlatform, this.YandexSettings.RewardedAdId.Id, AdFormatConstants.Rewarded, AdFormatConstants.Rewarded);
            this.signalBus.Fire(new RewardedAdShowFailedSignal(this.CurrentRewardedAdPlacement, args.Message, adInfo));
            this.DestroyRewardedAd();
            this.LoadRewardAds();
        }

        private void HandleRewardedAdClicked(object sender, EventArgs args)
        {
            this.logService.Log($"onelog: Yandex: HandleRewardedAdClicked");
            var adInfo = new AdInfo(this.AdPlatform, this.YandexSettings.RewardedAdId.Id, AdFormatConstants.Rewarded, AdFormatConstants.Rewarded);
            this.signalBus.Fire(new RewardedAdClickedSignal(this.CurrentRewardedAdPlacement, adInfo));
        }

        private void HandleRewardedAdShown(object sender, EventArgs args)
        {
            this.logService.Log($"onelog: Yandex: HandleRewardedAdShown");
            var adInfo = new AdInfo(this.AdPlatform, this.YandexSettings.RewardedAdId.Id, AdFormatConstants.Rewarded, AdFormatConstants.Rewarded);
            this.signalBus.Fire(new RewardedAdDisplayedSignal(this.CurrentRewardedAdPlacement, adInfo));
        }

        private void HandleRewardedAdReward(object sender, Reward args)
        {
            this.logService.Log($"onelog: Yandex: HandleRewardedAdReward");
            this.IsRewardedAdReward = true;
        }

        #endregion

        #region Public

        public bool IsRewardedAdReady(string place) => !string.IsNullOrEmpty(place) && this.rewardedAd != null;

        public void LoadRewardAds(string place = "")
        {
            if (string.IsNullOrEmpty(place)) return;
            this.rewardedAdLoader.LoadAd(new AdRequestConfiguration.Builder(this.YandexSettings.RewardedAdId.Id).Build());
        }

        public bool TryGetRewardPlacementId(string placement, out string id)
        {
            id = default;
            return false;
        }

        public void ShowRewardedAd(string place, Action onCompleted, Action onFailed)
        {
            this.IsRewardedAdReward = false;
            this.CurrentRewardedAdPlacement = place;
            this.OnRewardedAdCompleted = onCompleted;
            this.OnRewardedAdFailed = onFailed;
            this.rewardedAd?.Show();
        }

        #endregion

        #endregion

        #region Interstitial

        private void InitInterstitialAd()
        {
            this.interstitialAdLoader = new InterstitialAdLoader();
            this.interstitialAdLoader.OnAdLoaded += this.HandleInterstitialLoaded;
            this.interstitialAdLoader.OnAdFailedToLoad += this.HandleInterstitialFailedToLoad;

            this.LoadInterstitialAd();
        }

        private void DestroyInterstitial()
        {
            this.interstitialAd?.Destroy();
            this.interstitialAd = null;
        }

        #region Events

        private void HandleInterstitialLoaded(object sender, InterstitialAdLoadedEventArgs args)
        {
            this.logService.Log($"onelog: Yandex: HandleInterstitialLoaded, AdUnitId: {args.Interstitial.GetInfo().AdUnitId}");
            this.interstitialAd = args.Interstitial;

            this.interstitialAd.OnAdClicked += this.HandleInterstitialAdClicked;
            this.interstitialAd.OnAdShown += this.HandleInterstitialShown;
            this.interstitialAd.OnAdFailedToShow += this.HandleInterstitialFailedToShow;
            this.interstitialAd.OnAdDismissed += this.HandleInterstitialDismissed;
            this.interstitialAd.OnAdImpression += this.HandleImpression;
            var adInfo = new AdInfo(this.AdPlatform, this.YandexSettings.InterstitialAdId.Id, AdFormatConstants.Interstitial);
            this.signalBus.Fire(new InterstitialAdLoadedSignal("", 0,adInfo));
        }

        private void HandleInterstitialFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        {
            this.logService.Log($"onelog: Yandex: HandleInterstitialFailedToLoad {args.Message}");

            this.signalBus.Fire(new InterstitialAdLoadFailedSignal("", args.Message, 0));
        }

        private void HandleInterstitialDismissed(object sender, EventArgs args)
        {
            this.logService.Log($"onelog: Yandex: HandleInterstitialDismissed");
            var adInfo = new AdInfo(this.AdPlatform, this.YandexSettings.InterstitialAdId.Id, AdFormatConstants.Interstitial);
            this.signalBus.Fire(new InterstitialAdClosedSignal(this.CurrentInterstitialAdPlacement, adInfo));
            this.DestroyInterstitial();
            this.LoadInterstitialAd();
        }

        private void HandleInterstitialFailedToShow(object sender, EventArgs args)
        {
            this.logService.Log($"onelog: Yandex: HandleInterstitialFailedToShow");
            this.DestroyInterstitial();
            this.LoadInterstitialAd();
        }

        private void HandleInterstitialAdClicked(object sender, EventArgs args)
        {
            this.logService.Log($"onelog: Yandex: HandleInterstitialAdClicked");
            var adInfo = new AdInfo(this.AdPlatform, this.YandexSettings.InterstitialAdId.Id, AdFormatConstants.Interstitial);
            this.signalBus.Fire(new InterstitialAdClickedSignal(this.CurrentInterstitialAdPlacement, adInfo));
        }

        private void HandleInterstitialShown(object sender, EventArgs args)
        {
            this.logService.Log($"onelog: Yandex: HandleInterstitialShown");
            var adInfo = new AdInfo(this.AdPlatform, this.YandexSettings.InterstitialAdId.Id, AdFormatConstants.Interstitial);
            this.signalBus.Fire(new InterstitialAdDisplayedSignal(this.CurrentInterstitialAdPlacement, adInfo));
        }

        #endregion

        #region Public

        public bool IsInterstitialAdReady(string place) => this.interstitialAd != null;

        public virtual void LoadInterstitialAd(string place = "")
        {
            if (string.IsNullOrEmpty(place)) return;
            this.interstitialAdLoader.LoadAd(new AdRequestConfiguration.Builder(this.YandexSettings.InterstitialAdId.Id).Build());
        }

        public         bool TryGetInterstitialPlacementId(string placement, out string id) { id = default; return false; }

        public void ShowInterstitialAd(string place)
        {
            this.CurrentInterstitialAdPlacement = place;
            this.interstitialAd?.Show();
        }

        #endregion

        #endregion

        #region AOA

        private void InitAoaAd()
        {
            this.appOpenAdLoader = new AppOpenAdLoader();
            this.appOpenAdLoader.OnAdLoaded += this.HandleAoaAdLoaded;
            this.appOpenAdLoader.OnAdFailedToLoad += this.HandleAoaAdFailedToLoad;

            this.LoadAoaAd();
        }

        private void LoadAoaAd() => this.appOpenAdLoader.LoadAd(new AdRequestConfiguration.Builder(this.YandexSettings.AoaAdId.Id).Build());

        private void DestroyAoaAd()
        {
            this.appOpenAd?.Destroy();
            this.appOpenAd = null;
        }

        #region Events

        private void HandleAoaAdLoaded(object sender, AppOpenAdLoadedEventArgs args)
        {
            this.logService.Log($"onelog: Yandex: HandleAoaAdLoaded");
            this.appOpenAd = args.AppOpenAd;

            this.appOpenAd.OnAdClicked += this.HandleAoaAdClicked;
            this.appOpenAd.OnAdShown += this.HandleAoaAdShown;
            this.appOpenAd.OnAdFailedToShow += this.HandleAoaAdFailedToShow;
            this.appOpenAd.OnAdDismissed += this.HandleAoaAdDismissed;
            this.appOpenAd.OnAdImpression += this.HandleImpression;

            var adInfo = new AdInfo(this.AdPlatform, this.YandexSettings.AoaAdId.Id, AdFormatConstants.AppOpen);
            this.signalBus.Fire(new AppOpenLoadedSignal("", adInfo));
        }

        private void HandleAoaAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        {
            this.logService.Log($"onelog: Yandex: HandleAoaAdFailedToLoad: {args.Message}");
            this.signalBus.Fire(new AppOpenLoadFailedSignal(""));
        }

        private void HandleAoaAdClicked(object sender, EventArgs args)
        {
            this.logService.Log($"onelog: Yandex: HandleAoaAdClicked");
            var adInfo = new AdInfo(this.AdPlatform, this.YandexSettings.AoaAdId.Id, AdFormatConstants.AppOpen);
            this.signalBus.Fire(new AppOpenClickedSignal("", adInfo));
        }

        private void HandleAoaAdShown(object sender, EventArgs args)
        {
            this.logService.Log($"onelog: Yandex: HandleAoaAdShown");
            var adInfo = new AdInfo(this.AdPlatform, this.YandexSettings.AoaAdId.Id, AdFormatConstants.AppOpen);
            this.signalBus.Fire(new AppOpenFullScreenContentOpenedSignal("", adInfo));
            this.IsShowingAoaAd = true;
        }

        private void HandleAoaAdDismissed(object sender, EventArgs args)
        {
            this.logService.Log($"onelog: Yandex: HandleAoaAdDismissed");
            var adInfo = new AdInfo(this.AdPlatform, this.YandexSettings.AoaAdId.Id, AdFormatConstants.AppOpen);
            this.signalBus.Fire(new AppOpenFullScreenContentClosedSignal("", adInfo));
            this.DestroyAoaAd();
            this.LoadAoaAd();
            this.IsShowingAoaAd = false;
        }

        private void HandleAoaAdFailedToShow(object sender, AdFailureEventArgs args)
        {
            this.logService.Log($"onelog: Yandex: HandleAdFailedToShow event received with message: {args.Message}");
            this.signalBus.Fire(new AppOpenFullScreenContentFailedSignal("", args.Message));
            this.DestroyAoaAd();
            this.LoadAoaAd();
        }

        #endregion

        #region Public

        public bool IsAOAReady() => this.appOpenAd != null && !this.IsShowingAoaAd;

        public void ShowAOAAds() => this.appOpenAd?.Show();

        #endregion

        #endregion

        #region Remove Ads

        public void RemoveAds() { PlayerPrefs.SetInt("EM_REMOVE_ADS", -1); }

        public bool IsRemoveAds() => PlayerPrefs.HasKey("EM_REMOVE_ADS");

        #endregion

        #region Ads Revenue

        private void HandleImpression(object sender, ImpressionData impressionData)
        {
            var sData = impressionData?.rawData;
            this.logService.Log($"onelog: Yandex: HandleImpression: {sData}");
            if (string.IsNullOrEmpty(sData)) return;

            try
            {
                var data = JsonConvert.DeserializeObject<YandexImpressionData>(sData);
                var adsRevenueEvent = new AdsRevenueEvent()
                {
                    AdsRevenueSourceId = AdRevenueConstants.ARSourceYandex,
                    Revenue = data.revenueUSD,
                    Currency = "USD",
                    Placement = data.adType,
                    AdNetwork = data.network.name,
                    AdFormat = data.adType,
                    AdUnit = data.ad_unit_id
                };

                this.analyticServices.Track(adsRevenueEvent);
                this.signalBus.Fire(new AdRevenueSignal(adsRevenueEvent));
            }
            catch (Exception e)
            {
                this.logService.Error($"onelog: Yandex: Failed to parse impression data: {sData}");
            }
        }

        #endregion
    }
}
#endif