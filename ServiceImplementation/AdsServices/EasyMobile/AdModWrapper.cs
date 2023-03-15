namespace ServiceImplementation.AdsServices.EasyMobile
{
#if EM_ADMOB
    using System;
    using System.Collections.Generic;
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using GameFoundation.Scripts.Utilities.LogService;
    using GoogleMobileAds.Api;
    using GoogleMobileAds.Common;
    using UnityEngine;
    using Zenject;

    public class AdModWrapper : IAOAAdService, IInitializable
    {
        #region inject

        private readonly ILogService logService;
        private readonly Config      config;
        private readonly SignalBus   signalBus;
        private readonly IAdServices adServices;

        #endregion

        public AdModWrapper(ILogService logService, Config config, SignalBus signalBus, IAdServices adServices)
        {
            this.logService = logService;
            this.config     = config;
            this.signalBus  = signalBus;
            this.adServices = adServices;
        }

        public void Initialize()
        {
            this.signalBus.Subscribe<ShowInterstitialAdSignal>(this.ShownInterstitialAdHandler);

            MobileAds.Initialize(_ =>
            {
                this.LoadAOAAd();
                AppStateEventNotifier.AppStateChanged += this.OnAppStateChanged;
            });
        }

        #region AOA
        
        private bool IsAOAAdAvailable => this.AppOpenAd != null && (DateTime.UtcNow - this.loadTime).TotalHours < 4;

        private int CurrentAOAAdIdIndex = 0;

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

        private DateTime  loadTime;
        private AppOpenAd AppOpenAd;
        private bool      isShowedFirstOpen = false;
        private bool      IsResumedFromAds  = false;
        
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

            this.ShowAdIfAvailable();
        }

        #region IAOAService

        public bool IsShowingAd { get; set; } = false;

        public void LoadAOAAd()
        {
            if (this.adServices.IsRemoveAds())
            {
                return;
            }

            this.LoadAppOpenAd();
        }

        public void ShowAdIfAvailable()
        {
            if (this.IsShowingAd)
            {
                return;
            }

            if (!this.IsAOAAdAvailable)
            {
                this.LoadAOAAd();
                return;
            }

            if (this.adServices.IsRemoveAds())
            {
                return;
            }

            this.AppOpenAd.OnAdDidDismissFullScreenContent      += this.HandleAppOpenAdDidDismissFullScreenContent;
            this.AppOpenAd.OnAdFailedToPresentFullScreenContent += this.HandleAppOpenAdFailedToPresentFullScreenContent;
            this.AppOpenAd.OnAdDidPresentFullScreenContent      += this.HandleAppOpenAdDidPresentFullScreenContent;
            this.AppOpenAd.OnAdDidRecordImpression              += this.HandleAppOpenAdDidRecordImpression;
            this.AppOpenAd.OnPaidEvent                          += this.HandlePaidEvent;

            this.AppOpenAd.Show();
        }

        #endregion

        public void LoadAppOpenAd()
        {
            this.logService.Log("Start request Open App Ads Tier " + this.CurrentAOAAdIdIndex);

            var request = new AdRequest.Builder().Build();

            AppOpenAd.LoadAd(this.config.ADModAoaIds[this.CurrentAOAAdIdIndex], ScreenOrientation.Portrait, request, ((appOpenAd, error) =>
            {
                if (error != null)
                {
                    // Handle the error.
                    this.logService
                        .Log($"Failed to load the ad. (reason: {error.LoadAdError.GetMessage()}), tier {this.CurrentAOAAdIdIndex}");

                    this.CurrentAOAAdIdIndex++;

                    if (this.CurrentAOAAdIdIndex < this.config.ADModAoaIds.Count)
                        this.LoadAppOpenAd();
                    else
                        this.CurrentAOAAdIdIndex = 0;

                    return;
                }

                // App open ad is loaded.
                this.AppOpenAd        = appOpenAd;
                this.CurrentAOAAdIdIndex = 0;
                this.loadTime         = DateTime.UtcNow;

                if (!this.isShowedFirstOpen && this.config.IsShowAOAAtOpenApp)
                {
                    this.ShowAdIfAvailable();
                    this.isShowedFirstOpen = true;
                }
            }));
        }

        private void HandleAppOpenAdDidDismissFullScreenContent(object sender, EventArgs args)
        {
            this.logService.Log("Closed app open ad");
            // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
            this.AppOpenAd   = null;
            this.IsShowingAd = false;
            this.LoadAOAAd();
        }

        private void HandleAppOpenAdFailedToPresentFullScreenContent(object sender, AdErrorEventArgs args)
        {
            this.logService.Log($"Failed to present the ad (reason: {args.AdError.GetMessage()})");
            // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
            this.AppOpenAd = null;
            this.LoadAOAAd();
        }

        private void HandleAppOpenAdDidPresentFullScreenContent(object sender, EventArgs args)
        {
            this.logService.Log("Displayed app open ad");
            this.IsShowingAd = true;
        }

        private void HandleAppOpenAdDidRecordImpression(object sender, EventArgs args) { this.logService.Log("Recorded ad impression"); }

        private void HandlePaidEvent(object sender, AdValueEventArgs args) { this.logService.Log($"Received paid event. (currency: {args.AdValue.CurrencyCode}, value: {args.AdValue.Value}"); }
        
        #endregion
        
    }
#endif
}