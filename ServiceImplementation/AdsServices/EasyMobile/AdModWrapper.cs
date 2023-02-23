namespace ServiceImplementation.AdsServices.EasyMobile
{
#if EM_ADMOB
    using Core.AdsServices;
    using GameFoundation.Scripts.Utilities.LogService;
    using System;
    using GoogleMobileAds.Api;
    using UnityEngine;

    public class AdModWrapper : IAOAAdService
    {
        #region inject

        private readonly ILogService logService;
        private readonly string      adModAoaId;

        #endregion

        public AdModWrapper(ILogService logService, string adModAOAId)
        {
            this.logService = logService;
            this.adModAoaId = adModAOAId;
        }

        public event Action AppOpenOpened;

        public AppOpenAd AppOpenAd { get; private set; }


        public bool IsAppOpenAdLoaded() { return this.AppOpenAd != null && this.AppOpenAd.CanShowAd(); }

        public void ShowAppOpenAd() { this.AppOpenAd.Show(); }

        private void LoadAppOpenAd()
        {
            // Clean up the old ad before loading a new one.
            if (this.AppOpenAd != null)
            {
                this.AppOpenAd.Destroy();
                this.AppOpenAd = null;
            }

            this.logService.Log("Loading the app open ad.");

            // Create our request used to load the ad.
            var adRequest = new AdRequest.Builder().Build();

            // send the request to load the ad.
            AppOpenAd.Load(this.adModAoaId, ScreenOrientation.Portrait, adRequest, (AppOpenAd ad, LoadAdError error) =>
                                                                                   {
                                                                                       // if error is not null, the load request failed.
                                                                                       if (error != null || ad == null)
                                                                                       {
                                                                                           this.logService.Error("app open ad failed to load an ad " + "with error : " + error);
                                                                                           return;
                                                                                       }

                                                                                       this.logService.Log("App open ad loaded with response : " + ad.GetResponseInfo());

                                                                                       this.AppOpenAd = ad;
                                                                                       this.RegisterEventHandlers(ad);
                                                                                   });
        }

        private void RegisterEventHandlers(AppOpenAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) => { this.logService.Log(String.Format("App open ad paid {0} {1}.", adValue.Value, adValue.CurrencyCode)); };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () => { this.logService.Log("App open ad recorded an impression."); };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () => { this.logService.Log("App open ad was clicked."); };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
                                              {
                                                  this.logService.Log("App open ad full screen content opened.");
                                                  this.AppOpenOpened?.Invoke();
                                              };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () => { this.logService.Log("App open ad full screen content closed."); };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) => { this.logService.Error("App open ad failed to open full screen content " + "with error : " + error); };
        }
    }
#endif
}