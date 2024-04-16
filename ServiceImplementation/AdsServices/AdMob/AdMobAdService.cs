#if ADMOB
namespace ServiceImplementation.AdsServices.AdMob
{
    using System;
    using Core.AdsServices;
    using Core.AdsServices.CollapsibleBanner;
    using Core.AdsServices.Signals;
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using Core.AnalyticServices.Signal;
    using GoogleMobileAds.Api;
    using ServiceImplementation.Configs;
    using ServiceImplementation.Configs.Ads;
    using UnityEngine;
    using Zenject;

    public class AdMobAdService : IAdServices, IAdLoadService, IBackFillAdsService, IInitializable, ICollapsibleBannerAd
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
            return adValue =>
            {
                var adsRevenueEvent = new AdsRevenueEvent
                {
                    AdsRevenueSourceId = AdRevenueConstants.ARSourceAdMob,
                    AdNetwork          = "AdMob",
                    AdFormat           = format,
                    Placement          = placement,
                    Currency           = "USD",
                    Revenue            = adValue.Value / 1e6,
                };

                this.signalBus.Fire(new AdRevenueSignal(adsRevenueEvent));

                this.analyticService.Track(adsRevenueEvent);
            };
        }

        #region Collapsible Banner

        private bool       isAvailableShowCollapsibleBanner;
        private BannerView collapsibleBannerView;
        private string     collapsibleBannerGuid = GetNewGuid();

        public void ShowCollapsibleBannerAd(bool useNewGuid, BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom)
        {
            const string PLACEMENT = "CollapsibleBanner";

            if (string.IsNullOrEmpty(this.config.CollapsibleBannerAdId.Id))
            {
                Debug.Log("onelog: ShowCollapsibleBannerAd - CollapsibleBannerAdId is null or empty. Please check the AdMob settings.");
                return;
            }

            this.collapsibleBannerGuid = useNewGuid ? GetNewGuid() : this.collapsibleBannerGuid;

            if (this.collapsibleBannerView == null)
            {
                var adSize = this.config.IsAdaptiveBannerEnabled ? AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth) : AdSize.Banner;
                this.collapsibleBannerView = new BannerView(this.config.CollapsibleBannerAdId.Id, adSize, bannerAdsPosition.ToAdMobAdPosition());

                #region Events

                this.collapsibleBannerView.OnBannerAdLoaded            += () => this.OnCollapsibleBannerLoaded(PLACEMENT);
                this.collapsibleBannerView.OnBannerAdLoadFailed        += error => this.OnCollapsibleBannerLoadFailed(PLACEMENT, error);
                this.collapsibleBannerView.OnAdFullScreenContentOpened += () => this.OnCollapsibleBannerPresented(PLACEMENT);
                this.collapsibleBannerView.OnAdFullScreenContentClosed += () => this.OnCollapsibleBannerDismissed(PLACEMENT);
                this.collapsibleBannerView.OnAdClicked                 += () => this.OnCollapsibleBannerClicked(PLACEMENT);
                this.collapsibleBannerView.OnAdPaid                    += this.TrackAdRevenue("CollapsibleBanner", PLACEMENT);

                #endregion
            }

            this.isAvailableShowCollapsibleBanner = true;
            var request = new AdRequest();
#if UNITY_IOS
            if(useNewGuid) AddPramsCollapsible();
#else
            AddPramsCollapsible();
#endif
            Debug.Log("onelog: ShowCollapsibleBannerAd - Load CollapsibleBanner.");
            this.collapsibleBannerView.LoadAd(request);
            return;

            void AddPramsCollapsible()
            {
                request.Extras.Add("collapsible_request_id", this.collapsibleBannerGuid);
                request.Extras.Add("collapsible", bannerAdsPosition == BannerAdsPosition.Bottom ? "bottom" : "top");
            }
        }

        private static string GetNewGuid() => Guid.NewGuid().ToString();

        public void HideCollapsibleBannerAd()
        {
            this.isAvailableShowCollapsibleBanner = false;
            this.collapsibleBannerView?.Hide();
            Debug.Log("onelog: HideCollapsibleBannerAd");
        }

        public void DestroyCollapsibleBannerAd()
        {
            if (this.collapsibleBannerView != null)
            {
                this.collapsibleBannerView.Destroy();
                this.collapsibleBannerView = null;
            }

            Debug.Log("onelog: DestroyCollapsibleBannerAd");
        }

        private void OnCollapsibleBannerLoaded(string placement)
        {
            this.signalBus.Fire(new CollapsibleBannerAdLoadedSignal(placement));
            if (this.isAvailableShowCollapsibleBanner)
            {
                this.collapsibleBannerView?.Show();
            }

            Debug.Log("onelog: OnCollapsibleBannerLoaded");
        }

        private void OnCollapsibleBannerLoadFailed(string placement, AdError adError)
        {
            Debug.Log($"onelog: OnCollapsibleBannerLoadFailed {placement} - {adError.GetMessage()}");
            this.signalBus.Fire(new CollapsibleBannerAdLoadFailedSignal(placement, adError.GetMessage()));
        }

        private void OnCollapsibleBannerPresented(string placement)
        {
            Debug.Log("onelog: OnCollapsibleBannerPresented");
            this.signalBus.Fire(new CollapsibleBannerAdPresentedSignal(placement));
        }

        private void OnCollapsibleBannerDismissed(string placement) { this.signalBus.Fire(new CollapsibleBannerAdDismissedSignal(placement)); }

        private void OnCollapsibleBannerClicked(string placement) { this.signalBus.Fire(new CollapsibleBannerAdClickedSignal(placement)); }

        #endregion
    }
}
#endif