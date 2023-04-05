namespace ServiceImplementation.AdsServices.EasyMobile
{
#if EM_ADMOB
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Utilities.Extension;
    using GameFoundation.Scripts.Utilities.LogService;
    using GoogleMobileAds.Api;
    using GoogleMobileAds.Common;
    using UnityEngine;
    using Zenject;

    public class AdModWrapper : IAOAAdService, IMRECAdService
    {
        #region inject

        private readonly ILogService       logService;
        private readonly Config            config;
        private readonly SignalBus         signalBus;
        private readonly IAdServices       adServices;
        private readonly IAnalyticServices analyticService;

        #endregion

        public AdModWrapper(ILogService logService, Config config, SignalBus signalBus, IAdServices adServices, IAnalyticServices analyticService)
        {
            this.logService      = logService;
            this.config          = config;
            this.signalBus       = signalBus;
            this.adServices      = adServices;
            this.analyticService = analyticService;
        }

        public void Init()
        {
            this.signalBus.Subscribe<InterstitialAdDisplayedSignal>(this.ShownAdInDifferentProcessHandler);
            this.signalBus.Subscribe<RewardedAdDisplayedSignal>(this.ShownAdInDifferentProcessHandler);
            MobileAds.Initialize(_ =>
                                 {
                                     this.IntervalCall(5);
                                     AppStateEventNotifier.AppStateChanged += this.OnAppStateChanged;
                                 });
        }

        private async void IntervalCall(int intervalSecond)
        {
            this.LoadAppOpenAd();
            this.LoadAllMRec();
            await UniTask.Delay(TimeSpan.FromSeconds(intervalSecond));
            this.IntervalCall(intervalSecond);
        }

        #region AOA

        public class Config
        {
            public List<string>                       ADModAoaIds;
            public Dictionary<AdViewPosition, string> ADModMRecIds;

            public readonly bool IsShowAOAAtOpenApp   = true;
            public readonly bool OpenAOAAfterResuming = true;

            public Config(List<string> adModAoaIds, bool isShowAoaAtOpenApp = true, bool openAoaAfterResuming = true)
            {
                this.ADModAoaIds          = adModAoaIds;
                this.OpenAOAAfterResuming = openAoaAfterResuming;
                this.IsShowAOAAtOpenApp   = isShowAoaAtOpenApp;
            }
        }

        private bool isShowedFirstOpen = false;
        private bool isResumedFromAds  = false;

        private void ShownAdInDifferentProcessHandler() { this.isResumedFromAds = true; }

        private async void OnAppStateChanged(AppState state)
        {
            await UniTask.SwitchToMainThread();
            // Display the app open ad when the app is foregrounded.
            this.logService.Log($"App State is {state}");

            if (state != AppState.Foreground) return;
            if (!this.config.OpenAOAAfterResuming) return;

            if (this.isResumedFromAds)
            {
                this.isResumedFromAds = false;

                return;
            }

            if (!this.adServices.IsRemoveAds())
            {
                this.ShowAdIfAvailable();
            }
        }

        #region IAOAService

        public bool IsShowingAd { get; set; } = false;

        public void ShowAdIfAvailable()
        {
            if (this.IsShowingAd)
            {
                return;
            }

            var loadedAppOpenAd = this.aoaAdIdToLoadedAdInstance.Values.FirstOrDefault(openAd => openAd.IsAoaAdAvailable);
            loadedAppOpenAd?.Show();
        }

        #endregion

        private Dictionary<string, LoadedAppOpenAd> aoaAdIdToLoadedAdInstance = new();

        private class LoadedAppOpenAd
        {
            private AppOpenAd appOpenAd;
            private DateTime  loadedTime;
            public  bool      IsLoading = false;

            public void Init(AppOpenAd appOpenAd)
            {
                if (this.appOpenAd != null)
                {
                    this.appOpenAd.Destroy();
                    this.appOpenAd = null;
                }

                this.IsLoading                        =  false;
                this.appOpenAd                        =  appOpenAd;
                this.loadedTime                       =  DateTime.UtcNow;
                appOpenAd.OnAdFullScreenContentClosed += this.AOAHandleAppOpenAdDidDismissFullScreenContent;
                appOpenAd.OnAdFullScreenContentFailed += this.AOAHandleAppOpenAdFailedToPresentFullScreenContent;
            }

            private async void AOAHandleAppOpenAdFailedToPresentFullScreenContent(AdError agError)
            {
                await UniTask.SwitchToMainThread();
                this.appOpenAd.Destroy();
                this.appOpenAd = null;
            }

            private async void AOAHandleAppOpenAdDidDismissFullScreenContent()
            {
                await UniTask.SwitchToMainThread();
                this.appOpenAd.Destroy();
                this.appOpenAd = null;
            }

            public bool IsAoaAdAvailable => this.appOpenAd != null && (DateTime.UtcNow - this.loadedTime).TotalHours < 4; //AppOpenAd is valid for 4 hours

            public void Show() { this.appOpenAd.Show(); }
        }

        private void LoadAppOpenAd()
        {
            foreach (var configADModAoaId in this.config.ADModAoaIds)
            {
                this.LoadAppOpenAdWithId(configADModAoaId);
            }
        }

        private void LoadAppOpenAdWithId(string adUnitId)
        {
            this.logService.Log($"Start request Open App Ads Tier {adUnitId}");

            //Don't need to load this ad if it's already loaded.
            var loadedAppOpenAd = this.aoaAdIdToLoadedAdInstance.GetOrAdd(adUnitId, () => new LoadedAppOpenAd());

            if (loadedAppOpenAd.IsAoaAdAvailable || loadedAppOpenAd.IsLoading)
            {
                this.logService.Log($"Ad Status is available {loadedAppOpenAd.IsAoaAdAvailable} and loading {loadedAppOpenAd.IsLoading}");

                return;
            }

            loadedAppOpenAd.IsLoading = true;
            AppOpenAd.Load(adUnitId, Screen.orientation, new AdRequest.Builder().Build(), LoadAoaCompletedHandler);

            void LoadAoaCompletedHandler(AppOpenAd appOpenAd, LoadAdError error)
            {
                if (error != null)
                {
                    // Handle the error.
                    this.logService.Log($"Failed to load the ad. (reason: {error.GetMessage()}), id: {adUnitId}");
                    loadedAppOpenAd.IsLoading = false;

                    return;
                }

                // App open ad is loaded.
                appOpenAd.OnAdFullScreenContentClosed += this.AOAHandleAdFullScreenContentClosed;
                appOpenAd.OnAdFullScreenContentFailed += this.AOAHandleAdFullScreenContentFailed;
                appOpenAd.OnAdFullScreenContentOpened += this.AOAHandleAdFullScreenContentOpened;
                appOpenAd.OnAdImpressionRecorded      += this.AOAHandleAdImpressionRecorded;
                appOpenAd.OnAdPaid                    += this.AdMobHandlePaidEvent;

                loadedAppOpenAd.Init(appOpenAd);

                lock (this)
                {
                    if (!this.isShowedFirstOpen && this.config.IsShowAOAAtOpenApp)
                    {
                        loadedAppOpenAd.Show();
                        this.isShowedFirstOpen = true;
                    }
                }
            }
        }

        private async void AOAHandleAdFullScreenContentClosed()
        {
            await UniTask.SwitchToMainThread();
            this.logService.Log("Closed app open ad");
            // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
            this.IsShowingAd = false;
        }

        private async void AOAHandleAdFullScreenContentFailed(AdError args)
        {
            await UniTask.SwitchToMainThread();
            this.logService.Log($"Failed to present the ad (reason: {args.GetMessage()})");
            // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
        }

        private async void AOAHandleAdFullScreenContentOpened()
        {
            await UniTask.SwitchToMainThread();
            this.logService.Log("Displayed app open ad");
            this.IsShowingAd = true;
        }

        private async void AOAHandleAdImpressionRecorded()
        {
            await UniTask.SwitchToMainThread();
            this.logService.Log("Recorded ad impression");
        }

        private async void AdMobHandlePaidEvent(AdValue args)
        {
            await UniTask.SwitchToMainThread();
            this.analyticService.Track(new AdsRevenueEvent()
                                       {
                                           AdsRevenueSourceId = "AdMob",
                                           Revenue            = args.Value / 1e5,
                                           Currency           = "USD",
                                           Placement          = "AOA",
                                           AdNetwork          = "AdMob"
                                       });
            this.logService.Log($"Received paid event. (currency: {args.CurrencyCode}, value: {args.Value}");
        }

        #endregion

        #region MREC

        private Dictionary<AdViewPosition, BannerView> positionToMRECBannerView  = new();
        private Dictionary<AdViewPosition, bool>       positionToMRECToIsLoading = new();


        public void ShowMREC(AdViewPosition             adViewPosition) { this.positionToMRECBannerView[adViewPosition].Show(); }
        public void HideMREC(AdViewPosition             adViewPosition) { this.positionToMRECBannerView[adViewPosition].Hide(); }
        public void StopMRECAutoRefresh(AdViewPosition  adViewPosition) { }
        public void StartMRECAutoRefresh(AdViewPosition adViewPosition) { }
        public void LoadMREC(AdViewPosition adViewPosition)
        {
            if (this.positionToMRECBannerView.ContainsKey(adViewPosition) || this.positionToMRECToIsLoading.GetOrAdd(adViewPosition, () => false))
            {
                return;
            }

            var bannerView = new BannerView(this.config.ADModMRecIds[adViewPosition], AdSize.MediumRectangle, AdPosition.Center);


            var adRequest = new AdRequest.Builder().AddKeyword("car-climber-game").Build();

            // send the request to load the ad.
            bannerView.LoadAd(adRequest);
            this.positionToMRECToIsLoading[adViewPosition] =  true;
            bannerView.OnBannerAdLoaded                    += () => { this.positionToMRECBannerView.Add(adViewPosition, bannerView); };
            bannerView.OnBannerAdLoadFailed                += _ => { this.positionToMRECToIsLoading[adViewPosition] = false; };

            bannerView.OnBannerAdLoaded            += this.BannerViewOnAdLoaded;
            bannerView.OnBannerAdLoadFailed        += this.BannerViewOnAdLoadFailed;
            bannerView.OnAdClicked                 += this.BannerViewOnAdClicked;
            bannerView.OnAdPaid                    += this.AdMobHandlePaidEvent;
            bannerView.OnAdFullScreenContentOpened += this.BannerViewOnAdFullScreenContentOpened;
            bannerView.OnAdFullScreenContentClosed += this.BannerViewOnAdFullScreenContentClosed;
        }
        public bool IsReady(AdViewPosition adViewPosition) { return this.positionToMRECBannerView.ContainsKey(adViewPosition); }

        private void LoadAllMRec()
        {
            foreach (var (position, _) in this.config.ADModMRecIds)
            {
                this.LoadMREC(position);
            }
        }

        private async void BannerViewOnAdFullScreenContentClosed()
        {
            await UniTask.SwitchToMainThread();
            this.signalBus.Fire(new MRecAdDismissedSignal(""));
        }
        private async void BannerViewOnAdFullScreenContentOpened()
        {
            await UniTask.SwitchToMainThread();
            this.signalBus.Fire(new MRecAdDisplayedSignal(""));
        }

        private async void BannerViewOnAdClicked()
        {
            await UniTask.SwitchToMainThread();
            this.signalBus.Fire(new MRecAdClickedSignal(""));
        }

        private async void BannerViewOnAdLoadFailed(LoadAdError obj)
        {
            await UniTask.SwitchToMainThread();
            this.signalBus.Fire(new MRecAdLoadFailedSignal(""));
        }

        private async void BannerViewOnAdLoaded()
        {
            await UniTask.SwitchToMainThread();
            this.signalBus.Fire(new MRecAdLoadedSignal(""));
        }

        public event Action<string, AdInfo>    OnAdLoadedEvent;
        public event Action<string, ErrorInfo> OnAdLoadFailedEvent;
        public event Action<string, AdInfo>    OnAdClickedEvent;
        public event Action<string, AdInfo>    OnAdRevenuePaidEvent;
        public event Action<string, AdInfo>    OnAdExpandedEvent;
        public event Action<string, AdInfo>    OnAdCollapsedEvent;

        #endregion
    }

#endif
}