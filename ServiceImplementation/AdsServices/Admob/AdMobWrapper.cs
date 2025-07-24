#if ADMOB
namespace ServiceImplementation.AdsServices.Admob
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using Core.AnalyticServices.Signal;
    using Cysharp.Threading.Tasks;
    using GameFoundation.DI;
    using GameFoundation.Scripts.Utilities.Extension;
    using GameFoundation.Signals;
    using GoogleMobileAds.Api;
    using ServiceImplementation.Configs;
    using ServiceImplementation.Configs.Ads;
    using TheOne.Extensions;
    using TheOne.Logging;
    using UnityEngine;
    using UnityEngine.Scripting;
    using ILogger = TheOne.Logging.ILogger;
    #if ADMOB_NATIVE_ADS && !IMMERSIVE_ADS
    using System.Runtime.Remoting.Channels;
    using Core.AdsServices.Native;
    #endif

    public class AdMobWrapper : IAOAAdService, IMRECAdService, IInitializable
#if ADMOB_NATIVE_ADS && !IMMERSIVE_ADS
    , IAdMobNativeAdsService
#endif
    {
#region inject

        private readonly ILogger                    logService;
        private readonly SignalBus                  signalBus;
        private readonly IReadOnlyList<IAdServices> adServices;
        private readonly IAnalyticServices          analyticService;
        private readonly ThirdPartiesConfig         thirdPartiesConfig;
        private readonly AdServicesConfig           adServicesConfig;

#endregion

        [Preserve]
        public AdMobWrapper
        (
            ILoggerManager           loggerManager,
            SignalBus                signalBus,
            IEnumerable<IAdServices> adServices,
            IAnalyticServices        analyticService,
            ThirdPartiesConfig       thirdPartiesConfig,
            AdServicesConfig         adServicesConfig
        )
        {
            this.logService         = loggerManager.GetLogger(this);
            this.signalBus          = signalBus;
            this.adServices         = adServices.ToArray();
            this.analyticService    = analyticService;
            this.thirdPartiesConfig = thirdPartiesConfig;
            this.adServicesConfig   = adServicesConfig;
        }

        public void Initialize()
        {
            this.VerifySetting();
            this.Init();
        }

        private const string AdPlatForm = AdRevenueConstants.ARSourceAdMob;

        private AdMobSettings ADMobSettings => this.thirdPartiesConfig.AdSettings.AdMob;

        private void VerifySetting()
        {
            //Interstitial
            if (string.IsNullOrEmpty(this.ADMobSettings.DefaultInterstitialAdId.DefaultValue) && this.ADMobSettings.CustomInterstitialAdIds.Values.Contains(this.ADMobSettings.DefaultInterstitialAdId)) throw new RuntimeWrappedException("The default interstitial id is duplicated with custom interstitial Id");
            if (this.ADMobSettings.CustomInterstitialAdIds.GroupBy(x => x.Value).Any(group => group.Count() > 1)) throw new RuntimeWrappedException("There is duplicated interstitial admob ads service");

            //Rewarded ads
            if (string.IsNullOrEmpty(this.ADMobSettings.DefaultRewardedAdId.DefaultValue) && this.ADMobSettings.CustomRewardedAdIds.Values.Contains(this.ADMobSettings.DefaultInterstitialAdId)) throw new RuntimeWrappedException("The default interstitial id is duplicated with custom interstitial Id");
            if (this.ADMobSettings.CustomRewardedAdIds.GroupBy(x => x.Value).Any(group => group.Count() > 1)) throw new RuntimeWrappedException("There is duplicated Rewarded video admob ads service");
        }

        private async void Init()
        {
            await UniTask.SwitchToMainThread();
            #if !GOOGLE_MOBILE_ADS_BELLOW_7_4_0
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
            #endif
            this.logService.Info("AOA start init");
            MobileAds.Initialize(_ =>
                                 {
                                     this.logService.Info("AOA finished init");
                                     this.LoadAppOpenAd();
                                     #if ADMOB_NATIVE_ADS
                                     this.PreloadAllNativeAds();
                                     #endif
                                     #if ADMOB_ADS_DEBUG
                                     MobileAds.OpenAdInspector(_ => {});
                                     #endif
                                 });
        }

        // Temporarily disable this
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void InitAdmob()
        {
            MobileAds.Initialize(_ =>
                                 {
                                 });
        }

        private string GetInterstitialAdsIdByPlace(string place)
        {
            return this.ADMobSettings.CustomInterstitialAdIds.GetValueOrDefault(AdPlacement.PlacementWithName(place), this.ADMobSettings.DefaultInterstitialAdId).DefaultValue;
        }

        private AdPlacement GetPlacement(string placementId) => AdPlacement.PlacementWithName(placementId);

#region AOA

        public bool IsShowingAOAAd { get; set; } = false;

        public float LoadingTimeToShowAOA => this.adServicesConfig.AOALoadingThreshold;

        public bool IsAOAReady()
        {
            return this.aoaAdLoadedInstance.IsAoaAdAvailable && !this.IsShowingAOAAd;
        }

        public void ShowAOAAds(string placement)
        {
            this.aoaAdPlacement = placement;
            this.aoaAdLoadedInstance.Show();
            this.LoadAppOpenAd();
        }

        private LoadedAppOpenAd aoaAdLoadedInstance = new();
        private string aoaAdPlacement;

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
            var adUnitId = this.ADMobSettings.AOAAdId.DefaultValue;

            if (this.aoaAdLoadedInstance is { IsAoaAdAvailable: true })
            {
                this.logService.Info($"AOA ads was already loaded");

                return;
            }

            AppOpenAd.Load(adUnitId, new AdRequest(), LoadAoaCompletedHandler);

            return;

            async void LoadAoaCompletedHandler(AppOpenAd appOpenAd, LoadAdError error)
            {
                if (error != null)
                {
                    // Handle the error.
                    this.logService.Info($"Failed to load the ad. (reason: {error.GetMessage()}), id: {adUnitId}");
                    this.signalBus.Fire(new AppOpenLoadFailedSignal(""));

                    await UniTask.Delay(TimeSpan.FromSeconds(this.currentAOASleepLoadingTime));
                    this.currentAOASleepLoadingTime = Math.Min(this.currentAOASleepLoadingTime * 2, this.maxAOASleepLoadingTime);
                    this.LoadAppOpenAd();

                    return;
                }

                var adRevenueEvent = new AdInfo(AdMobWrapper.AdPlatForm, adUnitId, AdFormatConstants.AppOpen);
                this.signalBus.Fire(new AppOpenLoadedSignal("", adRevenueEvent));
                this.currentAOASleepLoadingTime = this.minAOASleepLoadingTime;

                // App open ad is loaded.
                appOpenAd.OnAdFullScreenContentClosed += this.AOAHandleAdFullScreenContentClosed;
                appOpenAd.OnAdFullScreenContentFailed += this.AOAHandleAdFullScreenContentFailed;
                appOpenAd.OnAdFullScreenContentOpened += this.AOAHandleAdFullScreenContentOpened;
                appOpenAd.OnAdImpressionRecorded      += this.AOAHandleAdImpressionRecorded;
                appOpenAd.OnAdClicked                 += this.AOAHandleAdClicked;
                appOpenAd.OnAdPaid                    += this.AOAHandleAdPaid;

                this.aoaAdLoadedInstance.Init(appOpenAd);
            }
        }

        private void AOAHandleAdClicked()
        {
            this.logService.Info("Clicked app open ad");
            var adRevenueEvent = new AdInfo(AdPlatForm, this.ADMobSettings.AOAAdId.DefaultValue, AdFormatConstants.AppOpen);
            this.signalBus.Fire(new AppOpenClickedSignal(this.aoaAdPlacement, adRevenueEvent));
        }

        private void AOAHandleAdFullScreenContentClosed()
        {
            this.logService.Info("Closed app open ad");
            var adRevenueEvent = new AdInfo(AdPlatForm, this.ADMobSettings.AOAAdId.DefaultValue, AdFormatConstants.AppOpen);
            this.signalBus.Fire(new AppOpenFullScreenContentClosedSignal(this.aoaAdPlacement, adRevenueEvent));
            this.IsShowingAOAAd = false;
        }

        private void AOAHandleAdFullScreenContentFailed(AdError args)
        {
            this.logService.Info($"Failed to present the ad (reason: {args.GetMessage()})");
            this.signalBus.Fire(new AppOpenFullScreenContentFailedSignal(this.aoaAdPlacement, args.GetMessage()));
        }

        private void AOAHandleAdFullScreenContentOpened()
        {
            this.logService.Info("Displayed app open ad");
            var adRevenueEvent = new AdInfo(AdPlatForm, this.ADMobSettings.AOAAdId.DefaultValue, AdFormatConstants.AppOpen);
            this.signalBus.Fire(new AppOpenFullScreenContentOpenedSignal(this.aoaAdPlacement, adRevenueEvent));
            this.IsShowingAOAAd = true;
        }

        private void AOAHandleAdImpressionRecorded()
        {
            this.logService.Info("Recorded ad impression");
        }

        private void AOAHandleAdPaid(AdValue obj) => this.AdMobHandlePaidEvent(obj, this.ADMobSettings.AOAAdId.DefaultValue, AdFormatConstants.AppOpen);

#endregion

#region MREC

        private readonly Dictionary<string, BannerViewHandler> idToMrecViewHandler = new();

        public void ShowMREC(string placement, AdScreenPosition position, AdScreenPosition offset)
        {
            this.LoadAllMRec();
            var adId              = this.ADMobSettings.MRECAdIds[AdPlacement.PlacementWithName(placement)];
            var mrecBannerHandler = this.idToMrecViewHandler[adId.DefaultValue];
            mrecBannerHandler.ShowBanner();
            this.MrecBannerViewDisplay();
        }

        public bool IsMRECReady(string placement, AdScreenPosition position, AdScreenPosition offset)
        {
            this.logService.Info("IsMRECReady start");
            var adPlacement = AdPlacement.PlacementWithName(placement);
            if (!this.ADMobSettings.MRECAdIds.TryGetValue(adPlacement, out var adId)) return false;
            var isMrecHandlerCreate = this.idToMrecViewHandler.ContainsKey(adId.DefaultValue);
            if (!isMrecHandlerCreate)
            {
                this.LoadMREC(placement, position, offset);
            }
            else
            {
                this.UpdatePlacementMrec(adId.DefaultValue, position, offset);
            }
            this.logService.Info("IsMRECReady check banner view is null");
            return this.idToMrecViewHandler[adId.DefaultValue].bannerView != null;
        }

        public void LoadMREC(string placement, AdScreenPosition adPosition, AdScreenPosition offset)
        {
            if (!this.ADMobSettings.MRECAdIds.TryGetValue(AdPlacement.PlacementWithName(placement), out var adId))
            {
                return;
            }

            this.logService.Info("LoadMREC start");
            if (this.idToMrecViewHandler.TryGetValue(adId.DefaultValue, out var bannerViewHandler)) return;
            this.logService.Info("LoadMREC creat new banner");

            var mrecPosition = adPosition.CanvasToUnityCoordinateSystem().ToAdmobPosition() + offset.FlipY();
            bannerViewHandler = new BannerViewHandler(adId.DefaultValue, AdSize.MediumRectangle, (int)mrecPosition.x, (int)mrecPosition.y);
            this.idToMrecViewHandler.Add(adId.DefaultValue, bannerViewHandler);

            bannerViewHandler.bannerView.OnBannerAdLoaded     += this.BannerViewOnAdLoaded;
            bannerViewHandler.bannerView.OnBannerAdLoadFailed += this.BannerViewOnAdLoadFailed;
            bannerViewHandler.bannerView.OnAdClicked          += this.BannerViewOnAdClicked;
            bannerViewHandler.bannerView.OnAdPaid             += this.MRECAdHandlePaid;

            return;
        }

        public void HideMREC(string placement)
        {
            var mrecBannerView = this.idToMrecViewHandler[this.ADMobSettings.MRECAdIds[AdPlacement.PlacementWithName(placement)].DefaultValue];

            if (mrecBannerView.bannerView == null) return;
            mrecBannerView.HideBanner();
            this.MrecBannerViewDismissed();
        }

        public void HideAllMREC()
        {
            this.idToMrecViewHandler.ForEach(x => this.HideMREC(x.Key));
        }

        public void DestroyMREC(string placement)
        {
            var adsId          = this.ADMobSettings.MRECAdIds[AdPlacement.PlacementWithName(placement)].DefaultValue;
            var mrecBannerView = this.idToMrecViewHandler[adsId];
            mrecBannerView.DestroyBanner();
            this.idToMrecViewHandler.Remove(adsId);
            this.MrecBannerViewDismissed();
        }

        private void LoadAllMRec()
        {
            foreach (var (_, mrecBannerHandler) in this.idToMrecViewHandler)
            {
                mrecBannerHandler.CreateBannerIfNeed();
            }
        }

        private void UpdatePlacementMrec(string adId, AdScreenPosition adPosition, AdScreenPosition offset)
        {
            var bannerViewHandler = this.idToMrecViewHandler[adId];
            var mrecPosition      = adPosition.CanvasToUnityCoordinateSystem().ToAdmobPosition() + offset.FlipY();
            bannerViewHandler.UpdatePosition((int)mrecPosition.x, (int)mrecPosition.y);
        }

        private void MrecBannerViewDismissed()
        {
            this.signalBus.Fire(new MRecAdDismissedSignal(""));
        }

        private void MrecBannerViewDisplay()
        {
            this.ADMobSettings.MRECAdIds.Select(mrecAdId => new AdInfo(AdMobWrapper.AdPlatForm, mrecAdId.Value.DefaultValue, AdFormatConstants.MREC))
               .ForEach(adInfo => this.signalBus.Fire(new MRecAdDisplayedSignal("", adInfo)));
        }

        private void BannerViewOnAdClicked()
        {
            this.ADMobSettings.MRECAdIds.Select(mrecAdId => new AdInfo(AdMobWrapper.AdPlatForm, mrecAdId.Value.DefaultValue, AdFormatConstants.MREC))
               .ForEach(adInfo => this.signalBus.Fire(new MRecAdClickedSignal("", adInfo)));
        }

        private void BannerViewOnAdLoadFailed(LoadAdError obj)
        {
            this.logService.Error($"Failed to load ad: {obj.GetMessage()}");
            this.signalBus.Fire(new MRecAdLoadFailedSignal(""));
        }

        private void BannerViewOnAdLoaded()
        {
            this.ADMobSettings.MRECAdIds.Select(mrecAdId => new AdInfo(AdMobWrapper.AdPlatForm, mrecAdId.Value.DefaultValue, AdFormatConstants.MREC))
               .ForEach(adInfo => this.signalBus.Fire(new MRecAdLoadedSignal("", adInfo)));
        }

        private void MRECAdHandlePaid(AdValue obj) => this.AdMobHandlePaidEvent(obj, this.ADMobSettings.MRECAdIds.First().Value.DefaultValue, AdFormatConstants.MREC);

#endregion

#region Native Ads

        #if ADMOB_NATIVE_ADS && !IMMERSIVE_ADS
        private HashSet<string>              loadingNativeAdsId  { get; } = new();
        private Dictionary<string, NativeAd> adsIdToLoadedNativeAds { get; } = new();
        private AdLoadingStrategy            nativeLoadingStrategy      { get; } = new();

        private const string PrefixNativeAdsText = "loading...";

        bool IAdMobNativeAdsService.IsNativeAdsReady(string placement)
        {
            return this.adServicesConfig.EnableNativeAd && this.adsIdToLoadedNativeAds.ContainsKey(this.GetNativeAdsId(placement));
        }

        void IAdMobNativeAdsService.CreateNativeAds(NativeAdsView nativeAdsView)
        {
            if (!this.adServicesConfig.EnableNativeAd) return;
            var isReady = ((IAdMobNativeAdsService)this).IsNativeAdsReady(nativeAdsView.Placement);
            this.logService.Info($"native ads, placement {nativeAdsView.Placement}, is ready: {isReady}");
            if (isReady)
            {
                DrawNativeAds(this.adsIdToLoadedNativeAds[this.GetNativeAdsId(nativeAdsView.Placement)]);
            }

            return;
            void DrawNativeAds(NativeAd nativeAd)
            {
                this.logService.Info($"Start set native ad: {nativeAdsView.name}");

                this.logService.Info($"native star rating : {nativeAd.GetStarRating()}");
                this.logService.Info($"native store: {nativeAd.GetStore()}");
                this.logService.Info($"native Price: {nativeAd.GetPrice()}");
                this.logService.Info($"native advertiser text: {nativeAd.GetAdvertiserText()}");
                this.logService.Info($"native icon: {nativeAd.GetIconTexture()?.texelSize}");

                this.logService.Info($"native headline: {nativeAd.GetHeadlineText()}");
                this.logService.Info($"native call to action text: {nativeAd.GetCallToActionText()}");
                this.logService.Info($"native ad choice: {nativeAd.GetAdChoicesLogoTexture()?.texelSize}");

                // Get Texture2D for icon asset of native ad.
                nativeAdsView.headlineText.text = nativeAd.GetHeadlineText();

                if (!nativeAd.RegisterHeadlineTextGameObject(nativeAdsView.headlineText.gameObject))
                {
                    // Handle failure to register ad asset.
                    this.logService.Info($"Failed to register Headline text for native ad: {nativeAdsView.name}");
                }

                nativeAdsView.advertiserText.text = nativeAd.GetAdvertiserText();

                if (!nativeAd.RegisterAdvertiserTextGameObject(nativeAdsView.advertiserText.gameObject))
                {
                    nativeAdsView.advertiserText.text = PrefixNativeAdsText;

                    // Handle failure to register ad asset.
                    this.logService.Info($"Failed to register advertiser text for native ad: {nativeAdsView.name}");
                }

                nativeAdsView.callToActionText.text = nativeAd.GetCallToActionText();

                if (!nativeAd.RegisterCallToActionGameObject(nativeAdsView.callToActionText.gameObject))
                {
                    nativeAdsView.callToActionText.text = PrefixNativeAdsText;
                    this.logService.Info($"Failed to register call to action text for native ad: {nativeAdsView.name}");
                }

                if (nativeAd.GetIconTexture() != null)
                {
                    nativeAdsView.iconImage.gameObject.SetActive(true);
                    nativeAdsView.iconImage.texture = nativeAd.GetIconTexture();

                    // Register GameObject that will display icon asset of native ad.
                    if (!nativeAd.RegisterIconImageGameObject(nativeAdsView.iconImage.gameObject))
                    {
                        // Handle failure to register ad asset.
                        this.logService.Info($"Failed to register icon image for native ad: {nativeAdsView.name}");
                    }
                }

                if (nativeAdsView.imageTextures.Count > 0 && nativeAd.GetImageTextures() != null)
                {
                    for (var i = 0; i < nativeAdsView.imageTextures.Count; i++)
                    {
                        var rawImage = nativeAdsView.imageTextures[i];
                        rawImage.texture = nativeAd.GetImageTextures()[i];
                    }

                    if (nativeAd.RegisterImageGameObjects(nativeAdsView.imageTextures.Select(x => x.gameObject).ToList()) == 0)
                    {
                        this.logService.Info($"Failed to register list image for native ad: {nativeAdsView.name}");
                    }
                }

                if (nativeAd.GetAdChoicesLogoTexture() != null)
                {
                    nativeAdsView.adChoicesImage.gameObject.SetActive(true);
                    nativeAdsView.adChoicesImage.texture = nativeAd.GetAdChoicesLogoTexture();

                    if (!nativeAd.RegisterAdChoicesLogoGameObject(nativeAdsView.adChoicesImage.gameObject))
                    {
                        // Handle failure to register ad asset.
                        this.logService.Info($"Failed to register ad choices image for native ad: {nativeAdsView.name}");
                    }
                }
            }
        }

        void IAdMobNativeAdsService.ReleaseNativeAds(string placement)
        {
            var adsId = this.GetNativeAdsId(placement);
            if (!this.adsIdToLoadedNativeAds.Remove(adsId, out var nativeAd)) return;
            this.loadingNativeAdsId.Remove(adsId);
            nativeAd.Destroy();
            this.logService.Info($"Release native ad: {placement}");
            this.PreloadNativeAds(placement);
        }

        private string GetNativeAdsId(string placement) => this.ADMobSettings.NativeAdIds[this.GetPlacement(placement)].DefaultValue;

        private void PreloadAllNativeAds()
        {
            foreach (var (placement, _) in this.ADMobSettings.NativeAdIds)
            {
                this.PreloadNativeAds(placement.Name);
            }
        }

        private void PreloadNativeAds(string placement)
        {
            if (!this.adServicesConfig.EnableNativeAd) return;
            if (this.nativeLoadingStrategy.IsWaiting) return;
            var adsId = this.GetNativeAdsId(placement);
            if (this.loadingNativeAdsId.Contains(adsId) || this.adsIdToLoadedNativeAds.ContainsKey(adsId)) return;
            this.logService.Info($"Native ad start preload, placement {placement}");


            var adLoader = new AdLoader.Builder(adsId).ForNativeAd().Build();
            this.loadingNativeAdsId.Add(adsId);

            adLoader.OnNativeAdLoaded += (_, args) =>
                                         {
                                             this.loadingNativeAdsId.Remove(adsId);
                                             this.adsIdToLoadedNativeAds.Add(adsId, args.nativeAd);
                                             this.nativeLoadingStrategy.LoadSucceeded();
                                         };

            adLoader.OnAdFailedToLoad += (_, _) =>
                                         {
                                             this.loadingNativeAdsId.Remove(adsId);
                                             this.nativeLoadingStrategy.LoadFailed();
                                             this.nativeLoadingStrategy.WaitingAsync().ContinueWith(() => this.PreloadNativeAds(placement)).Forget();
                                         };

            adLoader.OnNativeAdLoaded  += (sender, args) => this.HandleNativeAdLoaded(adsId, sender, args);
            adLoader.OnAdFailedToLoad  += (sender, args) => this.HandleAdFailedToLoad(adsId, sender, args);
            adLoader.OnNativeAdClicked += (sender, args) => this.AdLoaderOnOnNativeAdClicked(adsId, sender, args);
            adLoader.LoadAd(new AdRequest());
        }

        private void AdLoaderOnOnNativeAdClicked(string adsId, object sender, EventArgs e)
        {
            this.logService.Info($"native ad clicked: {adsId}");
        }

        private void HandleAdFailedToLoad(string adsId, object sender, AdFailedToLoadEventArgs e)
        {
            this.logService.Info($"Native ad failed to load: {e.LoadAdError.GetMessage()}");
        }

        private void HandleNativeAdLoaded(string adsId, object sender, NativeAdEventArgs e)
        {
            e.nativeAd.OnPaidEvent += (obj, arg) => this.AdMobNativePaidHandler(adsId, obj, arg);
            this.logService.Info($"Native ad loaded successfully");
        }

        private void AdMobNativePaidHandler(string adsId, object sender, AdValueEventArgs e)
        {
            this.AdMobHandlePaidEvent(e.AdValue, adsId, AdFormatConstants.Native);
        }

        #endif

#endregion

        private void AdMobHandlePaidEvent(AdValue args, string adUnitId, string adFormat)
        {
            var adsRevenueEvent = new AdsRevenueEvent
                                  {
                                      AdsRevenueSourceId = AdMobWrapper.AdPlatForm,
                                      AdUnit             = adUnitId,
                                      AdFormat           = adFormat,
                                      AdNetwork          = "AdMob",
                                      Revenue            = args.Value / 1e6,
                                      Currency           = "USD",
                                  };

            this.analyticService.Track(adsRevenueEvent);
            this.signalBus.Fire(new AdRevenueSignal(adsRevenueEvent));
        }
    }

    public class BannerViewHandler
    {
        private readonly string                  adId;
        private readonly AdSize                  adSize;
        private          int                     x;
        private          int                     y;
        private readonly DateTime                lastTimeCreateBanner  = DateTime.Now;
        private readonly TimeSpan                minTimeRecreateBanner = TimeSpan.FromHours(1);
        private          int                     retryTime;
        private          CancellationTokenSource retryLoadCts;
        private          bool                    isHidden;

        internal BannerView bannerView;

        public BannerViewHandler(string adId, AdSize adSize, int x, int y)
        {
            this.adId   = adId;
            this.adSize = adSize;
            this.x      = x;
            this.y      = y;
            this.CreateBannerView();
        }

        private void OnBannerLoadFailed(LoadAdError obj)
        {
            Debug.Log("oneLog: AdMobWrapper: banner load failed");
            this.DestroyBanner();
            UniTask.WhenAll(UniTask.Delay(TimeSpan.FromSeconds(Mathf.Pow(2, this.retryTime++)), DelayType.Realtime), UniTask.WaitUntil(() => !this.isHidden))
                .AttachExternalCancellation((this.retryLoadCts = new()).Token)
                .ContinueWith(this.CreateBannerView)
                .Forget();
        }

        internal void CreateBannerIfNeed()
        {
            if (DateTime.Now - this.lastTimeCreateBanner < this.minTimeRecreateBanner) return;
            this.DestroyBanner();
            this.CreateBannerView();
        }

        private void CreateBannerView()
        {
            this.bannerView = new BannerView(this.adId, this.adSize, this.x, this.y);
            #if !UNITY_EDITOR
            this.bannerView.LoadAd(new AdRequest());
            #endif

            this.retryTime                       =  0;
            this.bannerView.OnBannerAdLoadFailed += this.OnBannerLoadFailed;
        }

        internal void DestroyBanner()
        {
            if (this.bannerView == null) return;
            this.retryLoadCts?.Cancel();
            this.retryLoadCts?.Dispose();
            this.bannerView.OnBannerAdLoadFailed -= this.OnBannerLoadFailed;
            this.bannerView.Destroy();
            this.bannerView = null;
        }

        internal void HideBanner()
        {
            this.isHidden = true;
            this.bannerView.Hide();
        }

        internal void ShowBanner()
        {
            this.isHidden = false;
            this.bannerView.SetPosition(this.x, this.y);
            this.bannerView.Show();
        }

        public void UpdatePosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
#endif