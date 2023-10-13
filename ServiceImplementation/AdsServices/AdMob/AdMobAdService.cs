#if ADMOB
namespace ServiceImplementation.AdsServices.AdMob
{
    using System;
    using System.Collections.Generic;
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using GameFoundation.Scripts.Utilities.Extension;
    using GameFoundation.Scripts.Utilities.LogService;
    using GoogleMobileAds.Api;
    using ServiceImplementation.Configs;
    using ServiceImplementation.Configs.Ads;
    using UnityEngine;
    using Zenject;

    public class AdMobAdService : IAdServices, IAdLoadService, IInitializable
    {
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

        public AdNetworkSettings AdNetworkSettings => this.config;

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

        private BannerView bannerView;

        public void ShowBannerAd(BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom, int width = 320, int height = 50)
        {
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

            this.bannerView.OnBannerAdLoaded            += () => this.signalBus.Fire<BannerAdLoadedSignal>(new(""));
            this.bannerView.OnBannerAdLoadFailed        += (error) => this.signalBus.Fire<BannerAdLoadFailedSignal>(new("", error.GetMessage()));
            this.bannerView.OnAdClicked                 += () => this.signalBus.Fire<BannerAdClickedSignal>(new(""));
            this.bannerView.OnAdFullScreenContentOpened += () => this.signalBus.Fire<BannerAdPresentedSignal>(new(""));
            this.bannerView.OnAdFullScreenContentClosed += () => this.signalBus.Fire<BannerAdDismissedSignal>(new(""));
            this.bannerView.OnAdPaid += adValue =>
            {
                this.analyticService.Track(new AdsRevenueEvent
                {
                    AdsRevenueSourceId = "AdMob",
                    AdNetwork          = "AdMob",
                    AdFormat           = "Banner",
                    Placement          = "Banner",
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

        private readonly Dictionary<string, InterstitialAd> interstitialAds = new();

        public bool IsInterstitialAdReady(string place)
        {
            return this.interstitialAds.GetOrDefault(place)?.CanShowAd() ?? false;
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

                #region Events

                ad.OnAdClicked += () => this.signalBus.Fire<InterstitialAdClickedSignal>(new(place));
                ad.OnAdPaid += adValue =>
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

                this.signalBus.Fire<InterstitialAdDownloadedSignal>(new(place));
                this.interstitialAds[place] = ad;
            });
        }

        public void ShowInterstitialAd(string place)
        {
            if (!this.IsInterstitialAdReady(place)) return;

            this.interstitialAds[place].Show();
        }

        private readonly Dictionary<string, RewardedAd> rewardedAds = new();

        public bool IsRewardedAdReady(string place)
        {
            return this.rewardedAds.GetOrDefault(place)?.CanShowAd() ?? false;
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

                #region Events

                ad.OnAdClicked += () => this.signalBus.Fire<RewardedAdLoadClickedSignal>(new(place));
                ad.OnAdPaid += adValue =>
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

                this.signalBus.Fire<RewardedAdLoadedSignal>(new(place));
                this.rewardedAds[place] = ad;
            });
        }

        public void ShowRewardedAd(string place, Action onCompleted)
        {
            if (!this.IsRewardedAdReady(place)) return;

            this.rewardedAds[place].Show(_ =>
            {
                this.signalBus.Fire<RewardedAdCompletedSignal>(new(place));
                onCompleted?.Invoke();
            });
        }

        public void RemoveAds(bool _)
        {
            PlayerPrefs.SetInt("ADMOB_REMOVE_ADS", 1);
        }

        public bool IsRemoveAds()
        {
            return PlayerPrefs.HasKey("ADMOB_REMOVE_ADS");
        }
    }
}
#endif