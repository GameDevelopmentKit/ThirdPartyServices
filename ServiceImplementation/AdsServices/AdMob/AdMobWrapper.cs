namespace ServiceImplementation.AdsServices.EasyMobile
{
#if ADMOB
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Utilities.Extension;
    using GameFoundation.Scripts.Utilities.LogService;
    using GoogleMobileAds.Api;
    using ServiceImplementation.Configs;
    using ServiceImplementation.Configs.Ads;
    using UnityEngine;
    using Zenject;
#if ADMOB_NATIVE_ADS
    using Core.AdsServices.Native;
#endif

    public class AdMobWrapper : IAOAAdService, IMRECAdService, IInitializable
#if ADMOB_NATIVE_ADS
        , INativeAdsService
#endif
    {
        #region inject

        private readonly ILogService        logService;
        private readonly SignalBus          signalBus;
        private readonly IAdServices        adServices;
        private readonly IAnalyticServices  analyticService;
        private readonly ThirdPartiesConfig thirdPartiesConfig;
        private readonly AdServicesConfig   adServicesConfig;

        #endregion

        public AdMobWrapper(ILogService logService, SignalBus signalBus, IAdServices adServices, IAnalyticServices analyticService,
            ThirdPartiesConfig thirdPartiesConfig, AdServicesConfig adServicesConfig)
        {
            this.logService         = logService;
            this.signalBus          = signalBus;
            this.adServices         = adServices;
            this.analyticService    = analyticService;
            this.thirdPartiesConfig = thirdPartiesConfig;
            this.adServicesConfig   = adServicesConfig;
        }

        public void Initialize()
        {
            this.VerifySetting();
            this.Init();
        }

        private AdMobSettings ADMobSettings => this.thirdPartiesConfig.AdSettings.AdMob;

        private void VerifySetting()
        {
            //Interstitial
            if (string.IsNullOrEmpty(this.ADMobSettings.DefaultInterstitialAdId.Id) && this.ADMobSettings.CustomInterstitialAdIds.Values.Contains(this.ADMobSettings.DefaultInterstitialAdId))
                throw new RuntimeWrappedException("The default interstitial id is duplicated with custom interstitial Id");
            if (this.ADMobSettings.CustomInterstitialAdIds.GroupBy(x => x.Value).Any(group => group.Count() > 1))
                throw new RuntimeWrappedException("There is duplicated interstitial admob ads service");

            //Rewarded ads
            if (string.IsNullOrEmpty(this.ADMobSettings.DefaultRewardedAdId.Id) && this.ADMobSettings.CustomRewardedAdIds.Values.Contains(this.ADMobSettings.DefaultInterstitialAdId))
                throw new RuntimeWrappedException("The default interstitial id is duplicated with custom interstitial Id");
            if (this.ADMobSettings.CustomRewardedAdIds.GroupBy(x => x.Value).Any(group => group.Count() > 1)) throw new RuntimeWrappedException("There is duplicated Rewarded video admob ads service");
        }

        private async void Init()
        {
            await UniTask.SwitchToMainThread();
#if !GOOGLE_MOBILE_ADS_BELLOW_7_4_0
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
#endif
            this.logService.Log("AOA start init");
            MobileAds.Initialize(_ =>
            {
                this.logService.Log("AOA finished init");
                this.LoadAppOpenAd();
                this.IntervalCall(5);
            });
        }

        // Temporarily disable this
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

        private string GetInterstitialAdsIdByPlace(string place)
        {
            return this.ADMobSettings.CustomInterstitialAdIds.GetValueOrDefault(AdPlacement.PlacementWithName(place), this.ADMobSettings.DefaultInterstitialAdId).Id;
        }

        #region AOA

        public bool IsShowingAOAAd { get; set; } = false;

        public float LoadingTimeToShowAOA => this.thirdPartiesConfig.AdSettings.AOAThreshHold;


        public bool IsAOAReady() { return this.aoaAdLoadedInstance.IsAoaAdAvailable && !this.IsShowingAOAAd; }
        public void ShowAOAAds()
        {
            this.aoaAdLoadedInstance.Show();
            this.LoadAppOpenAd();
        }

        private LoadedAppOpenAd aoaAdLoadedInstance = new();

        private class LoadedAppOpenAd
        {
            private AppOpenAd appOpenAd;
            private DateTime  loadedTime;

            public void Init(AppOpenAd appOpenAd)
            {
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

        private int minAOASleepLoadingTime     = 8;
        private int currentAOASleepLoadingTime = 8;
        private int maxAOASleepLoadingTime     = 64;

        private void LoadAppOpenAd()
        {
            if (this.adServices.IsRemoveAds()) return;

            var adUnitId = this.ADMobSettings.AOAAdId.Id;

            if (this.aoaAdLoadedInstance is { IsAoaAdAvailable: true })
            {
                this.logService.Log($"AOA ads was already loaded");
                return;
            }

            AppOpenAd.Load(adUnitId, new AdRequest(), LoadAoaCompletedHandler);
            return;

            async void LoadAoaCompletedHandler(AppOpenAd appOpenAd, LoadAdError error)
            {
                if (error != null)
                {
                    // Handle the error.
                    this.logService.Log($"Failed to load the ad. (reason: {error.GetMessage()}), id: {adUnitId}");
                    this.signalBus.Fire(new AppOpenLoadFailedSignal(""));

                    await UniTask.Delay(TimeSpan.FromSeconds(this.currentAOASleepLoadingTime));
                    this.currentAOASleepLoadingTime = Math.Min(this.currentAOASleepLoadingTime * 2, this.maxAOASleepLoadingTime);
                    this.LoadAppOpenAd();

                    return;
                }

                this.signalBus.Fire(new AppOpenLoadedSignal(""));
                this.currentAOASleepLoadingTime = this.minAOASleepLoadingTime;

                // App open ad is loaded.
                appOpenAd.OnAdFullScreenContentClosed += this.AOAHandleAdFullScreenContentClosed;
                appOpenAd.OnAdFullScreenContentFailed += this.AOAHandleAdFullScreenContentFailed;
                appOpenAd.OnAdFullScreenContentOpened += this.AOAHandleAdFullScreenContentOpened;
                appOpenAd.OnAdImpressionRecorded      += this.AOAHandleAdImpressionRecorded;
                appOpenAd.OnAdPaid                    += this.AOAHandleAdPaid;

                this.aoaAdLoadedInstance.Init(appOpenAd);
            }
        }

        private void AOAHandleAdFullScreenContentClosed()
        {
            this.logService.Log("Closed app open ad");
            this.signalBus.Fire(new AppOpenFullScreenContentClosedSignal(""));
            this.IsShowingAOAAd = false;
        }

        private void AOAHandleAdFullScreenContentFailed(AdError args)
        {
            this.logService.Log($"Failed to present the ad (reason: {args.GetMessage()})");
            this.signalBus.Fire(new AppOpenFullScreenContentFailedSignal(""));
        }

        private void AOAHandleAdFullScreenContentOpened()
        {
            this.logService.Log("Displayed app open ad");
            this.signalBus.Fire(new AppOpenFullScreenContentOpenedSignal(""));
            this.IsShowingAOAAd = true;
        }

        private void AOAHandleAdImpressionRecorded() { this.logService.Log("Recorded ad impression"); }

        private void AOAHandleAdPaid(AdValue obj) => this.AdMobHandlePaidEvent(obj, "AOA");

        #endregion

        #region MREC

        private Dictionary<AdViewPosition, BannerView> positionToMRECBannerView  = new();
        private Dictionary<AdViewPosition, bool>       positionToMRECToIsLoading = new();

        public void ShowMREC(AdViewPosition adViewPosition)
        {
            this.positionToMRECBannerView[adViewPosition].Show();
            this.MrecBannerViewDisplay();
        }

        public void HideMREC(AdViewPosition adViewPosition)
        {
            this.positionToMRECBannerView[adViewPosition].Hide();
            this.MrecBannerViewDismissed();
        }

        public void StopMRECAutoRefresh(AdViewPosition adViewPosition) { }

        public void StartMRECAutoRefresh(AdViewPosition adViewPosition) { }

        public void LoadMREC(AdViewPosition adViewPosition)
        {
            if (this.positionToMRECBannerView.ContainsKey(adViewPosition) || this.positionToMRECToIsLoading.GetOrAdd(adViewPosition, () => false))
            {
                return;
            }

            var mrecBannerView = new BannerView(this.ADMobSettings.MRECAdIds[adViewPosition].Id, AdSize.MediumRectangle, adViewPosition.ToAdMobAdPosition());

            var adRequest = new AdRequest.Builder().Build();

            // send the request to load the ad.
            mrecBannerView.LoadAd(adRequest);
            this.positionToMRECToIsLoading[adViewPosition] = true;
#if UNITY_EDITOR
            HideMrecWhenLoaded();
#endif
            mrecBannerView.OnBannerAdLoaded     += HideMrecWhenLoaded;
            mrecBannerView.OnBannerAdLoadFailed += _ => { this.positionToMRECToIsLoading[adViewPosition] = false; };

            mrecBannerView.OnBannerAdLoaded     += this.BannerViewOnAdLoaded;
            mrecBannerView.OnBannerAdLoadFailed += this.BannerViewOnAdLoadFailed;
            mrecBannerView.OnAdClicked          += this.BannerViewOnAdClicked;
            mrecBannerView.OnAdPaid             += this.MRECAdHandlePaid;

            void HideMrecWhenLoaded()
            {
                mrecBannerView.Hide();
                this.positionToMRECToIsLoading[adViewPosition] = false;
                this.positionToMRECBannerView.Add(adViewPosition, mrecBannerView);
            }
        }

        public bool IsMRECReady(AdViewPosition adViewPosition) { return this.positionToMRECBannerView.ContainsKey(adViewPosition) && !this.positionToMRECToIsLoading[adViewPosition]; }

        public void HideAllMREC()
        {
            foreach (var (adViewPosition, value) in this.positionToMRECBannerView)
            {
                this.HideMREC(adViewPosition);
            }
        }

        private void LoadAllMRec()
        {
            foreach (var (position, _) in this.ADMobSettings.MRECAdIds)
            {
                this.LoadMREC(position);
            }
        }

        private void MrecBannerViewDismissed() { this.signalBus.Fire(new MRecAdDismissedSignal("")); }

        private void MrecBannerViewDisplay() { this.signalBus.Fire(new MRecAdDisplayedSignal("")); }

        private void BannerViewOnAdClicked() { this.signalBus.Fire(new MRecAdClickedSignal("")); }

        private void BannerViewOnAdLoadFailed(LoadAdError obj) { this.signalBus.Fire(new MRecAdLoadFailedSignal("")); }

        private void BannerViewOnAdLoaded() { this.signalBus.Fire(new MRecAdLoadedSignal("")); }

        private void MRECAdHandlePaid(AdValue obj) => this.AdMobHandlePaidEvent(obj, "MREC");

        #endregion

        #region Native Ads

#if ADMOB_NATIVE_ADS
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

        private void AdMobNativePaidHandler(object sender, AdValueEventArgs e) { this.AdMobHandlePaidEvent(e.AdValue, "NativeAds"); }

        private void LoadAllNativeAds()
        {
            foreach (var adId in this.ADMobSettings.NativeAdIds.Select(nativeAdId => nativeAdId.Id))
            {
                this.LoadNativeAds(adId);
            }
        }

#endif

        #endregion

        private void AdMobHandlePaidEvent(AdValue args, string adFormat)
        {
            this.analyticService.Track(new AdsRevenueEvent()
            {
                AdsRevenueSourceId = AdRevenueConstants.ARSourceAdMob,
                Revenue            = args.Value / 1e6,
                Currency           = "USD",
                Placement          = "AOA",
                AdNetwork          = "AdMob",
                AdFormat           = adFormat,
            });

            this.logService.Log($"Received paid event. (currency: {args.CurrencyCode}, value: {args.Value}");
        }
    }
#endif
}