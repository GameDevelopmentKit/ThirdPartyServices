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

    public class AdModWrapper : IAOAAdService, IInitializable
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

        public void Initialize()
        {
            this.signalBus.Subscribe<InterstitialAdDisplayedSignal>(this.ShownAdInDifferentProcessHandler);
            this.signalBus.Subscribe<RewardedAdDisplayedSignal>(this.ShownAdInDifferentProcessHandler);

        }

        public void Init()
        {
            MobileAds.Initialize(_ =>
            {
                this.IntervalCall(5);
                AppStateEventNotifier.AppStateChanged += this.OnAppStateChanged;
            });
        }

        private async void IntervalCall(int intervalSecond)
        {
            this.LoadAppOpenAd();
            await UniTask.Delay(TimeSpan.FromSeconds(intervalSecond));
            this.IntervalCall(intervalSecond);
        }

        #region AOA

        public class Config
        {
            public List<string> ADModAoaIds;
            public bool         IsShowAOAAtOpenApp = true;
            public bool         OpenAfterResuming  = true;

            public Config(List<string> adModAoaIds, bool isShowAoaAtOpenApp = true, bool openAfterResuming = true)
            {
                this.ADModAoaIds        = adModAoaIds;
                this.OpenAfterResuming  = openAfterResuming;
                this.IsShowAOAAtOpenApp = isShowAoaAtOpenApp;
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
            if (!this.config.OpenAfterResuming) return;

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
                appOpenAd.OnAdPaid                    += this.AOAHandlePaidEvent;

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

        private async void AOAHandlePaidEvent(AdValue args)
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
    }

#endif
}