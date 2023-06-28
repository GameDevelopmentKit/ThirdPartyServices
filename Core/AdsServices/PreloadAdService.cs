namespace Core.AdsServices
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using Zenject;
    using Debug = UnityEngine.Debug;

    public class PreloadAdService: IInitializable
    {
        private List<IAdLoadService> adLoadServices;
        private AdServicesConfig     adServicesConfig;
        
        public PreloadAdService(List<IAdLoadService> adLoadServices,AdServicesConfig adServicesConfig)
        {
            this.adLoadServices   = adLoadServices;
            this.adServicesConfig = adServicesConfig;
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
            if(!loadService.IsInterstitialAdReady()) loadService.LoadInterstitialAd();
            if(!loadService.IsRewardedAdReady()) loadService.LoadRewardAds();
        }
        
    }
}