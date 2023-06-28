namespace Core.AdsServices
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using Sirenix.Utilities;
    using UnityEngine;
    using Zenject;

    public class PreloadAdService: IInitializable
    {
        private List<IAdLoadService>                        adLoadServices;
        private AdServicesConfig                            adServicesConfig;
        public PreloadAdService(List<IAdLoadService> adLoadServices,AdServicesConfig adServicesConfig)
        {
            this.adLoadServices     = adLoadServices;
            this.adServicesConfig   = adServicesConfig;
        }
        public void Initialize()
        {
            this.LoadAdsInterval();
        }

        private async void LoadAdsInterval()
        {
            Debug.Log("load ads interval");
            this.adLoadServices.ForEach(ads=>this.LoadSingleAds(ads));
            await UniTask.Delay(TimeSpan.FromSeconds(this.adServicesConfig.IntervalLoadAds));
            this.LoadAdsInterval();
        }

        private void LoadSingleAds(IAdLoadService loadService)
        {
            if(loadService.IsRemoveAds()) return;
            this.LoadSingleInterAds(loadService);
            this.LoadSingleRewardAds(loadService);
        }

        private void LoadSingleInterAds(IAdLoadService loadService)
        {
            if (loadService.AdNetworkSettings.CustomInterstitialAdIds == null || loadService.AdNetworkSettings.CustomInterstitialAdIds.Count == 0)
            {
                if(!loadService.IsInterstitialAdReady()) loadService.LoadInterstitialAd();
                return;
            }

            loadService.AdNetworkSettings.CustomInterstitialAdIds.ForEach(adsNetwork =>
            {
                if(!loadService.IsInterstitialAdReady(adsNetwork.Key.Name)) loadService.LoadInterstitialAd(adsNetwork.Key.Name);
            });
        }
        
        private void LoadSingleRewardAds(IAdLoadService loadService)
        {
            if (loadService.AdNetworkSettings.CustomRewardedAdIds == null || loadService.AdNetworkSettings.CustomRewardedAdIds.Count == 0)
            {
                if(!loadService.IsRewardedAdReady()) loadService.LoadRewardAds();
                return;
            }

            loadService.AdNetworkSettings.CustomRewardedAdIds.ForEach(adsNetwork =>
            {
                if(!loadService.IsRewardedAdReady(adsNetwork.Key.Name)) loadService.LoadRewardAds(adsNetwork.Key.Name);
            });
        }
        
        
    }
}