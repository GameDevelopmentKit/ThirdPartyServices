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
    using Core.AnalyticServices.Signal;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Utilities.Extension;
    using GameFoundation.Scripts.Utilities.LogService;
    using GoogleMobileAds.Api;
    using ServiceImplementation.AdsServices.Signal;
    using ServiceImplementation.Configs;
    using ServiceImplementation.Configs.Ads;
    using ServiceImplementation.FireBaseRemoteConfig;
    using UnityEngine;
    using UnityEngine.Scripting;
    using Zenject;
#if ADMOB_NATIVE_ADS && !IMMERSIVE_ADS
    using Core.AdsServices.Native;
#endif

    public class AdMobWrapper : IAOAAdService, IMRECAdService, IInitializable
#if ADMOB_NATIVE_ADS && !IMMERSIVE_ADS
        , INativeAdsService
#endif
    {
        #region inject

        private readonly ILogService                logService;
        private readonly ISignalBus                 signalBus;
        private readonly IReadOnlyList<IAdServices> adServices;
        private readonly IAnalyticServices          analyticService;
        private readonly ThirdPartiesConfig         thirdPartiesConfig;
        private readonly AdServicesConfig           adServicesConfig;
        private readonly IRemoteConfig              remoteConfig;

        #endregion

        public  int    Order          => 0;
        public  bool   IsShowingAOAAd { get; set; }
        private bool   isRemoteConfigFetched;
        private Action onDoneAOA;

        [Preserve]
        public AdMobWrapper
        (
            ILogService logService,
            ISignalBus signalBus,
            IEnumerable<IAdServices> adServices,
            IAnalyticServices analyticService,
            ThirdPartiesConfig thirdPartiesConfig,
            AdServicesConfig adServicesConfig, IRemoteConfig remoteConfig
        )
        {
            this.logService         = logService;
            this.signalBus          = signalBus;
            this.adServices         = adServices.ToArray();
            this.analyticService    = analyticService;
            this.thirdPartiesConfig = thirdPartiesConfig;
            this.adServicesConfig   = adServicesConfig;
            this.remoteConfig       = remoteConfig;
        }

        public void Initialize()
        {
            this.SetTimeoutFirebase();
            this.signalBus.Subscribe<RemoteConfigFetchedSucceededSignal>(this.OnRemoteConfigFetchedSucceeded);
            this.VerifySetting();
            this.Init();
#if ADMOB_NATIVE_ADS && !IMMERSIVE_ADS
            this.IntervalLoadNativeAds();
#endif
        }

        private async void SetTimeoutFirebase()
        {
            await UniTask.WaitForSeconds(5);
            this.isRemoteConfigFetched = true;
        }

        private void OnRemoteConfigFetchedSucceeded(RemoteConfigFetchedSucceededSignal obj) { this.isRemoteConfigFetched = true; }

        private const string AdPlatForm = AdRevenueConstants.ARSourceAdMob;

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
#if ADMOB_ADS_DEBUG
                                     MobileAds.OpenAdInspector(_ => {});
#endif
            });
        }

        // Temporarily disable this
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void InitAdmob() { MobileAds.Initialize(_ => { }); }

        private string GetInterstitialAdsIdByPlace(string place)
        {
            return this.ADMobSettings.CustomInterstitialAdIds.GetValueOrDefault(AdPlacement.PlacementWithName(place), this.ADMobSettings.DefaultInterstitialAdId).Id;
        }

        #region AOA

        public float LoadingTimeToShowAOA => this.adServicesConfig.AOALoadingThreshold;

        public bool IsAOAReady() { return this.aoaAdLoadedInstance.IsAoaAdAvailable && !this.IsShowingAOAAd; }

        public void ShowAOAAds(string placement, Action onDone = null)
        {
            this.onDoneAOA      = onDone;
            this.aoaAdPlacement = placement;
            this.aoaAdLoadedInstance.Show();
            this.LoadAppOpenAd();
        }

        private LoadedAppOpenAd aoaAdLoadedInstance = new();
        private string          aoaAdPlacement;

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
            if (this.adServices.Any(adService => adService.IsRemoveAds())) return;

            var adUnitId = this.ADMobSettings.AOAAdId.Id;

            var testAdsId = this.remoteConfig.GetRemoteConfigStringValue("open_ad_id", string.Empty);

            if (!string.IsNullOrEmpty(testAdsId))
            {
                adUnitId = testAdsId;
                this.logService.Log($"Using test AOA ad id: {adUnitId}");
            }

            if (string.IsNullOrEmpty(adUnitId)) return;

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
            this.logService.Log("Clicked app open ad");
            var adRevenueEvent = new AdInfo(AdPlatForm, this.ADMobSettings.AOAAdId.Id, AdFormatConstants.AppOpen);
            this.signalBus.Fire(new AppOpenClickedSignal(this.aoaAdPlacement, adRevenueEvent));
        }

        private void AOAHandleAdFullScreenContentClosed()
        {
            this.logService.Log("Closed app open ad");
            var adRevenueEvent = new AdInfo(AdPlatForm, this.ADMobSettings.AOAAdId.Id, AdFormatConstants.AppOpen);
            this.signalBus.Fire(new AppOpenFullScreenContentClosedSignal(this.aoaAdPlacement, adRevenueEvent));
            this.IsShowingAOAAd = false;
            this.onDoneAOA?.Invoke();
            this.onDoneAOA = null;
        }

        private void AOAHandleAdFullScreenContentFailed(AdError args)
        {
            this.logService.Log($"Failed to present the ad (reason: {args.GetMessage()})");
            this.signalBus.Fire(new AppOpenFullScreenContentFailedSignal(this.aoaAdPlacement, args.GetMessage()));
        }

        private void AOAHandleAdFullScreenContentOpened()
        {
            this.logService.Log("Displayed app open ad");
            var adRevenueEvent = new AdInfo(AdPlatForm, this.ADMobSettings.AOAAdId.Id, AdFormatConstants.AppOpen);
            this.signalBus.Fire(new AppOpenFullScreenContentOpenedSignal(this.aoaAdPlacement, adRevenueEvent));
            this.IsShowingAOAAd = true;
        }

        private void AOAHandleAdImpressionRecorded() { this.logService.Log("Recorded ad impression"); }

        private void AOAHandleAdPaid(AdValue obj) => this.AdMobHandlePaidEvent(obj, this.ADMobSettings.AOAAdId.Id, AdFormatConstants.AppOpen);

        #endregion

        #region MREC

        private readonly Dictionary<string, BannerViewHandler> idToMrecViewHandler = new();

        public void ShowMREC(string placement, AdScreenPosition position, AdScreenPosition offset)
        {
            this.LoadAllMRec();
            var adId              = this.ADMobSettings.MRECAdIds[AdPlacement.PlacementWithName(placement)];
            var mrecBannerHandler = this.idToMrecViewHandler[adId.Id];
            var mrecPosition      = position.CanvasToUnityCoordinateSystem().ToAdmobPosition() + offset.FlipY();
            mrecBannerHandler.bannerView.SetPosition((int)mrecPosition.x, (int)mrecPosition.y);
            mrecBannerHandler.bannerView.Show();

            this.MrecBannerViewDisplay();
        }

        public bool IsMRECReady(string placement, AdScreenPosition position)
        {
            var adPlacement = AdPlacement.PlacementWithName(placement);

            if (this.ADMobSettings.MRECAdIds.Count == 0)
            {
                return false;
            }

            var adsId = this.ADMobSettings.MRECAdIds.First().Value.Id;

            if (this.ADMobSettings.MRECAdIds.TryGetValue(adPlacement, out var adId))
            {
                adsId = adId.Id;
            }

            var isMrecHandlerCreate = this.idToMrecViewHandler.ContainsKey(adsId);

            if (!isMrecHandlerCreate)
            {
                this.LoadMREC(placement, position);
            }

            return this.idToMrecViewHandler[adsId].bannerView != null;
        }

        public void LoadMREC(string placement, AdScreenPosition adPosition)
        {
            if (!this.ADMobSettings.MRECAdIds.TryGetValue(AdPlacement.PlacementWithName(placement), out var adId))
            {
                return;
            }

            if (this.idToMrecViewHandler.TryGetValue(adId.Id, out var bannerViewHandler)) return;

            var mrecPosition = adPosition.CanvasToUnityCoordinateSystem().ToAdmobPosition();
            bannerViewHandler = new BannerViewHandler(adId.Id, AdSize.MediumRectangle, (int)mrecPosition.x, (int)mrecPosition.y);
            this.idToMrecViewHandler.Add(adId.Id, bannerViewHandler);

            bannerViewHandler.bannerView.OnBannerAdLoaded     += OnMrecBannerLoaded;
            bannerViewHandler.bannerView.OnBannerAdLoadFailed += OnMrecBannerLoadFailed;

            bannerViewHandler.bannerView.OnBannerAdLoaded     += this.BannerViewOnAdLoaded;
            bannerViewHandler.bannerView.OnBannerAdLoadFailed += this.BannerViewOnAdLoadFailed;
            bannerViewHandler.bannerView.OnAdClicked          += this.BannerViewOnAdClicked;
            bannerViewHandler.bannerView.OnAdPaid             += this.MRECAdHandlePaid;

            return;

            void OnMrecBannerLoaded() { Debug.Log("mrec loaded"); }

            void OnMrecBannerLoadFailed(LoadAdError _) { Debug.Log("mrec load failed"); }
        }

        public void HideMREC(string placement, AdScreenPosition position)
        {
            var mrecBannerView = this.idToMrecViewHandler[this.ADMobSettings.MRECAdIds[AdPlacement.PlacementWithName(placement)].Id];

            if (mrecBannerView.bannerView == null) return;
            mrecBannerView.bannerView.Hide();
            this.MrecBannerViewDismissed();
        }

        public void HideAllMREC() { }

        private void LoadAllMRec()
        {
            foreach (var (_, mrecBannerHandler) in this.idToMrecViewHandler)
            {
                mrecBannerHandler.CreatBannerIfNeed();
            }
        }

        private void MrecBannerViewDismissed() { this.signalBus.Fire(new MRecAdDismissedSignal("")); }

        private void MrecBannerViewDisplay()
        {
            this.ADMobSettings.MRECAdIds.Select(mrecAdId => new AdInfo(AdMobWrapper.AdPlatForm, mrecAdId.Value.Id, AdFormatConstants.MREC))
                .ForEach(adInfo => this.signalBus.Fire(new MRecAdDisplayedSignal("", adInfo)));
        }

        private void BannerViewOnAdClicked()
        {
            this.ADMobSettings.MRECAdIds.Select(mrecAdId => new AdInfo(AdMobWrapper.AdPlatForm, mrecAdId.Value.Id, AdFormatConstants.MREC))
                .ForEach(adInfo => this.signalBus.Fire(new MRecAdClickedSignal("", adInfo)));
        }

        private void BannerViewOnAdLoadFailed(LoadAdError obj)
        {
            Debug.LogError($"AdmobWrapper Failed to load ad: {obj.GetMessage()}");
            this.signalBus.Fire(new MRecAdLoadFailedSignal(""));
        }

        private void BannerViewOnAdLoaded()
        {
            this.ADMobSettings.MRECAdIds.Select(mrecAdId => new AdInfo(AdMobWrapper.AdPlatForm, mrecAdId.Value.Id, AdFormatConstants.MREC))
                .ForEach(adInfo => this.signalBus.Fire(new MRecAdLoadedSignal("", adInfo)));
        }

        private void MRECAdHandlePaid(AdValue obj) => this.AdMobHandlePaidEvent(obj, this.ADMobSettings.MRECAdIds.First().Value.Id, AdFormatConstants.MREC);

        #endregion

        #region Native Ads

#if ADMOB_NATIVE_ADS && !IMMERSIVE_ADS

        private const string PrefixNativeAdsText    = "loading...";
        private const int    MaxNativeAdsPerRequest = 3;

        private readonly Dictionary<string, List<NativeAdInstanceWrapper>> nativeAdsPool = new();
        private readonly Dictionary<string, int>                           loadingCount  = new();
        private readonly Dictionary<NativeAdsView, NativeAd>               viewToAdMap   = new();

        private async void IntervalLoadNativeAds()
        {
            if (!this.isRemoteConfigFetched)
            {
                await UniTask.WaitUntil(() => this.isRemoteConfigFetched);
            }

            while (this.adServicesConfig.EnableNativeAd && this.adServicesConfig.EnableAds)
            {
                this.LoadAllNativeAds();
                await UniTask.Delay(TimeSpan.FromSeconds(this.adServicesConfig.NativeAdLoadInterval));
            }
        }

        private void LoadAllNativeAds()
        {
            foreach (var adId in this.ADMobSettings.NativeAdIds.Select(n => n.Id))
            {
                this.EnsureAdListExists(adId);

                var currentCount = this.nativeAdsPool[adId].Count;
                var loading      = this.loadingCount.GetValueOrDefault(adId, 0);
                var needToLoad   = this.adServicesConfig.NativeAdCount - currentCount - loading;
                this.logService.Log($"[NativeAd] {adId} - Loaded: {currentCount}, loading: {loading}, needToLoad: {needToLoad}");

                if (needToLoad < 0) continue;

                var toLoad = Math.Min(MaxNativeAdsPerRequest, needToLoad);
                this.loadingCount[adId] = loading + toLoad;

                for (var i = 0; i < toLoad; i++)
                {
                    this.LoadNativeAd(adId);
                }
            }
        }

        private void LoadNativeAd(string adId)
        {
            this.logService.Log($"Start loading native ad: {adId}");

            var adLoader = new AdLoader.Builder(adId).ForNativeAd().Build();

            adLoader.OnNativeAdLoaded += (_, args) => this.OnNativeAdLoaded(adId, args.nativeAd);
            adLoader.OnAdFailedToLoad += (_, _) => this.OnNativeAdFailed(adId);

            adLoader.OnNativeAdLoaded  += this.HandleNativeAdLoaded;
            adLoader.OnAdFailedToLoad  += this.HandleAdFailedToLoad;
            adLoader.OnNativeAdClicked += this.OnNativeAdClicked;
            adLoader.OnNativeAdClosed  += this.OnNativeAdClosed;

#if ADMOB_BELLOW_9_0_0
            adLoader.LoadAd(new AdRequest.Builder().Build());
#else
            adLoader.LoadAd(new AdRequest());
#endif

#if UNITY_EDITOR
            this.EnsureAdListExists(adId);
            this.nativeAdsPool[adId].Add(new NativeAdInstanceWrapper { NativeAdInstance = null });
            this.DecreaseLoadingCount(adId);
#endif
        }

        private void OnNativeAdLoaded(string adId, NativeAd nativeAd)
        {
            this.EnsureAdListExists(adId);
            this.nativeAdsPool[adId].Add(new NativeAdInstanceWrapper { NativeAdInstance = nativeAd });

            this.DecreaseLoadingCount(adId);
            this.logService.Log($"Native ad loaded: {adId} - Total: {this.nativeAdsPool[adId].Count}");
        }

        private void OnNativeAdFailed(string adId)
        {
            this.DecreaseLoadingCount(adId);
            this.logService.Log($"Native ad failed to load: {adId}");
        }

        private void DecreaseLoadingCount(string adId)
        {
            if (this.loadingCount.ContainsKey(adId))
            {
                this.loadingCount[adId] = Math.Max(0, this.loadingCount[adId] - 1);
            }
        }

        private void EnsureAdListExists(string adId)
        {
            if (!this.nativeAdsPool.ContainsKey(adId))
            {
                this.nativeAdsPool[adId] = new List<NativeAdInstanceWrapper>();
            }
        }

        public List<NativeAdInstanceWrapper> GetNativeAds(string adId = "")
        {
#if CREATIVE && !FORCE_ADS
            return new List<NativeAdInstanceWrapper>();
#endif
            if (this.nativeAdsPool.Count == 0) return new List<NativeAdInstanceWrapper>();

            return string.IsNullOrEmpty(adId)
                ? this.nativeAdsPool.FirstOrDefault().Value ?? new List<NativeAdInstanceWrapper>()
                : this.nativeAdsPool.TryGetValue(adId, out var ads)
                    ? ads
                    : new List<NativeAdInstanceWrapper>();
        }

        public void RemoveNativeAd(NativeAdInstanceWrapper nativeAd)
        {
            var entry = this.nativeAdsPool.FirstOrDefault(kv => kv.Value.Contains(nativeAd));

            if (string.IsNullOrEmpty(entry.Key)) return;

            entry.Value.Remove(nativeAd);
            this.logService.Log($"Removed native ad from: {entry.Key}, Remaining: {entry.Value.Count}");
        }

        public void DrawNativeAds(NativeAdsView view)
        {
            if (!this.adServicesConfig.EnableNativeAd || this.viewToAdMap.ContainsKey(view)) return;

            var availableAds = this.GetNativeAds();

            if (availableAds.Count == 0) return;

            var adWrapper = availableAds.FirstOrDefault();

            if (adWrapper?.NativeAdInstance is not NativeAd nativeAd) return;

            var key = this.nativeAdsPool.FirstOrDefault(x => x.Value.Contains(adWrapper)).Key;

            if (!string.IsNullOrEmpty(key))
            {
                this.nativeAdsPool[key].Remove(adWrapper);
            }

            this.viewToAdMap.TryAdd(view, nativeAd);
            this.logService.Log($"Set native ad for view: {view.name}");

            this.SetupNativeView(view, nativeAd);
        }

        private void SetupNativeView(NativeAdsView view, NativeAd ad)
        {
            view.headlineText.text = ad.GetHeadlineText();

            if (!ad.RegisterHeadlineTextGameObject(view.headlineText.gameObject))
                this.logService.Log($"Failed to register headline for: {view.name}");

            view.advertiserText.text = ad.GetAdvertiserText();

            if (!ad.RegisterAdvertiserTextGameObject(view.advertiserText.gameObject))
            {
                view.advertiserText.text = PrefixNativeAdsText;
                this.logService.Log($"Failed to register advertiser for: {view.name}");
            }

            view.callToActionText.text = ad.GetCallToActionText();

            if (!ad.RegisterCallToActionGameObject(view.callToActionText.gameObject))
            {
                view.callToActionText.text = PrefixNativeAdsText;
                this.logService.Log($"Failed to register CTA for: {view.name}");
            }

            var icon = ad.GetIconTexture();

            if (icon != null)
            {
                view.iconImage.gameObject.SetActive(true);
                view.iconImage.texture = icon;

                if (!ad.RegisterIconImageGameObject(view.iconImage.gameObject))
                    this.logService.Log($"Failed to register icon for: {view.name}");
            }

            var adChoices = ad.GetAdChoicesLogoTexture();

            if (adChoices != null)
            {
                view.adChoicesImage.gameObject.SetActive(true);
                view.adChoicesImage.texture = adChoices;

                if (!ad.RegisterAdChoicesLogoGameObject(view.adChoicesImage.gameObject))
                    this.logService.Log($"Failed to register ad choices for: {view.name}");
            }
        }

        // Event Handlers
        private void OnNativeAdClosed(object sender, EventArgs e)
        {
            this.signalBus.Fire(new NativeAdCloseSignal());
            this.logService.Log("Native ad closed");
        }

        private void OnNativeAdClicked(object sender, EventArgs e)
        {
            this.signalBus.Fire(new NativeAdClickSignal());
            this.logService.Log("Native ad clicked");
        }

        private void HandleAdFailedToLoad(object sender, AdFailedToLoadEventArgs e) { this.logService.Log($"Native ad failed: {e.LoadAdError.GetMessage()}"); }

        private void HandleNativeAdLoaded(object sender, NativeAdEventArgs e)
        {
            e.nativeAd.OnPaidEvent += HandleAdPaidEvent;
            this.logService.Log("Native ad loaded and registered for paid event");
        }

        private void HandleAdPaidEvent(object sender, AdValueEventArgs e) { this.AdMobHandlePaidEvent(e.AdValue, this.ADMobSettings.NativeAdIds.First().Id, AdFormatConstants.Native); }

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
        private readonly string   adId;
        private readonly AdSize   adSize;
        private readonly int      x;
        private readonly int      y;
        private readonly DateTime lastTimeCreateBanner  = DateTime.Now;
        private readonly TimeSpan minTimeRecreateBanner = TimeSpan.FromHours(1);
        private          int      loadFailedTime;

        internal BannerView bannerView;

        public BannerViewHandler(string adId, AdSize adSize, int x, int y)
        {
            this.adId   = adId;
            this.adSize = adSize;
            this.x      = x;
            this.y      = y;
            this.CreateBannerView();
        }

        private void CreateBannerView()
        {
            this.bannerView = new BannerView(this.adId, this.adSize, this.x, this.y);
#if !UNITY_EDITOR
            this.bannerView.LoadAd(new AdRequest());
#endif

            this.bannerView.OnBannerAdLoaded     += this.OnBannerLoaded;
            this.bannerView.OnBannerAdLoadFailed += this.OnBannerLoadFailed;
        }

        internal void CreatBannerIfNeed()
        {
            if (DateTime.Now - this.lastTimeCreateBanner < this.minTimeRecreateBanner) return;
            this.DestroyBanner();
            this.CreateBannerView();
        }

        private void OnBannerLoaded() { this.loadFailedTime = 0; }

        private void DestroyBanner()
        {
            if (this.bannerView == null) return;
            this.bannerView.Destroy();
            this.bannerView = null;
        }

        private async void OnBannerLoadFailed(LoadAdError obj)
        {
            this.loadFailedTime += 1;
            this.DestroyBanner();
            await UniTask.Delay(TimeSpan.FromSeconds(Mathf.Pow(2, this.loadFailedTime)), DelayType.Realtime);
            this.CreateBannerView();
        }
    }
#endif
}