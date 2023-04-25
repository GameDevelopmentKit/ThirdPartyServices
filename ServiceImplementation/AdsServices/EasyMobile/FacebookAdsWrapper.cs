namespace ServiceImplementation.AdsServices.EasyMobile
{
    using System;
    using Core.AdsServices;
    using GameFoundation.Scripts.Utilities.LogService;
    using Packages.com.gdk._3rd.Plugins.WebGL;
    using Zenject;

    public class FacebookAdsWrapper : IAdServices, IInitializable, IDisposable
    {
        private readonly ILogService      logService;
        private readonly SignalBus        signalBus;
        private readonly AdServicesConfig adServicesConfig;
        
        public FacebookAdsWrapper(ILogService logService,SignalBus signalBus, AdServicesConfig adServicesConfig)
        {
            this.logService       = logService;
            this.signalBus        = signalBus;
            this.adServicesConfig = adServicesConfig;
        }
        public void          Initialize()                                                                                                   { throw new NotImplementedException(); }
        public void          Dispose()                                                                                                      { throw new NotImplementedException(); }
        public void          GrantDataPrivacyConsent()                                                                                      { throw new NotImplementedException(); }
        public void          RevokeDataPrivacyConsent()                                                                                     { throw new NotImplementedException(); }
        public void          GrantDataPrivacyConsent(AdNetwork adNetwork)                                                                   { throw new NotImplementedException(); }
        public void          RevokeDataPrivacyConsent(AdNetwork adNetwork)                                                                  { throw new NotImplementedException(); }
        public ConsentStatus GetDataPrivacyConsent(AdNetwork adNetwork)                                                                     { throw new NotImplementedException(); }
        public void ShowBannerAd(BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom, int width = 320, int height = 50)
        {
            if (!this.adServicesConfig.EnableBannerAd)
            {
                return;
            }
            this.logService.Log("Enter facebook ad id here!", LogLevel.WARNING);
            FBInstant.ShowBannerAd("");
        }
        public void HideBannedAd()
        {
            FBInstant.HideBannerAd();
        }
        public void DestroyBannerAd()                                            { this.logService.Log("Use hide banner ad method instead!", LogLevel.EXCEPTION); }
        public bool IsInterstitialAdReady(string place)                          { return FBInstant.PreloadInterstitialAd(place); }
        public void ShowInterstitialAd(string place)
        {
            if (!this.IsInterstitialAdReady(place)) return;
            this.logService.LogWithColor("");
        }
        public bool IsRewardedAdReady(string place)                              { throw new NotImplementedException(); }
        public void ShowRewardedAd(string place)                                 { throw new NotImplementedException(); }
        public void ShowRewardedAd(string place, Action onCompleted)             { throw new NotImplementedException(); }
        public bool IsRewardedInterstitialAdReady()                              { throw new NotImplementedException(); }
        public void ShowRewardedInterstitialAd(string place)                     { throw new NotImplementedException(); }
        public void ShowRewardedInterstitialAd(string place, Action onCompleted) { throw new NotImplementedException(); }
        public void RemoveAds(bool revokeConsent = false)                        { throw new NotImplementedException(); }
        public bool IsAdsInitialized()                                           { throw new NotImplementedException(); }
        public bool IsRemoveAds()                                                { throw new NotImplementedException(); }
                                                                                                           
    }
}