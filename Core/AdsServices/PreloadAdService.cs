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

        private async void LoadAdsInterval(string place = "")
        {
            Debug.Log("load ads intreval");
            this.adLoadServices.ForEach(ads=>this.LoadSingleAds(ads,place));
            await UniTask.Delay(TimeSpan.FromSeconds(this.adServicesConfig.IntervalLoadAds));
            this.LoadAdsInterval(place);
        }

        private void LoadSingleAds(IAdLoadService loadService, string place)
        {
            if(loadService.IsRemoveAds()) return;
            if(!loadService.IsInterstitialAdReady(place)) loadService.LoadInterstitialAd();
            if(!loadService.IsRewardedAdReady(place)) loadService.LoadRewardAds();
        }
        
    }
}