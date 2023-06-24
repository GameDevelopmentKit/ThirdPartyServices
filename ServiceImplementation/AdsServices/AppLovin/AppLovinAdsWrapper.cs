#if ADS_ENABLE && ADS_APPLOVIN
namespace ServiceImplementation.AdsServices.AppLovin
{
    using System;
    using System.Collections.Generic;
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using GameFoundation.Scripts.Utilities.LogService;
    using ServiceImplementation.Configs;
    using ServiceImplementation.Configs.Ads;
    using UnityEngine;
    using Zenject;

    public class AppLovinAdsWrapper : IAdServices, IInitializable, IDisposable
    {
        #region Inject

        private readonly ILogService      logService;
        private readonly SignalBus        signalBus;
        private readonly AdServicesConfig adServicesConfig;

        #endregion

        #region Cache

        private AppLovinSettings              adsSettings;
        private AdPlacement                   currentShowingInterstitial;
        private AdPlacement                   currentShowingRewarded;
        private Dictionary<AdPlacement, bool> rewardedCompleted = new();

        private readonly Dictionary<AdPlacement, KeyValuePair<BannerAdsPosition, BannerSize>> placementToBanner = new();

        private bool     isInit;
        private DateTime lasTimeShowInter = DateTime.MinValue;

        private event Action RewardedAdCompletedOneTimeAction;

        #endregion

        public AppLovinAdsWrapper(ILogService logService, SignalBus signalBus, AdServicesConfig adServicesConfig)
        {
            this.logService       = logService;
            this.signalBus        = signalBus;
            this.adServicesConfig = adServicesConfig;
        }

        public void Initialize()
        {
            this.adsSettings = ThirdPartiesConfig.Advertising.AppLovin;

            MaxSdk.SetIsAgeRestrictedUser(this.adsSettings.AgeRestrictMode);
            MaxSdk.SetSdkKey(ThirdPartiesConfig.Advertising.AppLovin.SDKKey);
            MaxSdk.InitializeSdk();

            this.InitBannerAds();
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
            else
            {
                return string.Empty;
            }
        }

        private MaxSdkBase.BannerPosition ToAppLovinAdPosition(BannerAdsPosition pos)
        {
            switch (pos)
            {
                case BannerAdsPosition.Top:
                    return MaxSdkBase.BannerPosition.TopCenter;
                case BannerAdsPosition.Bottom:
                    return MaxSdkBase.BannerPosition.BottomCenter;
                case BannerAdsPosition.TopLeft:
                    return MaxSdkBase.BannerPosition.TopLeft;
                case BannerAdsPosition.TopRight:
                    return MaxSdkBase.BannerPosition.TopRight;
                case BannerAdsPosition.BottomLeft:
                    return MaxSdkBase.BannerPosition.BottomLeft;
                case BannerAdsPosition.BottomRight:
                    return MaxSdkBase.BannerPosition.BottomRight;
                default:
                    return MaxSdkBase.BannerPosition.Centered;
            }
        }

        #endregion

        #region Banner

        public void InitBannerAds() { }

        public void DisposeBannerAds() { }

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
                MaxSdk.CreateBanner(id, this.ToAppLovinAdPosition(position));
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
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += this.OnInterstitialCompleted;

            this.InternalLoadInterstitialAd(AdPlacement.Default);
        }

        private void DisposeInterstitialAds() { MaxSdkCallbacks.Interstitial.OnAdHiddenEvent -= this.OnInterstitialCompleted; }

        public bool IsInterstitialAdReady(string place)
        {
            var isPlacementReady = this.IsInterstitialPlacementReady(place, out var id);
            return isPlacementReady && MaxSdk.IsInterstitialReady(id);
        }

        public void ShowInterstitialAd(string place)
        {
            var placement = AdPlacement.PlacementWithName(place);
            var totalTime = (DateTime.UtcNow - this.lasTimeShowInter).TotalSeconds;
            if (totalTime < this.adServicesConfig.InterstitialAdInterval) return;

            this.lasTimeShowInter = DateTime.UtcNow;
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

            this.InternalLoadRewarded(AdPlacement.Default);
        }

        private void DisposeRewardedAds()
        {
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent         += this.OnRewardedHidden;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += this.OnRewardedCompleted;
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

        public void RemoveAds(bool revokeConsent = false)
        {
            // TODO
        }

        public bool IsAdsInitialized() { return this.isInit; }

        public bool IsRemoveAds()
        {
            // TODO 
            return false;
        }
    }
}

#endif