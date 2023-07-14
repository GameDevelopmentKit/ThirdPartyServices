#if APPLOVIN
namespace ServiceImplementation.AdsServices.AppLovin
{
    using System;
    using System.Collections.Generic;
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Utilities.LogService;
    using ServiceImplementation.Configs;
    using ServiceImplementation.Configs.Ads;
    using UnityEngine;
    using Zenject;

    public class AppLovinAdsWrapper : IAdServices, IMRECAdService, IInitializable, IDisposable, IAdLoadService
    {
        #region Inject

        private readonly ILogService                        logService;
        private readonly SignalBus                          signalBus;
        private readonly AdServicesConfig                   adServicesConfig;
        private readonly Dictionary<AdViewPosition, string> positionToMRECAdUnitId;
        private readonly ThirdPartiesConfig                 thirdPartiesConfig;

        #endregion

        #region Cache

        private AppLovinSettings              adsSettings;
        private AdPlacement                   currentShowingInterstitial;
        private AdPlacement                   currentShowingRewarded;
        private Dictionary<AdPlacement, bool> rewardedCompleted = new();

        private readonly Dictionary<AdPlacement, KeyValuePair<BannerAdsPosition, BannerSize>> placementToBanner = new();

        private bool         isInit;
        private event Action RewardedAdCompletedOneTimeAction;

        #endregion

        public AppLovinAdsWrapper(ILogService logService, SignalBus signalBus, AdServicesConfig adServicesConfig, Dictionary<AdViewPosition, string> positionToMRECAdUnitId,
            ThirdPartiesConfig thirdPartiesConfig)
        {
            this.logService             = logService;
            this.signalBus              = signalBus;
            this.adServicesConfig       = adServicesConfig;
            this.positionToMRECAdUnitId = positionToMRECAdUnitId;
            this.thirdPartiesConfig     = thirdPartiesConfig;
        }

        public async void Initialize()
        {
            this.adsSettings = this.thirdPartiesConfig.AdSettings.AppLovin;

            MaxSdk.SetIsAgeRestrictedUser(this.adsSettings.AgeRestrictMode);
            MaxSdk.SetSdkKey(this.thirdPartiesConfig.AdSettings.AppLovin.SDKKey);
            MaxSdk.InitializeSdk();

            await UniTask.WaitUntil(MaxSdk.IsInitialized);
            this.InitBannerAds();
            this.InitMRECAds();
            this.InitInterstitialAds();
            this.InitRewardedAds();

            this.isInit = true;

            this.logService.Log("AppLovin Ads Services has been initialized!");
        }

        public void Dispose()
        {
            this.DisposeBannerAds();
            this.DisposeInterstitialAds();
            this.DisposeRewardedAds();
            this.DisposeMRECAds();
        }

        #region Extension

        protected virtual string FindIdForPlacement(Dictionary<AdPlacement, AdId> dict, AdPlacement placement)
        {
            AdId idObj = null;
            if (placement != null && dict != null)
            {
                dict.TryGetValue(placement, out idObj);
            }

            if (idObj != null && !string.IsNullOrEmpty(idObj.Id))
            {
                return idObj.Id;
            }

            return string.Empty;
        }

        private MaxSdkBase.BannerPosition ConvertToBannerAdPosition(BannerAdsPosition pos)
        {
            return pos switch
            {
                BannerAdsPosition.Top => MaxSdkBase.BannerPosition.TopCenter,
                BannerAdsPosition.Bottom => MaxSdkBase.BannerPosition.BottomCenter,
                BannerAdsPosition.TopLeft => MaxSdkBase.BannerPosition.TopLeft,
                BannerAdsPosition.TopRight => MaxSdkBase.BannerPosition.TopRight,
                BannerAdsPosition.BottomLeft => MaxSdkBase.BannerPosition.BottomLeft,
                BannerAdsPosition.BottomRight => MaxSdkBase.BannerPosition.BottomRight,
                _ => MaxSdkBase.BannerPosition.Centered
            };
        }

        private MaxSdkBase.AdViewPosition ConvertAdViewPosition(AdViewPosition adViewPosition) =>
            adViewPosition switch
            {
                AdViewPosition.TopLeft => MaxSdkBase.AdViewPosition.TopLeft,
                AdViewPosition.TopCenter => MaxSdkBase.AdViewPosition.TopCenter,
                AdViewPosition.TopRight => MaxSdkBase.AdViewPosition.TopRight,
                AdViewPosition.CenterLeft => MaxSdkBase.AdViewPosition.CenterLeft,
                AdViewPosition.Centered => MaxSdkBase.AdViewPosition.Centered,
                AdViewPosition.CenterRight => MaxSdkBase.AdViewPosition.CenterRight,
                AdViewPosition.BottomLeft => MaxSdkBase.AdViewPosition.BottomLeft,
                AdViewPosition.BottomCenter => MaxSdkBase.AdViewPosition.BottomCenter,
                AdViewPosition.BottomRight => MaxSdkBase.AdViewPosition.BottomRight,
                _ => MaxSdkBase.AdViewPosition.BottomCenter
            };

        private AdInfo ConvertAdInfo(MaxSdkBase.AdInfo maxAdInfo)
        {
            return new AdInfo()
            {
                AdUnitIdentifier   = maxAdInfo.AdUnitIdentifier,
                AdFormat           = maxAdInfo.AdFormat,
                NetworkName        = maxAdInfo.NetworkName,
                NetworkPlacement   = maxAdInfo.NetworkPlacement,
                Placement          = maxAdInfo.NetworkPlacement,
                CreativeIdentifier = maxAdInfo.CreativeIdentifier,
                Revenue            = maxAdInfo.Revenue,
                RevenuePrecision   = maxAdInfo.RevenuePrecision,
                DspName            = maxAdInfo.DspName
            };
        }

        #endregion

        #region MREC

        private void InitMRECAds()
        {
            foreach (var (position, adUnitId) in this.positionToMRECAdUnitId)
            {
                this.logService.Log($"Check max init {MaxSdk.IsInitialized()}");
                MaxSdk.CreateMRec(adUnitId, this.ConvertAdViewPosition(position));
            }

            MaxSdkCallbacks.MRec.OnAdLoadedEvent      += this.OnMRecAdLoadedEvent;
            MaxSdkCallbacks.MRec.OnAdLoadFailedEvent  += this.OnMRecAdLoadFailedEvent;
            MaxSdkCallbacks.MRec.OnAdClickedEvent     += this.OnMRecAdClickedEvent;
            MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += this.OnMRecAdRevenuePaidEvent;
            MaxSdkCallbacks.MRec.OnAdExpandedEvent    += this.OnMRecAdExpandedEvent;
            MaxSdkCallbacks.MRec.OnAdCollapsedEvent   += this.OnMRecAdCollapsedEvent;
        }

        private void DisposeMRECAds()
        {
            MaxSdkCallbacks.MRec.OnAdLoadedEvent      -= this.OnMRecAdLoadedEvent;
            MaxSdkCallbacks.MRec.OnAdLoadFailedEvent  -= this.OnMRecAdLoadFailedEvent;
            MaxSdkCallbacks.MRec.OnAdClickedEvent     -= this.OnMRecAdClickedEvent;
            MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent -= this.OnMRecAdRevenuePaidEvent;
            MaxSdkCallbacks.MRec.OnAdExpandedEvent    -= this.OnMRecAdExpandedEvent;
            MaxSdkCallbacks.MRec.OnAdCollapsedEvent   -= this.OnMRecAdCollapsedEvent;
        }

        public void ShowMREC(AdViewPosition adViewPosition)
        {
            if (!this.adServicesConfig.EnableMRECAd)
            {
                return;
            }

            MaxSdk.UpdateMRecPosition(this.positionToMRECAdUnitId[adViewPosition], this.ConvertAdViewPosition(adViewPosition));
            MaxSdk.ShowMRec(this.positionToMRECAdUnitId[adViewPosition]);
        }

        public void HideMREC(AdViewPosition adViewPosition) { MaxSdk.HideMRec(this.positionToMRECAdUnitId[adViewPosition]); }

        public void StopMRECAutoRefresh(AdViewPosition adViewPosition) { MaxSdk.StopMRecAutoRefresh(this.positionToMRECAdUnitId[adViewPosition]); }

        public void StartMRECAutoRefresh(AdViewPosition adViewPosition) { MaxSdk.StartMRecAutoRefresh(this.positionToMRECAdUnitId[adViewPosition]); }

        public void LoadMREC(AdViewPosition adViewPosition) { MaxSdk.LoadMRec(this.positionToMRECAdUnitId[adViewPosition]); }

        public bool IsMRECReady(AdViewPosition adViewPosition) { return this.positionToMRECAdUnitId.ContainsKey(adViewPosition); }

        public void HideAllMREC()
        {
            foreach (var (adViewPosition, adUnitId) in this.positionToMRECAdUnitId)
            {
                this.HideMREC(adViewPosition);
            }
        }

        public event Action<string, AdInfo>    OnAdLoadedEvent;
        public event Action<string, ErrorInfo> OnAdLoadFailedEvent;
        public event Action<string, AdInfo>    OnAdClickedEvent;
        public event Action<string, AdInfo>    OnAdRevenuePaidEvent;
        public event Action<string, AdInfo>    OnAdExpandedEvent;
        public event Action<string, AdInfo>    OnAdCollapsedEvent;

        #endregion

        #region Banner

        private void InitBannerAds()
        {
            MaxSdkCallbacks.Banner.OnAdLoadedEvent     += this.OnBannerAdLoadedHandler;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += this.OnBannerAdLoadFailedHandler;
            MaxSdkCallbacks.Banner.OnAdClickedEvent    += this.OnBannerAdClickedHandler;
            MaxSdkCallbacks.Banner.OnAdExpandedEvent   += this.OnBannerAdExpandedHandler;
            MaxSdkCallbacks.Banner.OnAdCollapsedEvent  += this.OnBannerAdCollapsedHandler;
        }

        private void DisposeBannerAds()
        {
            MaxSdkCallbacks.Banner.OnAdLoadedEvent     -= this.OnBannerAdLoadedHandler;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent -= this.OnBannerAdLoadFailedHandler;
            MaxSdkCallbacks.Banner.OnAdClickedEvent    -= this.OnBannerAdClickedHandler;
            MaxSdkCallbacks.Banner.OnAdExpandedEvent   -= this.OnBannerAdExpandedHandler;
            MaxSdkCallbacks.Banner.OnAdCollapsedEvent  -= this.OnBannerAdCollapsedHandler;
        }

        public void ShowBannerAd(BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom, int width = 320, int height = 50)
        {
            this.InternalShowBanner(AdPlacement.Default, bannerAdsPosition, new BannerSize(width, height));
        }

        public void HideBannedAd() { this.InternalHideBanner(AdPlacement.Default); }

        public void DestroyBannerAd() { this.InternalDestroyBanner(AdPlacement.Default); }

        private bool IsBannerPlacementReady(string place, out string id)
        {
            var placement = AdPlacement.PlacementWithName(place);
            id = placement == AdPlacement.Default
                ? this.adsSettings.DefaultBannerAdId.Id
                : this.FindIdForPlacement(this.adsSettings.CustomBannerAdIds, placement);
            return !string.IsNullOrEmpty(id);
        }

        private void InternalShowBanner(AdPlacement adPlacement, BannerAdsPosition position, BannerSize bannerSize)
        {
            if (!this.IsBannerPlacementReady(adPlacement.Name, out var id)) return;

            var shouldCreateBanner = !this.placementToBanner.ContainsKey(adPlacement)
                                     || this.placementToBanner[adPlacement].Key != position
                                     || this.placementToBanner[adPlacement].Value != bannerSize;

            if (shouldCreateBanner)
            {
                this.InternalDestroyBanner(adPlacement);
                MaxSdk.CreateBanner(id, this.ConvertToBannerAdPosition(position));
                MaxSdk.SetBannerBackgroundColor(id, new Color(1, 1, 1, 0));

                if (!this.placementToBanner.ContainsKey(adPlacement))
                {
                    this.placementToBanner.Add(adPlacement, new KeyValuePair<BannerAdsPosition, BannerSize>(position, bannerSize));
                }
            }

            MaxSdk.ShowBanner(id);
        }

        private void InternalHideBanner(AdPlacement adPlacement)
        {
            if (!this.IsBannerPlacementReady(adPlacement.Name, out var id)) return;
            MaxSdk.HideBanner(id);
        }

        private void InternalDestroyBanner(AdPlacement adPlacement)
        {
            if (!this.IsBannerPlacementReady(adPlacement.Name, out var id)) return;
            MaxSdk.DestroyBanner(id);
            this.placementToBanner.Remove(adPlacement);
        }

        #endregion

        #region Interstitial

        private void InitInterstitialAds()
        {
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent     += this.OnInterstitialCompleted;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent  += this.InterstitialAdDisplayedSignal;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += this.OnInterstitialAdLoadFailedHandler;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent    += this.OnInterstitialAdClickedHandler;

            this.InternalLoadInterstitialAd(AdPlacement.Default);
        }

        private void DisposeInterstitialAds()
        {
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent     -= this.OnInterstitialCompleted;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent  -= this.InterstitialAdDisplayedSignal;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent -= this.OnInterstitialAdLoadFailedHandler;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent    -= this.OnInterstitialAdClickedHandler;
        }

        public bool IsInterstitialAdReady(string place)
        {
            var isPlacementReady = this.IsInterstitialPlacementReady(place, out var id);
            return isPlacementReady && MaxSdk.IsInterstitialReady(id);
        }

        public void ShowInterstitialAd(string place)
        {
            var placement = AdPlacement.PlacementWithName(place);
            this.InternalShowInterstitialAd(placement);
        }

        private bool IsInterstitialPlacementReady(string place, out string id)
        {
            var placement = AdPlacement.PlacementWithName(place);
            id = placement == AdPlacement.Default
                ? this.adsSettings.DefaultInterstitialAdId.Id
                : this.FindIdForPlacement(this.adsSettings.CustomInterstitialAdIds, placement);
            return !string.IsNullOrEmpty(id);
        }

        private void InternalLoadInterstitialAd(AdPlacement adPlacement)
        {
            if (!this.IsInterstitialPlacementReady(adPlacement.Name, out var id)) return;
            MaxSdk.LoadInterstitial(id);
        }

        private void InternalShowInterstitialAd(AdPlacement adPlacement)
        {
            if (!this.IsInterstitialPlacementReady(adPlacement.Name, out var id)) return;
            MaxSdk.ShowInterstitial(id);
            this.currentShowingInterstitial = adPlacement;
        }

        private void OnInterstitialCompleted(string arg1, MaxSdkBase.AdInfo arg2)
        {
            this.signalBus.Fire(new InterstitialAdClosedSignal(this.currentShowingInterstitial.Name));
            this.InternalLoadInterstitialAd(this.currentShowingInterstitial);
        }

        #endregion

        #region Rewarded

        private void InitRewardedAds()
        {
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent         += this.OnRewardedHidden;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += this.OnRewardedCompleted;
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent         += this.OnRewardedAdLoadedHandler;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent     += this.OnRewardedAdLoadFailedHandler;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent        += this.OnRewardedAdClickedHandler;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent      += this.OnRewardedAdDisplayedHandler;

            this.InternalLoadRewarded(AdPlacement.Default);
        }

        private void DisposeRewardedAds()
        {
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent         -= this.OnRewardedHidden;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent -= this.OnRewardedCompleted;
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent         -= this.OnRewardedAdLoadedHandler;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent     -= this.OnRewardedAdLoadFailedHandler;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent        -= this.OnRewardedAdClickedHandler;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent      -= this.OnRewardedAdDisplayedHandler;
        }

        private void OnRewardedCompleted(string arg1, MaxSdkBase.Reward arg2, MaxSdkBase.AdInfo arg3) { this.rewardedCompleted[this.currentShowingRewarded] = true; }

        private void OnRewardedHidden(string arg1, MaxSdkBase.AdInfo arg2)
        {
            if (this.rewardedCompleted.ContainsKey(this.currentShowingRewarded))
            {
                if (this.rewardedCompleted[this.currentShowingRewarded])
                {
                    this.OnRewardCompleted(this.currentShowingRewarded);
                    return;
                }
            }

            this.OnRewardedSkipped(this.currentShowingRewarded);
        }

        private void OnRewardCompleted(AdPlacement placement)
        {
            this.RewardedAdCompletedOneTimeAction?.Invoke();
            this.RewardedAdCompletedOneTimeAction = null;
            this.signalBus.Fire(new RewardedAdCompletedSignal(placement.Name));
            this.InternalLoadRewarded(placement);
        }

        private void OnRewardedSkipped(AdPlacement placement)
        {
            this.RewardedAdCompletedOneTimeAction = null;
            this.signalBus.Fire(new RewardedSkippedSignal(placement.Name));
            this.InternalLoadRewarded(placement);
        }

        private bool IsRewardedPlacementReady(string place, out string id)
        {
            var placement = AdPlacement.PlacementWithName(place);
            id = placement == AdPlacement.Default
                ? this.adsSettings.DefaultRewardedAdId.Id
                : this.FindIdForPlacement(this.adsSettings.CustomRewardedAdIds, placement);
            return !string.IsNullOrEmpty(id);
        }

        public bool IsRewardedAdReady(string place)
        {
            var isPlacementReady = this.IsRewardedPlacementReady(place, out var id);
            return isPlacementReady && MaxSdk.IsRewardedAdReady(id);
        }

        public void ShowRewardedAd(string place, Action onCompleted)
        {
            var placement = AdPlacement.PlacementWithName(place);
            this.RewardedAdCompletedOneTimeAction = onCompleted;
            this.InternalShowRewarded(placement);
        }

        private void InternalLoadRewarded(AdPlacement placement)
        {
            if (!this.IsRewardedPlacementReady(placement.Name, out var id)) return;
            MaxSdk.LoadRewardedAd(id);
        }

        private void InternalShowRewarded(AdPlacement placement)
        {
            if (!this.IsRewardedPlacementReady(placement.Name, out var id)) return;

            if (!this.rewardedCompleted.ContainsKey(placement))
            {
                this.rewardedCompleted.Add(placement, false);
            }

            this.rewardedCompleted[placement] = false;
            MaxSdk.ShowRewardedAd(id);
            this.currentShowingRewarded = placement;
        }

        #endregion

        #region Ads Event

        //.............
        // Banner
        //.............

        private void OnBannerAdCollapsedHandler(string arg1, MaxSdkBase.AdInfo arg2) { this.signalBus.Fire(new BannerAdDismissedSignal(arg2.Placement)); }

        private void OnBannerAdExpandedHandler(string arg1, MaxSdkBase.AdInfo arg2) { this.signalBus.Fire(new BannerAdPresentedSignal(arg2.Placement)); }

        private void OnBannerAdClickedHandler(string arg1, MaxSdkBase.AdInfo arg2) { this.signalBus.Fire(new BannerAdClickedSignal(arg2.Placement)); }

        private void OnBannerAdLoadFailedHandler(string arg1, MaxSdkBase.ErrorInfo arg2) { this.signalBus.Fire(new BannerAdLoadFailedSignal("empty", arg2.Message)); }

        private void OnBannerAdLoadedHandler(string arg1, MaxSdkBase.AdInfo arg2) { this.signalBus.Fire(new BannerAdLoadedSignal(arg2.Placement)); }

        //.............
        // Interstitial
        //.............

        private void OnInterstitialAdClickedHandler(string arg1, MaxSdkBase.AdInfo arg2) { this.signalBus.Fire(new InterstitialAdClickedSignal(arg2.Placement)); }

        private void OnInterstitialAdLoadFailedHandler(string arg1, MaxSdkBase.ErrorInfo arg2) { this.signalBus.Fire(new InterstitialAdLoadFailedSignal("empty", arg2.Message)); }

        private void InterstitialAdDisplayedSignal(string arg1, MaxSdkBase.AdInfo arg2) { this.signalBus.Fire(new InterstitialAdDisplayedSignal(arg2.Placement)); }

        //.............
        // Rewarded
        //.............

        private void OnRewardedAdClickedHandler(string arg1, MaxSdkBase.AdInfo arg2) { this.signalBus.Fire(new RewardedAdLoadClickedSignal(arg2.Placement)); }

        private void OnRewardedAdDisplayedHandler(string arg1, MaxSdkBase.AdInfo arg2) { this.signalBus.Fire(new RewardedAdDisplayedSignal(arg2.Placement)); }

        private void OnRewardedAdLoadFailedHandler(string arg1, MaxSdkBase.ErrorInfo arg2) { this.signalBus.Fire(new RewardedAdLoadFailedSignal("empty", arg2.Message)); }

        private void OnRewardedAdLoadedHandler(string arg1, MaxSdkBase.AdInfo arg2) { this.signalBus.Fire(new RewardedAdLoadedSignal(arg2.Placement)); }

        //.............
        // MREC
        //.............

        private void OnMRecAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { this.OnAdLoadedEvent?.Invoke(adUnitId, this.ConvertAdInfo(adInfo)); }

        private void OnMRecAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo error) { this.OnAdLoadFailedEvent?.Invoke(adUnitId, new ErrorInfo((int)error.Code, error.Message)); }

        private void OnMRecAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { this.OnAdClickedEvent?.Invoke(adUnitId, this.ConvertAdInfo(adInfo)); }

        private void OnMRecAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { this.OnAdRevenuePaidEvent?.Invoke(adUnitId, this.ConvertAdInfo(adInfo)); }

        private void OnMRecAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { this.OnAdExpandedEvent?.Invoke(adUnitId, this.ConvertAdInfo(adInfo)); }

        private void OnMRecAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { this.OnAdCollapsedEvent?.Invoke(adUnitId, this.ConvertAdInfo(adInfo)); }

        #endregion

        public void RemoveAds(bool revokeConsent = false) { PlayerPrefs.SetInt("EM_REMOVE_ADS", -1); }

        public bool IsAdsInitialized() { return this.isInit; }

        public bool IsRemoveAds() { return PlayerPrefs.HasKey("EM_REMOVE_ADS"); }

        #region Load Ads

        public void LoadRewardAds(string place) { this.InternalLoadRewarded(AdPlacement.PlacementWithName(place)); }

        public void LoadInterstitialAd(string place) { this.InternalLoadInterstitialAd(AdPlacement.PlacementWithName(place)); }

        public AdNetworkSettings AdNetworkSettings => this.adsSettings;

        #endregion
    }
}

#endif