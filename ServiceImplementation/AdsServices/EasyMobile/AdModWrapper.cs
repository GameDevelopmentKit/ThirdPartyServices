namespace ServiceImplementation.AdsServices.EasyMobile
{
#if EM_ADMOB
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Core.AdsServices;
    using Core.AdsServices.Native;
    using Core.AdsServices.Signals;
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Utilities.Extension;
    using GameFoundation.Scripts.Utilities.LogService;
    using GoogleMobileAds.Api;
    using GoogleMobileAds.Common;
    using ServiceImplementation.AdsServices.Signal;
    using UnityEngine;
    using Zenject;

    public class AdModWrapper : IAOAAdService, IMRECAdService
#if ADMOB_NATIVE_ADS
                              , INativeAdsService
#endif
    {
        #region inject

        private readonly ILogService       logService;
        private readonly Config            config;
        private readonly SignalBus         signalBus;
        private readonly IAdServices       adServices;
        private readonly IAnalyticServices analyticService;
        private readonly AdServicesConfig  adServicesConfig;
        private readonly AppEventTracker   appEventTracker;

        #endregion

        public AdModWrapper(ILogService logService, Config config, SignalBus signalBus, IAdServices adServices, IAnalyticServices analyticService, AdServicesConfig adServicesConfig,AppEventTracker appEventTracker)
        {
            this.logService       = logService;
            this.config           = config;
            this.signalBus        = signalBus;
            this.adServices       = adServices;
            this.analyticService  = analyticService;
            this.adServicesConfig = adServicesConfig;
            this.appEventTracker  = appEventTracker;
        }

        public void Init()
        {
            this.signalBus.Subscribe<InterstitialAdDisplayedSignal>(this.ShownAdInDifferentProcessHandler);
            this.signalBus.Subscribe<RewardedAdDisplayedSignal>(this.ShownAdInDifferentProcessHandler);
            this.signalBus.Subscribe<InterstitialAdClosedSignal>(this.CloseAdInDifferentProcessHandler);
            this.signalBus.Subscribe<RewardedAdCompletedSignal>(this.CloseAdInDifferentProcessHandler);
            this.signalBus.Subscribe<RewardedSkippedSignal>(this.CloseAdInDifferentProcessHandler);

            this.StartLoadingAOATime = DateTime.Now;

            MobileAds.Initialize(_ =>
                                 {
                                     this.LoadAppOpenAd();
                                     this.IntervalCall(5);
                                     this.appEventTracker.ApplicationPauseAction += this.OnAppStateChange;
                                 });
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void InitAdmob() { MobileAds.Initialize(_ => { }); }

        private async void IntervalCall(int intervalSecond)
        {
            if (this.adServices.IsRemoveAds()) return;
            this.LoadAllMRec();
#if ADMOB_NATIVE_ADS
            this.LoadAllNativeAds();
#endif
            await UniTask.Delay(TimeSpan.FromSeconds(intervalSecond));
            this.IntervalCall(intervalSecond);
        }

        #region AOA

        private DateTime StartLoadingAOATime;
        private DateTime StartBackgroundTime;

        public class Config
        {
            public List<string>                       ADModAoaIds;
            public int                                AOAOpenAppThreshHold = 5; //after this number of seconds, AOA will not be shown
            public Dictionary<AdViewPosition, string> ADModMRecIds;
            public List<string>                       NativeAdIds;

            public bool IsShowAOAAtOpenApp   = true;
            public bool OpenAOAAfterResuming = true;

            public Config(List<string> adModAoaIds, bool isShowAoaAtOpenApp = true, bool openAoaAfterResuming = true)
            {
                this.ADModAoaIds          = adModAoaIds;
                this.OpenAOAAfterResuming = openAoaAfterResuming;
                this.IsShowAOAAtOpenApp   = isShowAoaAtOpenApp;
            }
        }

        public bool IsShowedFirstOpen { get; private set; } = false;

        private void ShownAdInDifferentProcessHandler()
        {
            this.logService.Log("ShownAdInDifferentProcessHandler");
            this.IsResumedFromAdsOrIAP = true;
        }

        private void CloseAdInDifferentProcessHandler()
        {
            this.logService.Log("CloseAdInDifferentProcessHandler");
            this.IsResumedFromAdsOrIAP = false;
        }

        private void OnAppStateChange(bool isForeGround)
        {
            this.OnAppStateChanged(isForeGround? AppState.Foreground: AppState.Background);
        }

        private async void OnAppStateChanged(AppState state)
        {
            await UniTask.SwitchToMainThread();
            this.logService.Log($"AOA App State is {state}");
            // Display the app open ad when the app is foregrounded.
            this.signalBus.Fire(new AppStateChangeSignal(state == AppState.Background));

            if (state != AppState.Foreground)
            {
                this.StartBackgroundTime = DateTime.Now;
                return;
            }

            var totalBackgroundSeconds = (DateTime.Now - this.StartBackgroundTime).TotalSeconds;
            if (totalBackgroundSeconds < this.adServicesConfig.MinPauseSecondToShowAoaAd)
            {
                this.logService.Log($"AOA background time: {totalBackgroundSeconds}");
                return;
            }

            if (!this.config.OpenAOAAfterResuming) return;

            if (this.IsResumedFromAdsOrIAP)
            {
                return;
            }

            if (!this.adServicesConfig.EnableAOAAd) return;

            if (!this.adServices.IsRemoveAds())
            {
                this.ShowAdIfAvailable();
            }
        }

        #region IAOAService

        public bool IsShowingAd           { get; set; } = false;
        public bool IsResumedFromAdsOrIAP { get; set; } = false;

        public float LoadingTimeToShowAOA => 5f;
        public async void ShowAdIfAvailable()
        {
            await UniTask.SwitchToMainThread();

            if (this.IsShowingAd)
            {
                return;
            }

            if (this.aoaAdLoadedInstance.IsAoaAdAvailable)
            {
                this.aoaAdLoadedInstance.Show();
                this.LoadAppOpenAd();
            }
        }

        #endregion

        private LoadedAppOpenAd aoaAdLoadedInstance = new();

        private class LoadedAppOpenAd
        {
            private AppOpenAd appOpenAd;
            private DateTime  loadedTime;
            public  bool      IsLoading = false;

            public void Init(AppOpenAd appOpenAd)
            {
                this.IsLoading  = false;
                this.appOpenAd  = appOpenAd;
                this.loadedTime = DateTime.UtcNow;
            }

            public bool IsAoaAdAvailable => this.appOpenAd != null && (DateTime.UtcNow - this.loadedTime).TotalHours < 4; //AppOpenAd is valid for 4 hours

            public void Show()
            {
                this.appOpenAd.Show();
                this.appOpenAd = null;
            }
        }

        private int currentAoaAdIndex          = 0;
        private int minAOASleepLoadingTime     = 8;
        private int currentAOASleepLoadingTime = 8;
        private int maxAOASleepLoadingTime     = 64;

        private void LoadAppOpenAd()
        {
            if (this.adServices.IsRemoveAds()) return;

            var adUnitId = this.config.ADModAoaIds[this.currentAoaAdIndex];
            this.logService.Log($"Start request Open App Ads Tier {this.currentAoaAdIndex} - {adUnitId}");


            //Don't need to load this ad if it's already loaded.

            if (this.aoaAdLoadedInstance is { IsAoaAdAvailable: true })
            {
                this.logService.Log($"AOA ads was already loaded");
                return;
            }

            this.aoaAdLoadedInstance.IsLoading = true;
            AppOpenAd.Load(adUnitId, Screen.orientation, new AdRequest.Builder().Build(), LoadAoaCompletedHandler);

            async void LoadAoaCompletedHandler(AppOpenAd appOpenAd, LoadAdError error)
            {
                if (error != null)
                {
                    // Handle the error.
                    this.logService.Log($"Failed to load the ad. (reason: {error.GetMessage()}), id: {adUnitId}");
                    this.aoaAdLoadedInstance.IsLoading = false;

                    this.currentAoaAdIndex = (this.currentAoaAdIndex + 1) % this.config.ADModAoaIds.Count;
                    if (this.currentAoaAdIndex != 0)
                    {
                        this.LoadAppOpenAd();
                    }
                    else
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(this.currentAOASleepLoadingTime));
                        this.currentAOASleepLoadingTime = Math.Min(this.currentAOASleepLoadingTime * 2, this.maxAOASleepLoadingTime);
                        this.LoadAppOpenAd();
                    }

                    return;
                }

                this.logService.Log($"Load Open App Ads Tier {this.currentAoaAdIndex} - {adUnitId} successfully!!");
                this.currentAoaAdIndex          = 0;
                this.currentAOASleepLoadingTime = this.minAOASleepLoadingTime;

                // App open ad is loaded.
                appOpenAd.OnAdFullScreenContentClosed += this.AOAHandleAdFullScreenContentClosed;
                appOpenAd.OnAdFullScreenContentFailed += this.AOAHandleAdFullScreenContentFailed;
                appOpenAd.OnAdFullScreenContentOpened += this.AOAHandleAdFullScreenContentOpened;
                appOpenAd.OnAdImpressionRecorded      += this.AOAHandleAdImpressionRecorded;
                appOpenAd.OnAdPaid                    += this.AdMobHandlePaidEvent;

                this.aoaAdLoadedInstance.Init(appOpenAd);

                if (!this.IsShowedFirstOpen && this.config.IsShowAOAAtOpenApp)
                {
                    var totalLoadingTime = (DateTime.Now - this.StartLoadingAOATime).TotalSeconds;
                    if (totalLoadingTime <= this.config.AOAOpenAppThreshHold)
                    {
                        this.ShowAdIfAvailable();
                    }
                    else
                    {
                        this.logService.Log($"AOA loading time for first open over the threshold {totalLoadingTime} > {this.config.AOAOpenAppThreshHold}!");
                    }

                    this.IsShowedFirstOpen = true;
                }
            }
        }

        private void AOAHandleAdFullScreenContentClosed()
        {
            this.logService.Log("Closed app open ad");
            this.signalBus.Fire(new AppOpenFullScreenContentClosedSignal(""));
            // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
            this.IsShowingAd = false;
        }

        private void AOAHandleAdFullScreenContentFailed(AdError args)
        {
            this.logService.Log($"Failed to present the ad (reason: {args.GetMessage()})");
            // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
        }

        private void AOAHandleAdFullScreenContentOpened()
        {
            this.logService.Log("Displayed app open ad");
            this.signalBus.Fire(new AppOpenFullScreenContentOpenedSignal(""));
            this.IsShowingAd = true;
        }

        private void AOAHandleAdImpressionRecorded()
        {
            this.logService.Log("Recorded ad impression");
        }

        private void AdMobHandlePaidEvent(AdValue args)
        {
            this.analyticService.Track(new AdsRevenueEvent()
                                       {
                                           AdsRevenueSourceId = "AdMob",
                                           Revenue            = args.Value / 1e5,
                                           Currency           = "USD",
                                           Placement          = "AOA",
                                           AdNetwork          = "AdMob"
                                       });

            this.logService.Log($"Received paid event. (currency: {args.CurrencyCode}, value: {args.Value}");
        }

        #endregion

        #region MREC

        private Dictionary<AdViewPosition, BannerView> positionToMRECBannerView  = new();
        private Dictionary<AdViewPosition, bool>       positionToMRECToIsLoading = new();

        public void ShowMREC(AdViewPosition adViewPosition) { this.positionToMRECBannerView[adViewPosition].Show(); }

        public void HideMREC(AdViewPosition adViewPosition) { this.positionToMRECBannerView[adViewPosition].Hide(); }

        public void StopMRECAutoRefresh(AdViewPosition adViewPosition) { }

        public void StartMRECAutoRefresh(AdViewPosition adViewPosition) { }

        public void LoadMREC(AdViewPosition adViewPosition)
        {
            if (this.positionToMRECBannerView.ContainsKey(adViewPosition) || this.positionToMRECToIsLoading.GetOrAdd(adViewPosition, () => false))
            {
                return;
            }

            var bannerView = new BannerView(this.config.ADModMRecIds[adViewPosition], AdSize.MediumRectangle, this.ConvertAdViewPosition(adViewPosition));

            var adRequest = new AdRequest.Builder().Build();

            // send the request to load the ad.
            bannerView.LoadAd(adRequest);
            this.positionToMRECToIsLoading[adViewPosition] = true;
#if UNITY_EDITOR
            OnBannerViewOnOnBannerAdLoaded();
#endif
            bannerView.OnBannerAdLoaded     += OnBannerViewOnOnBannerAdLoaded;
            bannerView.OnBannerAdLoadFailed += _ => { this.positionToMRECToIsLoading[adViewPosition] = false; };

            bannerView.OnBannerAdLoaded            += this.BannerViewOnAdLoaded;
            bannerView.OnBannerAdLoadFailed        += this.BannerViewOnAdLoadFailed;
            bannerView.OnAdClicked                 += this.BannerViewOnAdClicked;
            bannerView.OnAdPaid                    += this.AdMobHandlePaidEvent;
            bannerView.OnAdFullScreenContentOpened += this.BannerViewOnAdFullScreenContentOpened;
            bannerView.OnAdFullScreenContentClosed += this.BannerViewOnAdFullScreenContentClosed;

            void OnBannerViewOnOnBannerAdLoaded()
            {
                bannerView.Hide();
                this.positionToMRECToIsLoading[adViewPosition] = false;
                this.positionToMRECBannerView.Add(adViewPosition, bannerView);
            }
        }

        public bool IsMRECReady(AdViewPosition adViewPosition) { return this.positionToMRECBannerView.ContainsKey(adViewPosition); }

        private void LoadAllMRec()
        {
            foreach (var (position, _) in this.config.ADModMRecIds)
            {
                this.LoadMREC(position);
            }
        }

        private void BannerViewOnAdFullScreenContentClosed()
        {
            this.signalBus.Fire(new MRecAdDismissedSignal(""));
        }

        private void BannerViewOnAdFullScreenContentOpened()
        {
            this.signalBus.Fire(new MRecAdDisplayedSignal(""));
        }

        private void BannerViewOnAdClicked()
        {
            this.signalBus.Fire(new MRecAdClickedSignal(""));
        }

        private void BannerViewOnAdLoadFailed(LoadAdError obj)
        {
            this.signalBus.Fire(new MRecAdLoadFailedSignal(""));
        }

        private void BannerViewOnAdLoaded()
        {
            this.signalBus.Fire(new MRecAdLoadedSignal(""));
        }

        public event Action<string, AdInfo>    OnAdLoadedEvent;
        public event Action<string, ErrorInfo> OnAdLoadFailedEvent;
        public event Action<string, AdInfo>    OnAdClickedEvent;
        public event Action<string, AdInfo>    OnAdRevenuePaidEvent;
        public event Action<string, AdInfo>    OnAdExpandedEvent;
        public event Action<string, AdInfo>    OnAdCollapsedEvent;

        private AdPosition ConvertAdViewPosition(AdViewPosition adViewPosition) =>
            adViewPosition switch
            {
                AdViewPosition.TopLeft      => AdPosition.TopLeft,
                AdViewPosition.TopCenter    => AdPosition.Top,
                AdViewPosition.TopRight     => AdPosition.TopRight,
                AdViewPosition.CenterLeft   => AdPosition.Center,
                AdViewPosition.Centered     => AdPosition.Center,
                AdViewPosition.CenterRight  => AdPosition.Center,
                AdViewPosition.BottomLeft   => AdPosition.BottomLeft,
                AdViewPosition.BottomCenter => AdPosition.Bottom,
                AdViewPosition.BottomRight  => AdPosition.BottomRight,
                _                           => AdPosition.Center
            };

        #endregion

#if ADMOB_NATIVE_ADS

        #region Native Ads

        private Dictionary<string, NativeAd>        nativeAdsIdToNativeAd   { get; } = new();
        private HashSet<string>                     loadingNativeAdsIds     { get; } = new();
        private Dictionary<NativeAdsView, NativeAd> nativeAdsViewToNativeAd { get; } = new();

        private void LoadNativeAds(string adsId)
        {
            if (this.loadingNativeAdsIds.Contains(adsId) || this.nativeAdsIdToNativeAd.ContainsKey(adsId)) return;

            var adLoader = new AdLoader.Builder(adsId).ForNativeAd().Build();
            this.loadingNativeAdsIds.Add(adsId);

            adLoader.OnNativeAdLoaded += (_, arg) =>
                                         {
                                             this.nativeAdsIdToNativeAd.Add(adsId, arg.nativeAd);
                                             this.loadingNativeAdsIds.Remove(adsId);
                                         };

            adLoader.OnAdFailedToLoad += (_, _) => { this.loadingNativeAdsIds.Remove(adsId); };

            adLoader.OnNativeAdLoaded += this.HandleNativeAdLoaded;
            adLoader.OnAdFailedToLoad += this.HandleAdFailedToLoad;
            adLoader.LoadAd(new AdRequest.Builder().Build());
        }

        private NativeAd GetAvailableNativeAd()
        {
            var nativeAdPair = this.nativeAdsIdToNativeAd.First();
            this.nativeAdsIdToNativeAd.Remove(nativeAdPair.Key);

            return nativeAdPair.Value;
        }

        public void DrawNativeAds(NativeAdsView nativeAdsView)
        {
            if (!this.adServicesConfig.EnableNativeAd) return;

            if (this.nativeAdsIdToNativeAd.Count == 0 || this.nativeAdsViewToNativeAd.ContainsKey(nativeAdsView)) return;
            var nativeAd = this.nativeAdsViewToNativeAd.GetOrAdd(nativeAdsView, this.GetAvailableNativeAd);

            this.logService.Log($"Start set native ad: {nativeAdsView.name}");

            this.logService.Log($"native star rating : {nativeAd.GetStarRating()}");
            this.logService.Log($"native store: {nativeAd.GetStore()}");
            this.logService.Log($"native Price: {nativeAd.GetPrice()}");
            this.logService.Log($"native advertiser text: {nativeAd.GetAdvertiserText()}");
            this.logService.Log($"native icon: {nativeAd.GetIconTexture()?.texelSize}");

            this.logService.Log($"native headline: {nativeAd.GetHeadlineText()}");
            this.logService.Log($"native ad choice: {nativeAd.GetAdChoicesLogoTexture()?.texelSize}");

            // Get Texture2D for icon asset of native ad.
            nativeAdsView.headlineText.text = nativeAd.GetHeadlineText();

            if (!nativeAd.RegisterHeadlineTextGameObject(nativeAdsView.headlineText.gameObject))
            {
                // Handle failure to register ad asset.
                this.logService.Log($"Failed to register Headline text for native ad: {nativeAdsView.name}");
            }

            nativeAdsView.advertiserText.text = nativeAd.GetAdvertiserText();

            if (!nativeAd.RegisterAdvertiserTextGameObject(nativeAdsView.advertiserText.gameObject))
            {
                // Handle failure to register ad asset.
                this.logService.Log($"Failed to register advertiser text for native ad: {nativeAdsView.name}");
            }

            if (nativeAd.GetIconTexture() != null)
            {
                nativeAdsView.iconImage.gameObject.SetActive(true);
                nativeAdsView.iconImage.texture = nativeAd.GetIconTexture();

                // Register GameObject that will display icon asset of native ad.
                if (!nativeAd.RegisterIconImageGameObject(nativeAdsView.iconImage.gameObject))
                {
                    // Handle failure to register ad asset.
                    this.logService.Log($"Failed to register icon image for native ad: {nativeAdsView.name}");
                }
            }

            if (nativeAd.GetAdChoicesLogoTexture() != null)
            {
                nativeAdsView.adChoicesImage.gameObject.SetActive(true);
                nativeAdsView.adChoicesImage.texture = nativeAd.GetAdChoicesLogoTexture();

                if (!nativeAd.RegisterAdChoicesLogoGameObject(nativeAdsView.adChoicesImage.gameObject))
                {
                    // Handle failure to register ad asset.
                    this.logService.Log($"Failed to register ad choices image for native ad: {nativeAdsView.name}");
                }
            }
        }

        private void HandleAdFailedToLoad(object sender, AdFailedToLoadEventArgs e)
        {
            this.logService.Log($"Native ad failed to load: {e.LoadAdError.GetMessage()}");
        }

        private void HandleNativeAdLoaded(object sender, NativeAdEventArgs e)
        {
            e.nativeAd.OnPaidEvent += this.AdMobNativePaidHandler;
            this.logService.Log($"Native ad loaded successfully");
        }

        private void AdMobNativePaidHandler(object sender, AdValueEventArgs e) { this.AdMobHandlePaidEvent(e.AdValue); }

        private void LoadAllNativeAds()
        {
            foreach (var configNativeAdId in this.config.NativeAdIds)
            {
                this.LoadNativeAds(configNativeAdId);
            }
        }

        #endregion

#endif
    }

#endif
}