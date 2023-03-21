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

        public void Initialize() { this.signalBus.Subscribe<ShowInterstitialAdSignal>(this.ShownInterstitialAdHandler); }

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
        private bool IsResumedFromAds  = false;

        private void ShownInterstitialAdHandler() { this.IsResumedFromAds = true; }

        private void OnAppStateChanged(AppState state)
        {
            // Display the app open ad when the app is foregrounded.
            this.logService.Log($"App State is {state}");

            if (state != AppState.Foreground) return;
            if (!this.config.OpenAfterResuming) return;

            if (this.IsResumedFromAds)
            {
                this.IsResumedFromAds = false;

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

            var loadedAppOpenAd = this.aoaAdIdToLoadedAdInstance.Values.FirstOrDefault(openAd => openAd.IsAOAAdAvailable);
            loadedAppOpenAd?.Show();
        }

        #endregion

        private Dictionary<string, LoadedAppOpenAd> aoaAdIdToLoadedAdInstance = new();

        private class LoadedAppOpenAd
        {
            private AppOpenAd appOpenAd;
            private DateTime  loadedTime;
            public  bool      isLoading = false;

            public void Init(AppOpenAd appOpenAd)
            {
                if (this.appOpenAd != null)
                {
                    this.appOpenAd.Destroy();
                    this.appOpenAd = null;
                }

                this.isLoading                                 =  false;
                this.appOpenAd                                 =  appOpenAd;
                this.loadedTime                                =  DateTime.UtcNow;
                appOpenAd.OnAdDidDismissFullScreenContent      += this.HandleAppOpenAdDidDismissFullScreenContent;
                appOpenAd.OnAdFailedToPresentFullScreenContent += this.HandleAppOpenAdFailedToPresentFullScreenContent;
            }

            private void HandleAppOpenAdFailedToPresentFullScreenContent(object sender, AdErrorEventArgs e)
            {
                this.appOpenAd.Destroy();
                this.appOpenAd = null;
            }

            private void HandleAppOpenAdDidDismissFullScreenContent(object sender, EventArgs e)
            {
                this.appOpenAd.Destroy();
                this.appOpenAd = null;
            }

            public bool IsAOAAdAvailable => this.appOpenAd != null && (DateTime.UtcNow - this.loadedTime).TotalHours < 4; //AppOpenAd is valid for 4 hours

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

            if (loadedAppOpenAd.IsAOAAdAvailable || loadedAppOpenAd.isLoading) return;

            loadedAppOpenAd.isLoading = true;
            AppOpenAd.LoadAd(adUnitId, Screen.orientation, new AdRequest.Builder().Build(), LoadAOACompletedHandler);

            void LoadAOACompletedHandler(AppOpenAd appOpenAd, AdFailedToLoadEventArgs error)
            {
                if (error != null)
                {
                    // Handle the error.
                    this.logService.Log($"Failed to load the ad. (reason: {error.LoadAdError.GetMessage()}), id: {adUnitId}");

                    return;
                }

                // App open ad is loaded.
                appOpenAd.OnAdDidDismissFullScreenContent      += this.HandleAppOpenAdDidDismissFullScreenContent;
                appOpenAd.OnAdFailedToPresentFullScreenContent += this.HandleAppOpenAdFailedToPresentFullScreenContent;
                appOpenAd.OnAdDidPresentFullScreenContent      += this.HandleAppOpenAdDidPresentFullScreenContent;
                appOpenAd.OnAdDidRecordImpression              += this.HandleAppOpenAdDidRecordImpression;
                appOpenAd.OnPaidEvent                          += this.HandlePaidEvent;

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

        private void HandleAppOpenAdDidDismissFullScreenContent(object sender, EventArgs args)
        {
            this.logService.Log("Closed app open ad");
            // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
            this.IsShowingAd = false;
        }

        private void HandleAppOpenAdFailedToPresentFullScreenContent(object sender, AdErrorEventArgs args)
        {
            this.logService.Log($"Failed to present the ad (reason: {args.AdError.GetMessage()})");
            // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
        }

        private void HandleAppOpenAdDidPresentFullScreenContent(object sender, EventArgs args)
        {
            this.logService.Log("Displayed app open ad");
            this.IsShowingAd = true;
        }

        private void HandleAppOpenAdDidRecordImpression(object sender, EventArgs args) { this.logService.Log("Recorded ad impression"); }

        private void HandlePaidEvent(object sender, AdValueEventArgs args)
        {
            this.analyticService.Track(new AdsRevenueEvent() { Currency = args.AdValue.CurrencyCode, Revenue = args.AdValue.Value / 1e5 });
            this.logService.Log($"Received paid event. (currency: {args.AdValue.CurrencyCode}, value: {args.AdValue.Value}");
        }

        #endregion
    }

#endif
}