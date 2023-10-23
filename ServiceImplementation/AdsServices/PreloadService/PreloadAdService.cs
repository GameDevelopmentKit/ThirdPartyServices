namespace Core.AdsServices
{
    using System;
    using System.Collections.Generic;
    using Core.AdsServices.Signals;
    using Cysharp.Threading.Tasks;
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
            this.signalBus.Subscribe<InterstitialAdClosedSignal>(this.LoadInterAdsAfterShow);
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

            foreach (var (key, value) in loadService.AdNetworkSettings.CustomInterstitialAdIds)
            {
                if (!loadService.IsInterstitialAdReady(key.Name)) loadService.LoadInterstitialAd(key.Name);
            }
        }

        private void LoadInterAdsAfterShow(InterstitialAdClosedSignal signal) { this.LoadSingleInterAdWithPlace(signal.Placement); }

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

            foreach (var (key, value) in loadService.AdNetworkSettings.CustomRewardedAdIds)
            {
                if (!loadService.IsRewardedAdReady(key.Name)) loadService.LoadRewardAds(key.Name);
            }
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
            this.signalBus.TryUnsubscribe<InterstitialAdClosedSignal>(this.LoadInterAdsAfterShow);
            this.signalBus.TryUnsubscribe<RewardedSkippedSignal>(this.LoadRewardAdsAfterSkip);
        }
    }
}