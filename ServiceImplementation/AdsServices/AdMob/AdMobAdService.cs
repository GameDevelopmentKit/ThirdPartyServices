#if ADMOB
namespace ServiceImplementation.AdsServices.AdMob
{
    using System;
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using GameFoundation.Scripts.Utilities.LogService;
    using GoogleMobileAds.Api;
    using ServiceImplementation.Configs;
    using ServiceImplementation.Configs.Ads;
    using UnityEngine;
    using Zenject;

    public class AdMobAdService : IAdServices, IAdLoadService, IInitializable
    {
        #region Constructor

        private readonly AdMobSettings     config;
        private readonly SignalBus         signalBus;
        private readonly IAnalyticServices analyticService;
        private readonly ILogService       logger;

        public AdMobAdService(ThirdPartiesConfig config, SignalBus signalBus, IAnalyticServices analyticService, ILogService logger)
        {
            this.config          = config.AdSettings.AdMob;
            this.signalBus       = signalBus;
            this.analyticService = analyticService;
            this.logger          = logger;
        }

        #endregion

        public AdNetworkSettings AdNetworkSettings => this.config;

        #region Initialize

        private bool isInitialized;

        public void Initialize()
        {
            MobileAds.Initialize(initStatus =>
            {
                this.isInitialized = true;
                foreach (var (adapter, status) in initStatus.getAdapterStatusMap())
                {
                    this.logger.Log($"{adapter} {status.InitializationState}");
                }
            });
        }

        public bool IsAdsInitialized()
        {
            return this.isInitialized;
        }

        #endregion

        #region Banner

        private BannerView bannerView;

        public void ShowBannerAd(BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom, int width = 320, int height = 50)
        {
            const string PLACEMENT = "Banner";

            var size     = new AdSize(width, height);
            var position = bannerAdsPosition.ToAdMobAdPosition();

            if (this.bannerView is not null)
            {
                this.bannerView.SetPosition(position);
                this.bannerView.Show();
                return;
            }

            this.bannerView = new(this.config.DefaultBannerAdId.Id, size, position);

            #region Events

            this.bannerView.OnBannerAdLoaded            += () => this.signalBus.Fire<BannerAdLoadedSignal>(new(PLACEMENT));
            this.bannerView.OnBannerAdLoadFailed        += (error) => this.signalBus.Fire<BannerAdLoadFailedSignal>(new(PLACEMENT, error.GetMessage()));
            this.bannerView.OnAdClicked                 += () => this.signalBus.Fire<BannerAdClickedSignal>(new(PLACEMENT));
            this.bannerView.OnAdFullScreenContentOpened += () => this.signalBus.Fire<BannerAdPresentedSignal>(new(PLACEMENT));
            this.bannerView.OnAdFullScreenContentClosed += () => this.signalBus.Fire<BannerAdDismissedSignal>(new(PLACEMENT));
            this.bannerView.OnAdPaid += adValue =>
            {
                this.analyticService.Track(new AdsRevenueEvent
                {
                    AdsRevenueSourceId = "AdMob",
                    AdNetwork          = "AdMob",
                    AdFormat           = "Banner",
                    Placement          = PLACEMENT,
                    Currency           = "USD",
                    Revenue            = adValue.Value / 1e6,
                });
            };

            #endregion

            this.bannerView.LoadAd(new());
        }

        public void HideBannedAd()
        {
            this.bannerView?.Hide();
        }

        public void DestroyBannerAd()
        {
            this.bannerView?.Destroy();
            this.bannerView = null;
        }

        #endregion

        #region Interstitial

        private InterstitialAd interstitialAd;

        public bool IsInterstitialAdReady(string _)
        {
            return this.interstitialAd?.CanShowAd() ?? false;
        }

        public void LoadInterstitialAd(string place)
        {
            if (this.IsInterstitialAdReady(place)) return;

            InterstitialAd.Load(this.config.DefaultInterstitialAdId.Id, new(), (ad, error) =>
            {
                if (error is not null)
                {
                    this.signalBus.Fire<InterstitialAdLoadFailedSignal>(new(place, error.GetMessage()));
                    return;
                }
                this.signalBus.Fire<InterstitialAdDownloadedSignal>(new(place));
                this.interstitialAd = ad;
            });
        }

        public void ShowInterstitialAd(string place)
        {
            if (!this.IsInterstitialAdReady(place)) return;

            #region Events

            this.interstitialAd.OnAdClicked += () => this.signalBus.Fire<InterstitialAdClickedSignal>(new(place));
            this.interstitialAd.OnAdPaid += adValue =>
            {
                this.analyticService.Track(new AdsRevenueEvent
                {
                    AdsRevenueSourceId = "AdMob",
                    AdNetwork          = "AdMob",
                    AdFormat           = "Interstitial",
                    Placement          = place,
                    Currency           = "USD",
                    Revenue            = adValue.Value / 1e6,
                });
            };

            #endregion

            this.interstitialAd.Show();
        }

        #endregion

        #region Rewarded

        private RewardedAd rewardedAd;

        public bool IsRewardedAdReady(string place)
        {
            return this.rewardedAd?.CanShowAd() ?? false;
        }

        public void LoadRewardAds(string place)
        {
            if (this.IsRewardedAdReady(place)) return;

            RewardedAd.Load(this.config.DefaultRewardedAdId.Id, new(), (ad, error) =>
            {
                if (error is not null)
                {
                    this.signalBus.Fire<RewardedAdLoadFailedSignal>(new(place, error.GetMessage()));
                    return;
                }
                this.signalBus.Fire<RewardedAdLoadedSignal>(new(place));
                this.rewardedAd = ad;
            });
        }

        public void ShowRewardedAd(string place, Action onCompleted)
        {
            if (!this.IsRewardedAdReady(place)) return;

            #region Events

            this.rewardedAd.OnAdClicked += () => this.signalBus.Fire<RewardedAdLoadClickedSignal>(new(place));
            this.rewardedAd.OnAdPaid += adValue =>
            {
                this.analyticService.Track(new AdsRevenueEvent
                {
                    AdsRevenueSourceId = "AdMob",
                    AdNetwork          = "AdMob",
                    AdFormat           = "Reward",
                    Placement          = place,
                    Currency           = "USD",
                    Revenue            = adValue.Value / 1e6,
                });
            };

            #endregion

            this.rewardedAd.Show(_ =>
            {
                this.signalBus.Fire<RewardedAdCompletedSignal>(new(place));
                onCompleted?.Invoke();
            });
        }

        #endregion

        #region RemoveAds

        public void RemoveAds(bool _)
        {
            PlayerPrefs.SetInt("ADMOB_REMOVE_ADS", 1);
        }

        public bool IsRemoveAds()
        {
            return PlayerPrefs.HasKey("ADMOB_REMOVE_ADS");
        }

        #endregion
    }
}
#endif