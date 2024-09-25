namespace ServiceImplementation.AdsServices.PreloadService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using Core.AnalyticServices;
    using Core.AnalyticServices.Tools;
    using Cysharp.Threading.Tasks;
    using ServiceImplementation.Configs.Ads;
    using Zenject;
    using Debug = UnityEngine.Debug;

    public class PreloadAdService : IInitializable, IDisposable, ITickable
    {
        #region inject

        private readonly List<IAdLoadService>      adLoadServices;
        private readonly AdServicesConfig          adServicesConfig;
        private readonly SignalBus                 signalBus;
        private readonly IAnalyticServices         analyticServices;
        private readonly List<IAOAAdService>       aOaAdServices;
        private readonly UnScaleInGameStopWatchManager unScaleInGameStopWatchManager;

        #endregion
        
        private Dictionary<(IAdLoadService, string), UnScaleInGameStopWatch> interstitialAdStopwatch = new();
        private Dictionary<(IAdLoadService, string), UnScaleInGameStopWatch> rewardAdStopwatch       = new();
        private Dictionary<IAOAAdService, UnScaleInGameStopWatch>            aoaAdStartTime          = new();
        
        public PreloadAdService(List<IAdLoadService> adLoadServices, AdServicesConfig adServicesConfig, SignalBus signalBus, IAnalyticServices analyticServices, List<IAOAAdService> aOAAdServices, UnScaleInGameStopWatchManager unScaleInGameStopWatchManager)
        {
            this.adLoadServices            = adLoadServices;
            this.adServicesConfig          = adServicesConfig;
            this.signalBus                 = signalBus;
            this.analyticServices          = analyticServices;
            this.aOaAdServices             = aOAAdServices;
            this.unScaleInGameStopWatchManager = unScaleInGameStopWatchManager;
        }
        public void Initialize()
        {
            this.LoadAdsInterval();
            
            this.signalBus.Subscribe<RewardedAdCompletedSignal>(this.LoadRewardAdsAfterShow);
            this.signalBus.Subscribe<RewardedSkippedSignal>(this.LoadRewardAdsAfterSkip);
            this.signalBus.Subscribe<InterstitialAdClosedSignal>(this.LoadInterAdsAfterShow);
            this.aoaAdStartTime = this.aOaAdServices.ToDictionary(aOaAdService => aOaAdService, _ => this.unScaleInGameStopWatchManager.StartNew());
        }

        private async void LoadAdsInterval()
        {
            Debug.Log("load ads interval");
            this.adLoadServices.ForEach(this.LoadAdsOneTime);
            await UniTask.Delay(TimeSpan.FromSeconds(this.adServicesConfig.IntervalLoadAds));
            this.LoadAdsInterval();
        }

        private void LoadAdsOneTime(IAdLoadService loadService)
        {
            if (loadService.IsRemoveAds()) return;
            this.LoadAllInterAds(loadService);
            this.LoadAllRewardAds(loadService);
        }

        #region Load InterstitialAds
        
        private void LoadInterstitial(IAdLoadService adLoadService, string placement = "")
        {
            if (!adLoadService.IsInterstitialAdReady(placement))
            {
                adLoadService.LoadInterstitialAd(placement);
                this.interstitialAdStopwatch.TryAdd((adLoadService, placement), this.unScaleInGameStopWatchManager.StartNew());
                var adUnitId = adLoadService.TryGetInterstitialPlacementId(placement, out var id) ? id : string.Empty;
                var adInfo   = new AdInfo(adLoadService.AdPlatform, adUnitId, "Interstitial");
                this.signalBus.Fire<AdRequestSignal>(new(placement, adInfo));
            }
        }

        private void LoadAllInterAds(IAdLoadService loadService)
        {
            if (loadService.AdNetworkSettings.CustomInterstitialAdIds == null || loadService.AdNetworkSettings.CustomInterstitialAdIds.Count == 0)
            {
                this.LoadInterstitial(loadService);
                return;
            }

            foreach (var (key, _) in loadService.AdNetworkSettings.CustomInterstitialAdIds)
            {
                this.LoadInterstitial(loadService, key.Name);
            }
        }

        private void LoadInterAdsAfterShow(InterstitialAdClosedSignal signal) { this.LoadInterAdWithPlace(signal.Placement); }

        private void LoadInterAdWithPlace(string placement)
        {
            this.adLoadServices.ForEach(adLoadService =>
            {
                this.LoadInterstitial(adLoadService, placement);
            });
        }

        #endregion


        #region Load RewardAds
        
        private void LoadReward(IAdLoadService adLoadService, string placement = "")
        {
            if (!adLoadService.IsRewardedAdReady(placement))
            {
                adLoadService.LoadRewardAds(placement);
                this.rewardAdStopwatch.TryAdd((adLoadService, placement), this.unScaleInGameStopWatchManager.StartNew());
                var adUnitId = adLoadService.TryGetRewardPlacementId(placement, out var id) ? id : string.Empty;
                var adInfo   = new AdInfo(adLoadService.AdPlatform, adUnitId, "Rewarded");
                this.signalBus.Fire<AdRequestSignal>(new(placement, adInfo));
            }
        }

        private void LoadAllRewardAds(IAdLoadService loadService)
        {
            if (loadService.AdNetworkSettings.CustomRewardedAdIds == null || loadService.AdNetworkSettings.CustomRewardedAdIds.Count == 0)
            {
                this.LoadReward(loadService);
                return;
            }

            foreach (var (key, value) in loadService.AdNetworkSettings.CustomRewardedAdIds)
            {
                this.LoadReward(loadService, key.Name);
            }
        }

        private void LoadRewardAdsAfterShow(RewardedAdCompletedSignal signal) { this.LoadRewardAdWithPlace(signal.Placement); }

        private void LoadRewardAdsAfterSkip(RewardedSkippedSignal signal) { this.LoadRewardAdWithPlace(signal.Placement); }

        private void LoadRewardAdWithPlace(string placement)
        {
            this.adLoadServices.ForEach(ads =>
            {
                this.LoadReward(ads, placement);
            });
        }

        #endregion

        public void Dispose()
        {
            this.signalBus.TryUnsubscribe<RewardedAdCompletedSignal>(this.LoadRewardAdsAfterShow);
            this.signalBus.TryUnsubscribe<InterstitialAdClosedSignal>(this.LoadInterAdsAfterShow);
            this.signalBus.TryUnsubscribe<RewardedSkippedSignal>(this.LoadRewardAdsAfterSkip);
        }
        
        public void Tick()
        {
            // check interstitial ads
            if (this.interstitialAdStopwatch.Count > 0)
            {
                foreach (var ((adLoadService, placement), stopwatch) in this.interstitialAdStopwatch.ToList())
                {
                    if (adLoadService.IsInterstitialAdReady(placement))
                    {
                        this.analyticServices.Track(new PreLoadInter(placement, this.unScaleInGameStopWatchManager.Stop(stopwatch), adLoadService.GetType().Name));
                        this.interstitialAdStopwatch.Remove((adLoadService, placement));
                    }
                }
            }

            // check reward ads
            if (this.rewardAdStopwatch.Count > 0)
            {
                foreach (var ((adLoadService, placement), stopwatch) in this.rewardAdStopwatch.ToList())
                {
                    if (adLoadService.IsRewardedAdReady(placement))
                    {
                        this.analyticServices.Track(new PreLoadReward(placement, this.unScaleInGameStopWatchManager.Stop(stopwatch), adLoadService.GetType().Name));
                        this.rewardAdStopwatch.Remove((adLoadService, placement));
                    }
                } 
            }
            
            // check  AOA ads
            foreach (var aOaAdService in aOaAdServices)
            {
                if (!aOaAdService.IsAOAReady())
                {
                    // Record the start time when the service starts loading
                    if (!this.aoaAdStartTime.ContainsKey(aOaAdService))
                    {
                        this.aoaAdStartTime.Add(aOaAdService, this.unScaleInGameStopWatchManager.StartNew());
                    }
                }
                else
                {
                    // Calculate the elapsed time when the service is ready
                    if (this.aoaAdStartTime.Remove(aOaAdService, out var watch))
                    {
                        this.analyticServices.Track(new PreLoadAOA(string.Empty, this.unScaleInGameStopWatchManager.Stop(watch), aOaAdService.GetType().Name));
                    }
                }
            }
        }
    }
}