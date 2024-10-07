#if ADMOB
namespace ServiceImplementation.AdsServices.AdMob
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Core.AdsServices;
    using Core.AdsServices.CollapsibleBanner;
    using Core.AdsServices.Helpers;
    using Core.AdsServices.Signals;
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using Core.AnalyticServices.Signal;
    using GoogleMobileAds.Api;
    using ServiceImplementation.Configs;
    using ServiceImplementation.Configs.Ads;
    using UnityEngine;
    using Zenject;
    using Debug = UnityEngine.Debug;

    public class AdMobAdService : IAdServices, IAdLoadService, IInitializable, ICollapsibleBannerAd
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

        string IAdServices.AdPlatform => AdRevenueConstants.ARSourceAdMob;

        public string AdPlatform => AdRevenueConstants.ARSourceAdMob;

        public AdNetworkSettings AdNetworkSettings => this.config;

        private const string BannerAdFormat            = "Banner";
        private const string InterstitialAdFormat      = "Interstitial";
        private const string RewardedAdFormat          = "Rewarded";
        private const string CollapsibleBannerAdFormat = "CollapsibleBanner";

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
            var adInfo   = new AdInfo(this.AdPlatform, this.config.DefaultBannerAdId.Id, BannerAdFormat);
            var size     = new AdSize(width, height);
            var position = bannerAdsPosition.ToAdMobAdPosition();

            if (this.bannerView is not null)
            {
                this.bannerView.SetPosition(position);
                this.bannerView.Show();

                return;
            }

            this.bannerView = new BannerView(this.config.DefaultBannerAdId.Id, size, position);

            #region Events

            this.bannerView.OnBannerAdLoaded            += OnBannerAdLoaded;
            this.bannerView.OnBannerAdLoadFailed        += (error) => this.signalBus.Fire(new BannerAdLoadFailedSignal(BannerAdFormat, error.GetMessage()));
            this.bannerView.OnAdFullScreenContentOpened += OnAdFullScreenContentOpened;
            this.bannerView.OnAdFullScreenContentClosed += OnAddFullScreenContentClosed;
            this.bannerView.OnAdClicked                 += OnAdClicked;
            this.bannerView.OnAdPaid                    += this.TrackAdRevenue(BannerAdFormat, BannerAdFormat, this.config.DefaultBannerAdId.Id);

            void OnBannerAdLoaded() { this.signalBus.Fire(new BannerAdLoadedSignal(BannerAdFormat, adInfo)); }

            void OnAdFullScreenContentOpened() { this.signalBus.Fire(new BannerAdPresentedSignal(BannerAdFormat)); }

            void OnAddFullScreenContentClosed() { this.signalBus.Fire(new BannerAdDismissedSignal(BannerAdFormat)); }

            void OnAdClicked() { this.signalBus.Fire(new BannerAdClickedSignal(BannerAdFormat, adInfo)); }

            #endregion

            this.bannerView.LoadAd(new AdRequest());
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

        public bool TryGetInterstitialPlacementId(string place, out string id)
        {
            return AdPlacementHelper.TryGetPlacementId(
                place,
                this.config.DefaultInterstitialAdId,
                this.AdNetworkSettings.CustomInterstitialAdIds,
                out id);
        }

        public bool IsInterstitialAdReady(string _) { return this.interstitialAd?.CanShowAd() ?? false; }

        public void LoadInterstitialAd(string place)
        {
            if (this.IsInterstitialAdReady(place)) return;

            var stopwatch = Stopwatch.StartNew();

            InterstitialAd.Load(this.config.DefaultInterstitialAdId.Id, new AdRequest(), (ad, error) =>
            {
                stopwatch.Stop();

                if (error is not null)
                {
                    this.signalBus.Fire(new InterstitialAdLoadFailedSignal(place, error.GetMessage(), stopwatch.ElapsedMilliseconds));

                    return;
                }

                var adInfo = new AdInfo(this.AdPlatform, this.config.DefaultInterstitialAdId.Id, InterstitialAdFormat);
                this.signalBus.Fire(new InterstitialAdLoadedSignal(place, stopwatch.ElapsedMilliseconds, adInfo));
                this.interstitialAd?.Destroy();
                this.interstitialAd = ad;
            });
        }

        public void ShowInterstitialAd(string place)
        {
            if (!this.IsInterstitialAdReady(place)) return;
            var adInfo = new AdInfo(this.AdPlatform, this.config.DefaultInterstitialAdId.Id, InterstitialAdFormat);

            #region Events

            this.interstitialAd.OnAdFullScreenContentOpened += OnAdFullScreenContentOpened;
            this.interstitialAd.OnAdFullScreenContentClosed += OnAddFullScreenContentClosed;
            this.interstitialAd.OnAdFullScreenContentFailed += _ => this.signalBus.Fire(new InterstitialAdDisplayedFailedSignal(place));
            this.interstitialAd.OnAdClicked                 += OnAdClicked;
            this.interstitialAd.OnAdPaid                    += this.TrackAdRevenue(InterstitialAdFormat, place, this.config.DefaultInterstitialAdId.Id);

            void OnAdFullScreenContentOpened() { this.signalBus.Fire(new InterstitialAdDisplayedSignal(place, adInfo)); }

            void OnAddFullScreenContentClosed() { this.signalBus.Fire(new InterstitialAdClosedSignal(place, adInfo)); }

            void OnAdClicked() { this.signalBus.Fire(new InterstitialAdClickedSignal(place, adInfo)); }

            #endregion

            this.interstitialAd.Show();
        }

        #endregion

        #region Rewarded

        private RewardedAd rewardedAd;

        public bool IsRewardedAdReady(string place) { return this.rewardedAd?.CanShowAd() ?? false; }

        public bool TryGetRewardPlacementId(string placement, out string id)
        {
            return AdPlacementHelper.TryGetPlacementId(
                placement,
                this.config.DefaultRewardedAdId,
                this.AdNetworkSettings.CustomRewardedAdIds,
                out id);
        }

        public void LoadRewardAds(string place)
        {
            if (this.IsRewardedAdReady(place)) return;

            var stopwatch = Stopwatch.StartNew();

            RewardedAd.Load(this.config.DefaultRewardedAdId.Id, new AdRequest(), (ad, error) =>
            {
                stopwatch.Stop();

                if (error is not null)
                {
                    this.signalBus.Fire(new RewardedAdLoadFailedSignal(place, error.GetMessage(), stopwatch.ElapsedMilliseconds));

                    return;
                }

                var adInfo = new AdInfo(this.AdPlatform, this.config.DefaultRewardedAdId.Id, RewardedAdFormat);
                this.signalBus.Fire(new RewardedAdLoadedSignal(place, stopwatch.ElapsedMilliseconds, adInfo));
                this.rewardedAd?.Destroy();
                this.rewardedAd = ad;
            });
        }

        public void ShowRewardedAd(string place, Action onCompleted, Action onFailed)
        {
            if (!this.IsRewardedAdReady(place)) return;
            var adInfo = new AdInfo(this.AdPlatform, this.config.DefaultRewardedAdId.Id, RewardedAdFormat);

            #region Events

            this.rewardedAd.OnAdFullScreenContentOpened += OnAdFullScreenContentOpened;
            this.rewardedAd.OnAdFullScreenContentFailed += _ => OnAdFullScreenContentFailed();
            this.rewardedAd.OnAdClicked                 += OnAdClicked;
            this.rewardedAd.OnAdPaid                    += (_) => this.signalBus.Fire(new RewardedAdEligibleSignal(place));
            this.rewardedAd.OnAdPaid                    += this.TrackAdRevenue(RewardedAdFormat, place, this.config.DefaultRewardedAdId.Id);

            #endregion

            this.rewardedAd.Show(_ =>
            {
                this.signalBus.Fire(new RewardedAdCompletedSignal(place, adInfo));
                this.signalBus.Fire(new RewardedAdClosedSignal(place, adInfo));
                onCompleted?.Invoke();
            });

            this.signalBus.Fire(new RewardedAdCalledSignal(place, adInfo));

            return;

            void OnAdClicked() { this.signalBus.Fire(new RewardedAdClickedSignal(place, adInfo)); }

            void OnAdFullScreenContentOpened() { this.signalBus.Fire(new RewardedAdDisplayedSignal(place, adInfo)); }

            void OnAdFullScreenContentFailed()
            {
                this.signalBus.Fire(new RewardedSkippedSignal(place, adInfo));
                onFailed?.Invoke();
                onFailed = null;
            }
        }

        #endregion

        #region RemoveAds

        public void RemoveAds() { PlayerPrefs.SetInt("ADMOB_REMOVE_ADS", 1); }

        public bool IsRemoveAds() { return PlayerPrefs.HasKey("ADMOB_REMOVE_ADS"); }

        #endregion

        private Action<AdValue> TrackAdRevenue(string format, string placement, string adUnit)
        {
            return adValue =>
            {
                var adsRevenueEvent = new AdsRevenueEvent
                {
                    AdsRevenueSourceId = this.AdPlatform,
                    AdFormat           = format,
                    AdNetwork          = "AdMob",
                    AdUnit             = adUnit,
                    NetworkPlacement   = placement,
                    Revenue            = adValue.Value/1e6,
                    Currency           = adValue.CurrencyCode,
                };

                this.signalBus.Fire(new AdRevenueSignal(adsRevenueEvent));

                this.analyticService.Track(adsRevenueEvent);
            };
        }

        #region Collapsible Banner

        private string                                    collapsibleBannerGuid  = GetNewGuid();
        private Dictionary<BannerAdsPosition, BannerView> collapsibleBannerViews = new Dictionary<BannerAdsPosition, BannerView>();
        private Dictionary<BannerAdsPosition, bool>       IsCollapsibleLoading { get; set; } = new Dictionary<BannerAdsPosition, bool>();

        public void LoadCollapsibleBannerAd(bool useNewGuid, BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom)
        {
            if (string.IsNullOrEmpty(this.config.CollapsibleBannerAdId.Id))
            {
                Debug.Log(" ShowCollapsibleBannerAd - CollapsibleBannerAdId is null or empty. Please check the AdMob settings.");

                return;
            }

            if (this.IsCollapsibleLoading.TryGetValue(bannerAdsPosition, out var isLoading) && isLoading)
            {
                return;
            }

            this.collapsibleBannerGuid = useNewGuid ? GetNewGuid() : this.collapsibleBannerGuid;

            if (!this.collapsibleBannerViews.TryGetValue(bannerAdsPosition, out var collapsibleBannerView))
            {
                var adSize = this.config.IsAdaptiveBannerEnabled ? AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth) : AdSize.Banner;
                collapsibleBannerView = new BannerView(this.config.CollapsibleBannerAdId.Id, adSize, bannerAdsPosition.ToAdMobAdPosition());

                #region Events

                collapsibleBannerView.OnBannerAdLoaded            += () => this.OnCollapsibleBannerLoaded(CollapsibleBannerAdFormat, bannerAdsPosition, collapsibleBannerView);
                collapsibleBannerView.OnBannerAdLoadFailed        += error => this.OnCollapsibleBannerLoadFailed(CollapsibleBannerAdFormat, error, bannerAdsPosition);
                collapsibleBannerView.OnAdFullScreenContentOpened += () => this.OnCollapsibleBannerPresented(CollapsibleBannerAdFormat, bannerAdsPosition);
                collapsibleBannerView.OnAdFullScreenContentClosed += () => this.OnCollapsibleBannerDismissed(CollapsibleBannerAdFormat, bannerAdsPosition);
                collapsibleBannerView.OnAdClicked                 += () => this.OnCollapsibleBannerClicked(CollapsibleBannerAdFormat);
                collapsibleBannerView.OnAdPaid                    += this.TrackAdRevenue(CollapsibleBannerAdFormat, CollapsibleBannerAdFormat, this.config.CollapsibleBannerAdId.Id);

                #endregion
            }
            else
            {
                return;
            }

            var request = new AdRequest();
#if UNITY_IOS
            if(useNewGuid) AddPramsCollapsible();
#else
            AddPramsCollapsible();
#endif
            Debug.Log("Load CollapsibleBanner.");
            collapsibleBannerView.LoadAd(request);
            collapsibleBannerView.Hide();
            this.IsCollapsibleLoading[bannerAdsPosition]   = true;
            this.collapsibleBannerViews[bannerAdsPosition] = collapsibleBannerView;
            return;

            void AddPramsCollapsible()
            {
                request.Extras.Add("collapsible_request_id", this.collapsibleBannerGuid);
                request.Extras.Add("collapsible", bannerAdsPosition == BannerAdsPosition.Bottom ? "bottom" : "top");
            }
        }

        public void ShowCollapsibleBannerAd(bool useNewGuid, BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom)
        {
            if (!this.collapsibleBannerViews.TryGetValue(bannerAdsPosition, out var collapsibleBannerView)) return;
            
            collapsibleBannerView.Show();
        }

        private static string GetNewGuid() => Guid.NewGuid().ToString();

        public void HideCollapsibleBannerAd()
        {
            for (var i = 0; i < this.collapsibleBannerViews.Count; i++)
            {
                this.collapsibleBannerViews.ElementAt(i).Value.Hide();
            }

            Debug.Log("HideCollapsibleBannerAd");
        }

        public void DestroyCollapsibleBannerAd()
        {
            for (var i = 0; i < this.collapsibleBannerViews.Count; i++)
            {
                this.collapsibleBannerViews.ElementAt(i).Value.Destroy();
            }

            this.collapsibleBannerViews.Clear();

            Debug.Log("DestroyCollapsibleBannerAd");
        }

        public bool IsHasCollapsibleBannerAd(BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom) { return this.collapsibleBannerViews.ContainsKey(bannerAdsPosition); }

        private void OnCollapsibleBannerLoaded(string placement, BannerAdsPosition bannerAdsPosition, BannerView collapsibleBannerView)
        {
            var adInfo = new AdInfo(this.AdPlatform, this.config.CollapsibleBannerAdId.Id, CollapsibleBannerAdFormat);
            this.signalBus.Fire(new CollapsibleBannerAdLoadedSignal(placement, adInfo));
            this.IsCollapsibleLoading[bannerAdsPosition] = false;
            Debug.Log($"OnCollapsibleBannerLoaded {bannerAdsPosition}");
        }

        private void OnCollapsibleBannerLoadFailed(string placement, AdError adError, BannerAdsPosition bannerAdsPosition)
        {
            this.IsCollapsibleLoading[bannerAdsPosition] = false;
            Debug.Log($"OnCollapsibleBannerLoadFailed {placement} - {adError.GetMessage()}");
            this.signalBus.Fire(new CollapsibleBannerAdLoadFailedSignal(placement, adError.GetMessage()));
        }

        private void OnCollapsibleBannerPresented(string placement, BannerAdsPosition bannerAdsPosition)
        {
            Debug.Log("OnCollapsibleBannerPresented");
            this.signalBus.Fire(new CollapsibleBannerAdPresentedSignal(placement));
        }

        private void OnCollapsibleBannerDismissed(string placement, BannerAdsPosition bannerAdsPosition)
        {
            Debug.Log("OnCollapsibleBannerDismissed");
            this.signalBus.Fire(new CollapsibleBannerAdDismissedSignal(placement));
        }

        private void OnCollapsibleBannerClicked(string placement)
        {
            Debug.Log("OnCollapsibleBannerClicked");
            var adInfo = new AdInfo(this.AdPlatform, this.config.CollapsibleBannerAdId.Id, CollapsibleBannerAdFormat);
            this.signalBus.Fire(new CollapsibleBannerAdClickedSignal(placement, adInfo));
        }

        #endregion
    }
}
#endif