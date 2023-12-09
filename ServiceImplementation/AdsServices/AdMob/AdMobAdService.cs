#if ADMOB
namespace ServiceImplementation.AdsServices.AdMob
{
    using System;
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using GoogleMobileAds.Api;
    using ServiceImplementation.Configs;
    using ServiceImplementation.Configs.Ads;
    using UnityEngine;
    using Zenject;

    public class AdMobAdService : IAdServices, IAdLoadService, IBackFillAdsService, IInitializable
    {
        #region Constructor

        private readonly AdMobSettings     config;
        private readonly SignalBus         signalBus;
        private readonly IAnalyticServices analyticService;

        public AdMobAdService(ThirdPartiesConfig config, SignalBus signalBus, IAnalyticServices analyticService)
        {
            this.config          = config.AdSettings.AdMob;
            this.signalBus       = signalBus;
            this.analyticService = analyticService;
        }

        #endregion

        public AdNetworkSettings AdNetworkSettings => this.config;

        #region Initialize

        private bool isInitialized = true;

        public void Initialize()
        {
            // MobileAds.Initialize(_ =>
            // {
            //     this.isInitialized = true;
            // });
        }

        public bool IsAdsInitialized() { return this.isInitialized; }

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
            this.bannerView.OnAdFullScreenContentOpened += () => this.signalBus.Fire<BannerAdPresentedSignal>(new(PLACEMENT));
            this.bannerView.OnAdFullScreenContentClosed += () => this.signalBus.Fire<BannerAdDismissedSignal>(new(PLACEMENT));
            this.bannerView.OnAdClicked                 += () => this.signalBus.Fire<BannerAdClickedSignal>(new(PLACEMENT));
            this.bannerView.OnAdPaid                    += this.TrackAdRevenue("Banner", PLACEMENT);

            #endregion

            this.bannerView.LoadAd(new());
        }

        public void HideBannedAd() { this.bannerView?.Hide(); }

        public void DestroyBannerAd()
        {
            this.bannerView?.Destroy();
            this.bannerView = null;
        }

        #endregion

        #region Interstitial

        private InterstitialAd interstitialAd;

        public bool IsInterstitialAdReady(string _) { return this.interstitialAd?.CanShowAd() ?? false; }

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
                this.interstitialAd?.Destroy();
                this.interstitialAd = ad;
            });
        }

        public void ShowInterstitialAd(string place)
        {
            if (!this.IsInterstitialAdReady(place)) return;

            #region Events

            this.interstitialAd.OnAdFullScreenContentOpened += () => this.signalBus.Fire<InterstitialAdDisplayedSignal>(new(place));
            this.interstitialAd.OnAdFullScreenContentClosed += () => this.signalBus.Fire<InterstitialAdClosedSignal>(new(place));
            this.interstitialAd.OnAdFullScreenContentFailed += (_) => this.signalBus.Fire<InterstitialAdDisplayedFailedSignal>(new(place));
            this.interstitialAd.OnAdClicked                 += () => this.signalBus.Fire<InterstitialAdClickedSignal>(new(place));
            this.interstitialAd.OnAdPaid                    += (_) => this.signalBus.Fire<InterstitialAdEligibleSignal>(new(place));
            this.interstitialAd.OnAdPaid                    += this.TrackAdRevenue("Interstitial", place);

            #endregion

            this.interstitialAd.Show();
        }

        #endregion

        #region Rewarded

        private RewardedAd rewardedAd;

        public bool IsRewardedAdReady(string place) { return this.rewardedAd?.CanShowAd() ?? false; }

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
                this.rewardedAd?.Destroy();
                this.rewardedAd = ad;
            });
        }

        public void ShowRewardedAd(string place, Action onCompleted, Action onFailed)
        {
            if (!this.IsRewardedAdReady(place)) return;

            #region Events

            this.rewardedAd.OnAdFullScreenContentOpened += () => this.signalBus.Fire<RewardedAdDisplayedSignal>(new(place));
            this.rewardedAd.OnAdFullScreenContentFailed += (_) => OnAdFullScreenContentFailed();
            this.rewardedAd.OnAdClicked                 += () => this.signalBus.Fire<RewardedAdLoadClickedSignal>(new(place));
            this.rewardedAd.OnAdPaid                    += (_) => this.signalBus.Fire<RewardedAdEligibleSignal>(new(place));
            this.rewardedAd.OnAdPaid                    += this.TrackAdRevenue("Rewarded", place);

            #endregion

            this.rewardedAd.Show(_ =>
            {
                this.signalBus.Fire<RewardedAdCompletedSignal>(new(place));
                this.signalBus.Fire<RewardedAdClosedSignal>(new(place));
                onCompleted?.Invoke();
            });
            this.signalBus.Fire<RewardedAdCalledSignal>(new(place));

            void OnAdFullScreenContentFailed()
            {
                this.signalBus.Fire<RewardedSkippedSignal>(new(place));
                onFailed?.Invoke();
                onFailed = null;
            }
        }

        #endregion

        #region RemoveAds

        public void RemoveAds(bool _) { PlayerPrefs.SetInt("ADMOB_REMOVE_ADS", 1); }

        public bool IsRemoveAds() { return PlayerPrefs.HasKey("ADMOB_REMOVE_ADS"); }

        #endregion

        private Action<AdValue> TrackAdRevenue(string format, string placement)
        {
            return adValue => this.analyticService.Track(new AdsRevenueEvent
            {
                AdsRevenueSourceId = AdRevenueConstants.ARSourceAdMob,
                AdNetwork          = "AdMob",
                AdFormat           = format,
                Placement          = placement,
                Currency           = "USD",
                Revenue            = adValue.Value / 1e6,
            });
        }
    }
}
#endif