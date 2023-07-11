namespace Core.AdsServices
{
    using System;
    using System.Collections.Generic;
    using Core.AdsServices.Signals;
    using Cysharp.Threading.Tasks;
    using Sirenix.Utilities;
    using UnityEngine;
    using Zenject;

    public class PreloadAdService : IInitializable, IDisposable
    {
        private List<IAdLoadService> adLoadServices;
        private AdServicesConfig     adServicesConfig;
        private SignalBus            signalBus;
        public PreloadAdService(List<IAdLoadService> adLoadServices, AdServicesConfig adServicesConfig, SignalBus signalBus)
        {
            this.adLoadServices   = adLoadServices;
            this.adServicesConfig = adServicesConfig;
            this.signalBus        = signalBus;
        }
        public void Initialize()
        {
            this.LoadAdsInterval();
            this.signalBus.Subscribe<RewardedAdCompletedSignal>(this.LoadRewardAdsAfterShow);
            this.signalBus.Subscribe<RewardedSkippedSignal>(this.LoadRewardAdsAfterSkip);
            this.signalBus.Subscribe<RewardedInterstitialAdCompletedSignal>(this.LoadInterAdsAfterShow);
            this.signalBus.Subscribe<RewardInterstitialAdSkippedSignal>(this.LoadInterAdsAfterSkip);
        }

        private async void LoadAdsInterval()
        {
            Debug.Log("load ads interval");
            this.adLoadServices.ForEach(ads => this.LoadSingleAds(ads));
            await UniTask.Delay(TimeSpan.FromSeconds(this.adServicesConfig.IntervalLoadAds));
            this.LoadAdsInterval();
        }

        private void LoadSingleAds(IAdLoadService loadService)
        {
            if (loadService.IsRemoveAds()) return;
            this.LoadSingleInterAds(loadService);
            this.LoadSingleRewardAds(loadService);
        }

        #region Load InterstitialAds

        private void LoadSingleInterAds(IAdLoadService loadService)
        {
            if (loadService.AdNetworkSettings.CustomInterstitialAdIds == null || loadService.AdNetworkSettings.CustomInterstitialAdIds.Count == 0)
            {
                if (!loadService.IsInterstitialAdReady()) loadService.LoadInterstitialAd();
                return;
            }

            loadService.AdNetworkSettings.CustomInterstitialAdIds.ForEach(adsNetwork =>
            {
                if (!loadService.IsInterstitialAdReady(adsNetwork.Key.Name)) loadService.LoadInterstitialAd(adsNetwork.Key.Name);
            });
        }

        private void LoadInterAdsAfterShow(RewardedInterstitialAdCompletedSignal signal) { this.LoadSingleInterAdWithPlace(signal.Placement); }

        private void LoadInterAdsAfterSkip(RewardInterstitialAdSkippedSignal signal) { this.LoadSingleInterAdWithPlace(signal.Placement); }

        private void LoadSingleInterAdWithPlace(string placement)
        {
            this.adLoadServices.ForEach(ads =>
            {
                if (!ads.IsInterstitialAdReady(placement)) ads.LoadInterstitialAd(placement);
            });
        }

        #endregion


        #region Load RewardAds

        private void LoadSingleRewardAds(IAdLoadService loadService)
        {
            if (loadService.AdNetworkSettings.CustomRewardedAdIds == null || loadService.AdNetworkSettings.CustomRewardedAdIds.Count == 0)
            {
                if (!loadService.IsRewardedAdReady()) loadService.LoadRewardAds();
                return;
            }

            loadService.AdNetworkSettings.CustomRewardedAdIds.ForEach(adsNetwork =>
            {
                if (!loadService.IsRewardedAdReady(adsNetwork.Key.Name)) loadService.LoadRewardAds(adsNetwork.Key.Name);
            });
        }

        private void LoadRewardAdsAfterShow(RewardedAdCompletedSignal signal) { this.LoadSingleRewardAdWithPlace(signal.Placement); }

        private void LoadRewardAdsAfterSkip(RewardedSkippedSignal signal) { this.LoadSingleRewardAdWithPlace(signal.Placement); }

        private void LoadSingleRewardAdWithPlace(string placement)
        {
            this.adLoadServices.ForEach(ads =>
            {
                if (!ads.IsRewardedAdReady(placement)) ads.LoadRewardAds(placement);
            });
        }

        #endregion


        public void Dispose()
        {
            this.signalBus.TryUnsubscribe<RewardedAdCompletedSignal>(this.LoadRewardAdsAfterShow);
            this.signalBus.TryUnsubscribe<RewardedInterstitialAdCompletedSignal>(this.LoadInterAdsAfterShow);
            this.signalBus.TryUnsubscribe<RewardedSkippedSignal>(this.LoadRewardAdsAfterSkip);
            this.signalBus.TryUnsubscribe<RewardInterstitialAdSkippedSignal>(this.LoadInterAdsAfterSkip);
        }
    }
}