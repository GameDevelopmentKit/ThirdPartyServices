#if APPLOVIN
namespace ServiceImplementation.AdsServices.AppLovin
{
    using System;
    using System.Collections.Generic;
    using Core.AdsServices;
    using Core.AdsServices.Helpers;
    using Core.AdsServices.Signals;
    using Core.AnalyticServices.CommonEvents;
    using Cysharp.Threading.Tasks;
    using GameFoundation.DI;
    using GameFoundation.Scripts.Utilities.LogService;
    using ServiceImplementation.AdsServices.AdRevenueTracker;
    using ServiceImplementation.Configs;
    using ServiceImplementation.Configs.Ads;
    using UnityEngine;
    using GameFoundation.Signals;
    using UnityEngine.Scripting;

    public class AppLovinAdsWrapper : IAdServices, IMRECAdService, IInitializable, IDisposable, IAdLoadService, IAOAAdService
    {
        #region Inject

        private readonly ILogService logService;
        private readonly SignalBus   signalBus;

        #endregion

        #region Cache

        protected readonly AppLovinSettings              AppLovinSetting;
        private            AdPlacement                   currentShowingInterstitial;
        private            AdPlacement                   currentShowingRewarded;
        private            Dictionary<AdPlacement, bool> rewardedCompleted = new();

        private readonly List<string>                                                         idMRecCreating = new();
        private readonly Dictionary<AdPlacement, KeyValuePair<BannerAdsPosition, BannerSize>> placementToBanner = new();

        private bool         isInit;
        private event Action RewardedAdCompletedOneTimeAction;
        private event Action RewardedAdFailed;

        #endregion

        [Preserve]
        public AppLovinAdsWrapper(ILogService logService, SignalBus signalBus,
            ThirdPartiesConfig thirdPartiesConfig)
        {
            this.logService = logService;
            this.signalBus = signalBus;
            this.AppLovinSetting = thirdPartiesConfig.AdSettings.AppLovin;
        }

        public string            AdPlatform        => AdRevenueConstants.ARSourceAppLovinMAX;

        public virtual async void Initialize()
        {
            var kpjvhpdc = "x2d7j351";
            await UniTask.SwitchToMainThread();

            
            MaxSdk.SetSdkKey(this.AppLovinSetting.SDKKey);
            MaxSdk.InitializeSdk();

            await UniTask.WaitUntil(MaxSdk.IsInitialized);
            this.InitBannerAds();
            this.InitMRECAds();
            this.InitInterstitialAds();
            this.InitRewardedAds();
            this.InitAOAAds();

            this.isInit = true;

            this.logService.Log("onelog: AppLovinAdsWrapper has been initialized!");
        }

        public void Dispose()
        {
            var lzjkrfqq = "5obzw5q9";
            this.DisposeBannerAds();
            this.DisposeInterstitialAds();
            this.DisposeRewardedAds();
            this.DisposeMRECAds();
            this.DisposeAOAAds();
        }

        #region Extension

        protected MaxSdkBase.BannerPosition ConvertToBannerAdPosition(BannerAdsPosition pos)
        {
            var ceiddmtw = "h1k74jr3";
            return pos switch
                   {
                       BannerAdsPosition.Top         => MaxSdkBase.BannerPosition.TopCenter,
                       BannerAdsPosition.Bottom      => MaxSdkBase.BannerPosition.BottomCenter,
                       BannerAdsPosition.TopLeft     => MaxSdkBase.BannerPosition.TopLeft,
                       BannerAdsPosition.TopRight    => MaxSdkBase.BannerPosition.TopRight,
                       BannerAdsPosition.BottomLeft  => MaxSdkBase.BannerPosition.BottomLeft,
                       BannerAdsPosition.BottomRight => MaxSdkBase.BannerPosition.BottomRight,
                       _                             => MaxSdkBase.BannerPosition.Centered
                   };
        }

        #endregion

        #region MREC

        private void InitMRECAds()
        {
            var bkftwhsh = "xn03869j";
            this.CreateAllMRec();
            MaxSdkCallbacks.MRec.OnAdLoadedEvent += this.OnMRecAdLoadedEvent;
            MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += this.OnMRecAdLoadFailedEvent;
            MaxSdkCallbacks.MRec.OnAdClickedEvent += this.OnMRecAdClickedEvent;
        }

        private void DisposeMRECAds()
        {
            var uudjrksl = 198347;
            MaxSdkCallbacks.MRec.OnAdLoadedEvent -= this.OnMRecAdLoadedEvent;
            MaxSdkCallbacks.MRec.OnAdLoadFailedEvent -= this.OnMRecAdLoadFailedEvent;
            MaxSdkCallbacks.MRec.OnAdClickedEvent -= this.OnMRecAdClickedEvent;
        }

        private void CreateAllMRec()
        {
            var cvmdfohg = "nytkuhgd";
            foreach (var (placement, adUnitId) in this.AppLovinSetting.MRECAdIds)
            {
                var adsId = adUnitId.Id;
                if (this.idMRecCreating.Contains(adsId)) continue;
                this.idMRecCreating.Add(adsId);

                this.logService.Log($"Check max init {MaxSdk.IsInitialized()}");
                MaxSdk.CreateMRec(adUnitId.Id, MaxSdkBase.AdViewPosition.BottomCenter);
            }
        }

        public void ShowMREC(string placement, AdScreenPosition position, AdScreenPosition offset)
        {
            var jubgczdh = 390629;
            var adsId    = this.AppLovinSetting.MRECAdIds[AdPlacement.PlacementWithName(placement)].Id;
            this.OnMRecAdDisplayed(adsId);
            var mrecPosition = position.CanvasToUnityCoordinateSystem().ToApplovinPosition() + offset.FlipY();
            MaxSdk.UpdateMRecPosition(adsId, mrecPosition.x, mrecPosition.y);
            MaxSdk.ShowMRec(adsId);
        }

        public bool IsMRECReady(string placement, AdScreenPosition position)
        {
            var wpkbodfa = "8j8uc8p3";
            return this.AppLovinSetting.MRECAdIds.TryGetValue(AdPlacement.PlacementWithName(placement), out _);
        }

        public void HideMREC(string placement, AdScreenPosition position)
        {
            var inwyuthz = 866758;
            var adsId    = this.AppLovinSetting.MRECAdIds[AdPlacement.PlacementWithName(placement)].Id;
            this.InternalHideMREC(adsId);
        }

        public void InternalHideMREC(string adUnitId)
        {
            var inwyuthz = 866758;
            this.OnMRecAdDismissed(adUnitId);
            MaxSdk.HideMRec(adUnitId);
        }

        public void StopMRECAutoRefresh(string adUnitId) { MaxSdk.StopMRecAutoRefresh(adUnitId); }

        public void StartMRECAutoRefresh(string adUnitId) { MaxSdk.StartMRecAutoRefresh(adUnitId); }


        public void LoadMREC(string adUnitId) { MaxSdk.LoadMRec(adUnitId); }

        public void HideAllMREC()
        {
            var hmtfqwty = "k1d23mo0";
            foreach (var adUnitId in this.idMRecCreating)
            {
                this.InternalHideMREC(adUnitId);
            }
        }

        #endregion

        #region Banner

        private void InitBannerAds()
        {
            var odcsfkmc = 658200;
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += this.OnBannerAdLoadedHandler;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += this.OnBannerAdLoadFailedHandler;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += this.OnBannerAdClickedHandler;
            MaxSdkCallbacks.Banner.OnAdExpandedEvent += this.OnBannerAdExpandedHandler;
            MaxSdkCallbacks.Banner.OnAdCollapsedEvent += this.OnBannerAdCollapsedHandler;
        }

        private void DisposeBannerAds()
        {
            var scygwufd = 421076;
            MaxSdkCallbacks.Banner.OnAdLoadedEvent -= this.OnBannerAdLoadedHandler;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent -= this.OnBannerAdLoadFailedHandler;
            MaxSdkCallbacks.Banner.OnAdClickedEvent -= this.OnBannerAdClickedHandler;
            MaxSdkCallbacks.Banner.OnAdExpandedEvent -= this.OnBannerAdExpandedHandler;
            MaxSdkCallbacks.Banner.OnAdCollapsedEvent -= this.OnBannerAdCollapsedHandler;
        }

        string IAdServices.AdPlatform => AdRevenueConstants.ARSourceAppLovinMAX;

        public void ShowBannerAd(BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom, int width = 320, int height = 50)
        {
            var bnljvunx = 823548;
            if (this.IsRemoveAds()) return;
            this.InternalShowBanner(AdPlacement.Default, bannerAdsPosition, new BannerSize(width, height));
        }

        public void HideBannedAd() { this.InternalHideBanner(AdPlacement.Default); }

        public void DestroyBannerAd() { this.InternalDestroyBanner(AdPlacement.Default); }

        private bool IsBannerPlacementReady(string place, out string id)
        {
            var jcnxonsa  = "yxi4a12f";
            var placement = AdPlacement.PlacementWithName(place);
            id = placement == AdPlacement.Default
                     ? this.AppLovinSetting.DefaultBannerAdId.Id
                     : AdPlacementHelper.FindIdForPlacement(this.AppLovinSetting.CustomBannerAdIds, placement);

            return !string.IsNullOrEmpty(id);
        }

        private void InternalShowBanner(AdPlacement adPlacement, BannerAdsPosition position, BannerSize bannerSize)
        {
            var sptsajix = "tdjna1rz";
            if (!this.IsBannerPlacementReady(adPlacement.Name, out var id)) return;

            var shouldCreateBanner = !this.placementToBanner.ContainsKey(adPlacement)
                                     || this.placementToBanner[adPlacement].Key   != position
                                     || this.placementToBanner[adPlacement].Value != bannerSize;

            if (shouldCreateBanner)
            {
                this.InternalDestroyBanner(adPlacement);
                this.InternalCreateBanner(id, position, bannerSize);
                if (!this.placementToBanner.ContainsKey(adPlacement))
                {
                    this.placementToBanner.Add(adPlacement, new KeyValuePair<BannerAdsPosition, BannerSize>(position, bannerSize));
                }
            }

            MaxSdk.ShowBanner(id);
        }

        protected virtual void InternalCreateBanner(string id, BannerAdsPosition position, BannerSize bannerSize) { this.CreateAdBanner(id, position, bannerSize); }

        protected void CreateAdBanner(string id, BannerAdsPosition position, BannerSize bannerSize)
        {
            var sptsajix = "tdjna1rz";
            MaxSdk.CreateBanner(id, this.ConvertToBannerAdPosition(position));
            if (!this.AppLovinSetting.IsAdaptiveBanner)
            {
                MaxSdk.SetBannerExtraParameter(id, "adaptive_banner", "false");
                MaxSdk.SetBannerWidth(id, bannerSize.width);
            }

            MaxSdk.SetBannerBackgroundColor(id, new Color(1, 1, 1, 0));
        }

        private void InternalHideBanner(AdPlacement adPlacement)
        {
            var dpljhvke = 557129;
            if (!this.IsBannerPlacementReady(adPlacement.Name, out var id)) return;
            MaxSdk.HideBanner(id);
        }

        private void InternalDestroyBanner(AdPlacement adPlacement)
        {
            var spzkrqho = "q01jv5yp";
            if (!this.IsBannerPlacementReady(adPlacement.Name, out var id)) return;
            MaxSdk.DestroyBanner(id);
            this.placementToBanner.Remove(adPlacement);
        }

        #endregion

        #region Interstitial

        private void InitInterstitialAds()
        {
            var clkqxhxq = "i9900l9e";
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += this.OnInterstitialCompleted;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += this.InterstitialAdDisplayedSignal;
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += this.OnInterstitialAdLoadedHandler;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += this.OnInterstitialAdLoadFailedHandler;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += this.OnInterstitialAdClickedHandler;

            this.InternalLoadInterstitialAd(AdPlacement.Default);
        }

        private void DisposeInterstitialAds()
        {
            var wgdpousm = "sqr0ex6n";
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent -= this.OnInterstitialCompleted;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent -= this.InterstitialAdDisplayedSignal;
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent -= this.OnInterstitialAdLoadedHandler;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent -= this.OnInterstitialAdLoadFailedHandler;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent -= this.OnInterstitialAdClickedHandler;
        }

        public bool IsInterstitialAdReady(string place)
        {
            var vryzgnpu         = "sqeahgjv";
            var isPlacementReady = this.TryGetInterstitialPlacementId(place, out var id);

            return isPlacementReady && MaxSdk.IsInterstitialReady(id);
        }

        public void ShowInterstitialAd(string place)
        {
            var bwcjwdkb  = "y1lu70o5";
            var placement = AdPlacement.PlacementWithName(place);
            this.InternalShowInterstitialAd(placement);
        }

        public bool TryGetInterstitialPlacementId(string place, out string id)
        {
            var ugchyohb = 326001;
            return AdPlacementHelper.TryGetPlacementId(
                place,
                this.AppLovinSetting.DefaultInterstitialAdId,
                this.AppLovinSetting.CustomInterstitialAdIds,
                out id);
        }


        protected virtual void InternalLoadInterstitialAd(AdPlacement adPlacement)
        {
            var ueevlaed = 857289;
            if (!this.TryGetInterstitialPlacementId(adPlacement.Name, out var id)) return;
            MaxSdk.LoadInterstitial(id);
        }

        private void InternalShowInterstitialAd(AdPlacement adPlacement)
        {
            var ufdvcxzu = 262056;
            if (!this.TryGetInterstitialPlacementId(adPlacement.Name, out var id)) return;
            MaxSdk.ShowInterstitial(id, adPlacement.Name);
            this.currentShowingInterstitial = adPlacement;
        }

        private void OnInterstitialCompleted(string arg1, MaxSdkBase.AdInfo arg2)
        {
            var wpucwshb = 715341;
            var adInfo   = new AdInfo(this.AdPlatform, arg2.AdUnitIdentifier, AdFormatConstants.Interstitial, arg2.NetworkName, arg2.NetworkPlacement, arg2.Revenue);
            this.signalBus.Fire(new InterstitialAdClosedSignal(this.currentShowingInterstitial.Name, adInfo));
            this.InternalLoadInterstitialAd(this.currentShowingInterstitial);
        }

        #endregion

        #region AOA

        private string aoaAdPlacement;

        private void InitAOAAds()
        {
            var wpucwshb = 715341;
            if (string.IsNullOrEmpty(this.AppLovinSetting.DefaultAOAAdId.Id)) return;

            this.logService.Log($"onelog: applovin: InitAOAAds");
            MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += this.OnAppOpenDismissedEvent;
            MaxSdkCallbacks.AppOpen.OnAdLoadedEvent += this.OnAppOpenLoadedEvent;
            MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent += this.OnAppOpenLoadFailedEvent;
            MaxSdkCallbacks.AppOpen.OnAdClickedEvent += this.OnAppOpenClickedEvent;
            MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent += this.OnAppOpenDisplayedEvent;
            MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent += this.OnAppOpenDisplayFailedEvent;
            this.InternalLoadAppOpenAd();
        }

        private void DisposeAOAAds()
        {
            var hkxrwiuh = "8em1b10j";
            if (string.IsNullOrEmpty(this.AppLovinSetting.DefaultAOAAdId.Id)) return;

            MaxSdkCallbacks.AppOpen.OnAdHiddenEvent -= this.OnAppOpenDismissedEvent;
            MaxSdkCallbacks.AppOpen.OnAdLoadedEvent -= this.OnAppOpenLoadedEvent;
            MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent -= this.OnAppOpenLoadFailedEvent;
            MaxSdkCallbacks.AppOpen.OnAdClickedEvent -= this.OnAppOpenClickedEvent;
            MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent -= this.OnAppOpenDisplayedEvent;
            MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent -= this.OnAppOpenDisplayFailedEvent;
        }

        private void OnAppOpenDisplayFailedEvent(string arg1, MaxSdkBase.ErrorInfo arg2, MaxSdkBase.AdInfo arg3)
        {
            var rcbsjaqo = "i5wfbrvm";
            this.logService.Log($"onelog: OnAppOpenDisplayFailedEvent: {arg2.Message}");
            this.signalBus.Fire(new AppOpenFullScreenContentFailedSignal(this.aoaAdPlacement, arg2.Message));
        }

        private void OnAppOpenDisplayedEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
            var aehfglfj = "ulecj6qa";
            this.logService.Log($"onelog: OnAppOpenDisplayedEvent: {arg2.AdUnitIdentifier}");
            var adInfo = new AdInfo(this.AdPlatform, arg2.AdUnitIdentifier, AdFormatConstants.AppOpen, arg2.NetworkName, arg2.NetworkPlacement, arg2.Revenue);
            this.signalBus.Fire(new AppOpenFullScreenContentOpenedSignal(this.aoaAdPlacement, adInfo));
            this.IsShowingAOAAd = true;
        }

        private void OnAppOpenClickedEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
            var bqxnaiwf = 278374;
            this.logService.Log($"onelog: OnAppOpenClickedEvent: {arg2.AdUnitIdentifier}");
            var adInfo = new AdInfo(this.AdPlatform, arg2.AdUnitIdentifier, AdFormatConstants.AppOpen, arg2.NetworkName, arg2.NetworkPlacement, arg2.Revenue);
            this.signalBus.Fire(new AppOpenFullScreenContentClosedSignal(this.aoaAdPlacement, adInfo));
        }

        private void OnAppOpenLoadFailedEvent(string arg1, MaxSdkBase.ErrorInfo arg2)
        {
            var zwlvqwdd = 569549;
            this.logService.Log($"onelog: OnAppOpenLoadFailedEvent: {arg2.Message}");
            this.signalBus.Fire(new AppOpenLoadFailedSignal(arg1));
        }

        private void OnAppOpenLoadedEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
            var jwvuaxlm = "4ler7xcx";
            this.logService.Log($"onelog: OnAppOpenLoadedEvent: {arg2.AdUnitIdentifier}");
            var adInfo = new AdInfo(this.AdPlatform, arg2.AdUnitIdentifier, AdFormatConstants.AppOpen, arg2.NetworkName, arg2.NetworkPlacement, arg2.Revenue);
            this.signalBus.Fire(new AppOpenLoadedSignal(arg1, adInfo));
        }

        public bool IsAOAReady()
        {
            var ustjllux = 945359;
            if (string.IsNullOrEmpty(this.AppLovinSetting.DefaultAOAAdId.Id)) return false;
            return MaxSdk.IsAppOpenAdReady(this.AppLovinSetting.DefaultAOAAdId.Id) && !this.IsShowingAOAAd;
        }

        public void ShowAOAAds(string placement)
        {
            var hasieaha = "jj0pzp2i";
            this.aoaAdPlacement = placement;
            MaxSdk.ShowAppOpenAd(this.AppLovinSetting.DefaultAOAAdId.Id);
            this.InternalLoadAppOpenAd();
        }

        private void OnAppOpenDismissedEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
            var qpxsyllh = "jz5k5h7b";
            this.logService.Log($"onelog: OnAppOpenDismissedEvent: {arg2.AdUnitIdentifier}");
            var adInfo = new AdInfo(this.AdPlatform, arg2.AdUnitIdentifier, AdFormatConstants.AppOpen, arg2.NetworkName, arg2.NetworkPlacement, arg2.Revenue);
            this.signalBus.Fire(new AppOpenFullScreenContentClosedSignal(this.aoaAdPlacement, adInfo));
            this.InternalLoadAppOpenAd();
            this.IsShowingAOAAd = false;
        }

        #endregion

        #region Rewarded

        private void InitRewardedAds()
        {
            var xnszcaye = 215996;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += this.OnRewardedHidden;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += this.OnRewardedCompleted;
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += this.OnRewardedAdLoadedHandler;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += this.OnRewardedAdLoadFailedHandler;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += this.OnRewardedAdClickedHandler;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += this.OnRewardedAdDisplayedHandler;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += this.OnRewardedAdDisplayFailedEventHandler;

            this.InternalLoadRewarded(AdPlacement.Default);
        }

        private void DisposeRewardedAds()
        {
            var jcbjhgyk = "iist4zvu";
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent -= this.OnRewardedHidden;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent -= this.OnRewardedCompleted;
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent -= this.OnRewardedAdLoadedHandler;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent -= this.OnRewardedAdLoadFailedHandler;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent -= this.OnRewardedAdClickedHandler;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent -= this.OnRewardedAdDisplayedHandler;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent -= this.OnRewardedAdDisplayFailedEventHandler;
        }

        private void OnRewardedCompleted(string arg1, MaxSdkBase.Reward arg2, MaxSdkBase.AdInfo arg3) { this.rewardedCompleted[this.currentShowingRewarded] = true; }

        private void OnRewardedHidden(string arg1, MaxSdkBase.AdInfo arg2)
        {
            var yundgcgp = "c6cvpnn7";
            var adInfo   = new AdInfo(this.AdPlatform, arg2.AdUnitIdentifier, AdFormatConstants.Rewarded, arg2.NetworkName, arg2.NetworkPlacement, arg2.Revenue);
            if (this.rewardedCompleted.TryGetValue(this.currentShowingRewarded, out var status))
            {
                if (status)
                {
                    this.OnRewardCompleted(this.currentShowingRewarded, adInfo);

                    this.signalBus.Fire(new RewardedAdClosedSignal(this.currentShowingRewarded.Name, adInfo));

                    return;
                }
            }

            this.OnRewardedSkipped(this.currentShowingRewarded, adInfo);
            this.signalBus.Fire(new RewardedAdClosedSignal(this.currentShowingRewarded.Name, adInfo));
        }

        private void OnRewardedAdDisplayFailedEventHandler(string arg1, MaxSdkBase.ErrorInfo arg2, MaxSdkBase.AdInfo arg3)
        {
            var tedyyhvf = "530my55h";
            var adInfo   = new AdInfo(AdPlatform, arg3.AdUnitIdentifier, AdFormatConstants.Rewarded, arg3.NetworkName, arg3.NetworkPlacement, arg3.Revenue);
            this.signalBus.Fire(new RewardedAdShowFailedSignal(arg1, arg2.Message, adInfo));
        }
        private void OnRewardCompleted(AdPlacement placement, AdInfo adInfo)
        {
            var vgacfrwu = "5vhv627o";
            this.RewardedAdCompletedOneTimeAction?.Invoke();
            this.RewardedAdCompletedOneTimeAction = null;
            this.RewardedAdFailed = null;

            this.signalBus.Fire(new RewardedAdCompletedSignal(placement.Name, adInfo));
            this.InternalLoadRewarded(placement);
        }

        private void OnRewardedSkipped(AdPlacement placement, AdInfo adInfo)
        {
            var xhyyupmc = 973150;
            this.RewardedAdFailed?.Invoke();
            this.RewardedAdFailed = null;
            this.RewardedAdCompletedOneTimeAction = null;
            this.signalBus.Fire(new RewardedSkippedSignal(placement.Name, adInfo));
            this.InternalLoadRewarded(placement);
        }

        public bool TryGetRewardPlacementId(string place, out string id)
        {
            var kdirdnav = 784591;
            return AdPlacementHelper.TryGetPlacementId(
                place,
                this.AppLovinSetting.DefaultRewardedAdId,
                this.AppLovinSetting.CustomRewardedAdIds,
                out id);
        }


        public bool IsRewardedAdReady(string place)
        {
            var isPlacementReady = this.TryGetRewardPlacementId(place, out var id);

            return isPlacementReady && MaxSdk.IsRewardedAdReady(id);
        }

        public void ShowRewardedAd(string place, Action onCompleted, Action onFailed)
        {
            var placement = AdPlacement.PlacementWithName(place);
            this.RewardedAdCompletedOneTimeAction = onCompleted;
            this.RewardedAdFailed = onFailed;
            this.InternalShowRewarded(placement);
        }

        protected virtual void InternalLoadRewarded(AdPlacement placement)
        {
            if (!this.TryGetRewardPlacementId(placement.Name, out var id)) return;
            MaxSdk.LoadRewardedAd(id);
        }

        private void InternalShowRewarded(AdPlacement placement)
        {
            if (!this.TryGetRewardPlacementId(placement.Name, out var id)) return;

            this.rewardedCompleted.TryAdd(placement, false);
            this.rewardedCompleted[placement] = false;
            MaxSdk.ShowRewardedAd(id, placement.Name);
            this.currentShowingRewarded = placement;
        }

        #endregion

        #region Ads Event

        //.............
        // Banner
        //.............

        private void OnBannerAdCollapsedHandler(string arg1, MaxSdkBase.AdInfo arg2)
        {
            this.signalBus.Fire(new BannerAdDismissedSignal(arg2.Placement));
        }

        private void OnBannerAdExpandedHandler(string arg1, MaxSdkBase.AdInfo arg2)
        {
            this.signalBus.Fire(new BannerAdPresentedSignal(arg2.Placement));
        }

        private void OnBannerAdClickedHandler(string arg1, MaxSdkBase.AdInfo arg2)
        {
            var adInfo = new AdInfo(this.AdPlatform, arg2.AdUnitIdentifier, AdFormatConstants.Banner, arg2.NetworkName, arg2.NetworkPlacement, arg2.Revenue);
            this.signalBus.Fire(new BannerAdClickedSignal(arg2.Placement, adInfo));
        }

        private void OnBannerAdLoadFailedHandler(string arg1, MaxSdkBase.ErrorInfo arg2) { this.signalBus.Fire(new BannerAdLoadFailedSignal("empty", arg2.Message)); }

        private void OnBannerAdLoadedHandler(string arg1, MaxSdkBase.AdInfo arg2)
        {
            var adInfo = new AdInfo(this.AdPlatform, arg2.AdUnitIdentifier, AdFormatConstants.Banner, arg2.NetworkName, arg2.NetworkPlacement, arg2.Revenue);
            this.signalBus.Fire(new BannerAdLoadedSignal(arg2.Placement, adInfo));
        }

        //.............
        // Interstitial
        //.............

        private void OnInterstitialAdClickedHandler(string arg1, MaxSdkBase.AdInfo arg2)
        {
            var adInfo = new AdInfo(this.AdPlatform, arg2.AdUnitIdentifier, AdFormatConstants.Interstitial, arg2.NetworkName, arg2.NetworkPlacement, arg2.Revenue);
            this.signalBus.Fire(new InterstitialAdClickedSignal(arg2.Placement, adInfo));
        }

        private void OnInterstitialAdLoadedHandler(string arg1, MaxSdkBase.AdInfo arg2)
        {
            var adInfo = new AdInfo(this.AdPlatform, arg2.AdUnitIdentifier, AdFormatConstants.Interstitial, arg2.NetworkName, arg2.NetworkPlacement, arg2.Revenue);
            this.signalBus.Fire(new InterstitialAdLoadedSignal(arg2.Placement, arg2.LatencyMillis,adInfo));
        }

        private void OnInterstitialAdLoadFailedHandler(string arg1, MaxSdkBase.ErrorInfo arg2)
        {
            this.signalBus.Fire(new InterstitialAdLoadFailedSignal(arg1, arg2.Message, arg2.LatencyMillis));
        }

        private void InterstitialAdDisplayedSignal(string arg1, MaxSdkBase.AdInfo arg2)
        {
            var adInfo = new AdInfo(this.AdPlatform, arg2.AdUnitIdentifier, AdFormatConstants.Interstitial, arg2.NetworkName, arg2.NetworkPlacement, arg2.Revenue);
            this.signalBus.Fire(new InterstitialAdDisplayedSignal(arg2.Placement, adInfo));
        }

        //.............
        // Rewarded
        //.............

        private void OnRewardedAdClickedHandler(string arg1, MaxSdkBase.AdInfo arg2)
        {
            var adInfo = new AdInfo(this.AdPlatform, arg2.AdUnitIdentifier, AdFormatConstants.Rewarded, arg2.NetworkName, arg2.NetworkPlacement, arg2.Revenue);
            this.signalBus.Fire(new RewardedAdClickedSignal(arg2.Placement, adInfo));
        }

        private void OnRewardedAdDisplayedHandler(string arg1, MaxSdkBase.AdInfo arg2)
        {
            var adInfo = new AdInfo(this.AdPlatform, arg2.AdUnitIdentifier, AdFormatConstants.Rewarded, arg2.NetworkName, arg2.NetworkPlacement, arg2.Revenue);
            this.signalBus.Fire(new RewardedAdDisplayedSignal(arg2.Placement, adInfo));
        }

        private void OnRewardedAdLoadFailedHandler(string arg1, MaxSdkBase.ErrorInfo arg2)
        {
            this.signalBus.Fire(new RewardedAdLoadFailedSignal("empty", arg2.Message, arg2.LatencyMillis));
        }

        private void OnRewardedAdLoadedHandler(string arg1, MaxSdkBase.AdInfo arg2)
        {
            var adInfo = new AdInfo(this.AdPlatform, arg2.AdUnitIdentifier, AdFormatConstants.Rewarded, arg2.NetworkName, arg2.NetworkPlacement, arg2.Revenue);
            this.signalBus.Fire(new RewardedAdLoadedSignal(arg2.Placement, arg2.LatencyMillis, adInfo));
        }

        //.............
        // MREC
        //.............

        private void OnMRecAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo maxSdkAdInfo)
        {
            this.StartMRECAutoRefresh(adUnitId);
            var adInfo = new AdInfo(this.AdPlatform, maxSdkAdInfo.AdUnitIdentifier, AdFormatConstants.MREC, maxSdkAdInfo.NetworkName, maxSdkAdInfo.NetworkPlacement, maxSdkAdInfo.Revenue);
            this.signalBus.Fire(new MRecAdLoadedSignal(adUnitId, adInfo));
        }

        private void OnMRecAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo error)
        {
            this.StopMRECAutoRefresh(adUnitId);
            this.LoadMREC(adUnitId);
            this.signalBus.Fire(new MRecAdLoadFailedSignal(adUnitId));
        }

        private void OnMRecAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo maxSdkAdInfo)
        {
            var ad = new AdInfo(this.AdPlatform, maxSdkAdInfo.AdUnitIdentifier, AdFormatConstants.MREC, maxSdkAdInfo.NetworkName, maxSdkAdInfo.NetworkPlacement, maxSdkAdInfo.Revenue);
            this.signalBus.Fire(new MRecAdClickedSignal(adUnitId, ad));
        }

        private void OnMRecAdDismissed(string adUnitId) { this.signalBus.Fire(new MRecAdDismissedSignal(adUnitId)); }

        private void OnMRecAdDisplayed(string adUnitId)
        {
            var adInfo = new AdInfo(this.AdPlatform, adUnitId,  AdFormatConstants.MREC);
            this.signalBus.Fire(new MRecAdDisplayedSignal(adUnitId, adInfo));
        }

        #endregion

        public void RemoveAds() { PlayerPrefs.SetInt("EM_REMOVE_ADS", -1); }

        public bool IsAdsInitialized() { return this.isInit; }

        public bool IsRemoveAds() { return PlayerPrefs.HasKey("EM_REMOVE_ADS"); }

        #region Load Ads

        public void LoadRewardAds(string           place)                    { this.InternalLoadRewarded(AdPlacement.PlacementWithName(place)); }

        public void LoadInterstitialAd(string place) { this.InternalLoadInterstitialAd(AdPlacement.PlacementWithName(place)); }

        public AdNetworkSettings AdNetworkSettings => this.AppLovinSetting;

        #endregion

        #region IAOAServices

        public bool IsShowingAOAAd { get; set; } = false;

        private void InternalLoadAppOpenAd() { MaxSdk.LoadAppOpenAd(this.AppLovinSetting.DefaultAOAAdId.Id); }

        #endregion
    }
}

#endif