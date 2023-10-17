namespace Core.AdsServices
{
    using System;
    using ServiceImplementation.FireBaseRemoteConfig;
    using Zenject;

    public class AdServicesConfig : IInitializable, IDisposable
    {
        #region inject

        private readonly SignalBus     signalBus;
        private readonly IRemoteConfig remoteConfig;

        #endregion
        
        public bool EnableBannerAd               { get; set; } = true;
        public bool EnableInterstitialAd         { get; set; } = true;
        public bool EnableMRECAd                 { get; set; } = true;
        public bool EnableAOAAd                  { get; set; } = true;
        public bool EnableRewardedAd             { get; set; } = true;
        public bool EnableRewardedInterstitialAd { get; set; } = true;
        public bool EnableNativeAd               { get; set; } = true;
        public int  IntervalLoadAds              { get; set; } = 5;

        public int InterstitialAdInterval    { get; set; } = 10;
        public int MinPauseSecondToShowAoaAd { get; set; } = 0;

        public AdServicesConfig(SignalBus signalBus, IRemoteConfig remoteConfig)
        {
            this.signalBus    = signalBus;
            this.remoteConfig = remoteConfig;
        }
        
        public void Initialize()
        {
            this.signalBus.Subscribe<RemoteConfigFetchedSucceededSignal>(this.RemoteConfigFetchedSucceededHandler);
        }
        public void Dispose()
        {
            this.signalBus.Unsubscribe<RemoteConfigFetchedSucceededSignal>(this.RemoteConfigFetchedSucceededHandler);
        }

        private void RemoteConfigFetchedSucceededHandler()
        {
            this.EnableBannerAd = this.remoteConfig.GetRemoteConfigBoolValue("enable_banner_ad", true);
            this.EnableInterstitialAd = this.remoteConfig.GetRemoteConfigBoolValue("enable_interstitial_ad", true);
            this.EnableMRECAd = this.remoteConfig.GetRemoteConfigBoolValue("enable_mrec_ad", true);
            this.EnableAOAAd = this.remoteConfig.GetRemoteConfigBoolValue("enable_aoa_ad", true);
            this.EnableRewardedAd = this.remoteConfig.GetRemoteConfigBoolValue("enable_rewarded_ad", true);
            this.EnableRewardedInterstitialAd = this.remoteConfig.GetRemoteConfigBoolValue("enable_rewarded_interstitial_ad", true);
            this.EnableNativeAd = this.remoteConfig.GetRemoteConfigBoolValue("enable_native_ad", true);
            this.IntervalLoadAds = this.remoteConfig.GetRemoteConfigIntValue("interval_load_ads", 5);
            this.InterstitialAdInterval = this.remoteConfig.GetRemoteConfigIntValue("interstitial_ad_interval", 10);
            this.MinPauseSecondToShowAoaAd = this.remoteConfig.GetRemoteConfigIntValue("min_pause_second_to_show_aoa_ad", 0);
        }
    }
}