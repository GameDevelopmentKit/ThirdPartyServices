namespace ServiceImplementation.AdsServices.EasyMobile
{
#if EM_ADMOB
    using System;
    using System.Collections.Generic;
    using Core.AdsServices;
    using GameFoundation.Scripts.Utilities.LogService;
    using GoogleMobileAds.Api;
    using GoogleMobileAds.Common;
    using UnityEngine;
    using Zenject;

    public class AdModWrapper : IAOAAdService, IInitializable
    {
        #region inject

        private readonly ILogService  logService;
        private readonly List<string> adModAoaIds;
        private readonly IAdServices  adServices;

        #endregion

        public AdModWrapper(ILogService logService, List<string> adModAOAIds, IAdServices adServices)
        {
            this.logService  = logService;
            this.adModAoaIds = adModAOAIds;
            this.adServices  = adServices;
        }

        public event Action AppOpenOpened;


        private AppOpenAd AppOpenAd;

        private DateTime loadTime;

        private bool isShowingAd = false;

        private bool showFirstOpen = false;

        public static bool ConfigOpenApp   = true;
        public static bool ConfigResumeApp = true;

        public static bool ResumeFromAds = false;

        private bool IsAdAvailable => this.AppOpenAd != null && (DateTime.UtcNow - this.loadTime).TotalHours < 4;

        private int tierIndex = 1;

        public void Initialize()
        {
            MobileAds.Initialize(_ =>
                                 {
                                     this.LoadAOAAd();
                                     AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
                                 });
        }


        private void OnAppStateChanged(AppState state)
        {
            // Display the app open ad when the app is foregrounded.
            this.logService.Log($"App State is {state}");
            if (state != AppState.Foreground) return;
            if (ConfigResumeApp && !ResumeFromAds)
            {
                this.ShowAdIfAvailable();
            }
        }

        public void LoadAOAAd()
        {
            if (this.adServices.IsRemoveAds())
            {
                return;
            }

            this.LoadAppOpenAd();
        }

        public void LoadAppOpenAd()
        {
            this.logService.Log("Start request Open App Ads Tier " + this.tierIndex);

            var request = new AdRequest.Builder().Build();

            AppOpenAd.LoadAd(this.adModAoaIds[this.tierIndex - 1], ScreenOrientation.Portrait, request, ((appOpenAd, error) =>
                                                                                                         {
                                                                                                             if (error != null)
                                                                                                             {
                                                                                                                 // Handle the error.
                                                                                                                 this.logService
                                                                                                                     .Log($"Failed to load the ad. (reason: {error.LoadAdError.GetMessage()}), tier {this.tierIndex}");
                                                                                                                 if (this.tierIndex <= 3)
                                                                                                                     this.LoadAppOpenAd();
                                                                                                                 else
                                                                                                                     this.tierIndex = 1;
                                                                                                                 return;
                                                                                                             }

                                                                                                             // App open ad is loaded.
                                                                                                             this.AppOpenAd = appOpenAd;
                                                                                                             this.tierIndex = 1;
                                                                                                             this.loadTime  = DateTime.UtcNow;
                                                                                                             if (!this.showFirstOpen && ConfigOpenApp)
                                                                                                             {
                                                                                                                 this.ShowAdIfAvailable();
                                                                                                                 this.showFirstOpen = true;
                                                                                                             }
                                                                                                         }));
        }

        public void ShowAdIfAvailable()
        {
            if (this.isShowingAd)
            {
                return;
            }

            if (!this.IsAdAvailable)
            {
                this.LoadAOAAd();
            }

            this.AppOpenAd.OnAdDidDismissFullScreenContent      += this.HandleAppOpenAdDidDismissFullScreenContent;
            this.AppOpenAd.OnAdFailedToPresentFullScreenContent += this.HandleAppOpenAdFailedToPresentFullScreenContent;
            this.AppOpenAd.OnAdDidPresentFullScreenContent      += this.HandleAppOpenAdDidPresentFullScreenContent;
            this.AppOpenAd.OnAdDidRecordImpression              += this.HandleAppOpenAdDidRecordImpression;
            this.AppOpenAd.OnPaidEvent                          += this.HandlePaidEvent;

            this.AppOpenAd.Show();
        }

        private void HandleAppOpenAdDidDismissFullScreenContent(object sender, EventArgs args)
        {
            this.logService.Log("Closed app open ad");
            // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
            this.AppOpenAd   = null;
            this.isShowingAd = false;
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
            this.isShowingAd = true;
        }

        private void HandleAppOpenAdDidRecordImpression(object sender, EventArgs args) { this.logService.Log("Recorded ad impression"); }

        private void HandlePaidEvent(object sender, AdValueEventArgs args) { this.logService.Log($"Received paid event. (currency: {args.AdValue.CurrencyCode}, value: {args.AdValue.Value}"); }
#endif
    }
}