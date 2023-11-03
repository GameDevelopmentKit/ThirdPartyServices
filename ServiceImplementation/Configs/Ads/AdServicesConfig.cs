namespace Core.AdsServices
{
    using System;
    using ServiceImplementation.FireBaseRemoteConfig;
    using Zenject;

    public class AdServicesConfig : IInitializable, IDisposable
    {
        #region inject

        private readonly SignalBus           signalBus;
        private readonly IRemoteConfig       remoteConfig;
        private readonly RemoteConfigSetting remoteConfigSetting;

        #endregion

        public bool EnableBannerAd               { get; set; } = true;
        public bool EnableInterstitialAd         { get; set; } = true;
        public bool EnableMRECAd                 { get; set; } = true;
        public bool EnableAOAAd                  { get; set; } = true;
        public bool EnableRewardedAd             { get; set; } = true;
        public bool EnableRewardedInterstitialAd { get; set; } = true;
        public bool EnableNativeAd               { get; set; } = true;
        public int  IntervalLoadAds              { get; set; } = 5;

        public int InterstitialAdInterval           { get; set; } = 15;
        public int MinPauseSecondToShowAoaAd        { get; set; } = 0;
        public int DelayFirstInterstitialAdInterval { get; set; } = 45;
        public int AOAStartSession                  { get; set; } = 2;
        public int InterstitialAdStartLevel         { get; set; } = 1;

        public AdServicesConfig(SignalBus signalBus, IRemoteConfig remoteConfig, RemoteConfigSetting remoteConfigSetting)
        {
            this.signalBus           = signalBus;
            this.remoteConfig        = remoteConfig;
            this.remoteConfigSetting = remoteConfigSetting;
        }

        public void Initialize() { this.signalBus.Subscribe<RemoteConfigFetchedSucceededSignal>(this.RemoteConfigFetchedSucceededHandler); }

        public void Dispose() { this.signalBus.Unsubscribe<RemoteConfigFetchedSucceededSignal>(this.RemoteConfigFetchedSucceededHandler); }

        private void RemoteConfigFetchedSucceededHandler()
        {
            this.EnableBannerAd                   = this.remoteConfig.GetRemoteConfigBoolValue(this.remoteConfigSetting.GetRemoteKey("enable_banner_ad"), true);
            this.EnableInterstitialAd             = this.remoteConfig.GetRemoteConfigBoolValue(this.remoteConfigSetting.GetRemoteKey("enable_interstitial_ad"), true);
            this.EnableMRECAd                     = this.remoteConfig.GetRemoteConfigBoolValue(this.remoteConfigSetting.GetRemoteKey("enable_mrec_ad"), true);
            this.EnableAOAAd                      = this.remoteConfig.GetRemoteConfigBoolValue(this.remoteConfigSetting.GetRemoteKey("enable_aoa_ad"), true);
            this.EnableRewardedAd                 = this.remoteConfig.GetRemoteConfigBoolValue(this.remoteConfigSetting.GetRemoteKey("enable_rewarded_ad"), true);
            this.EnableRewardedInterstitialAd     = this.remoteConfig.GetRemoteConfigBoolValue(this.remoteConfigSetting.GetRemoteKey("enable_rewarded_interstitial_ad"), true);
            this.EnableNativeAd                   = this.remoteConfig.GetRemoteConfigBoolValue(this.remoteConfigSetting.GetRemoteKey("enable_native_ad"), true);
            this.IntervalLoadAds                  = this.remoteConfig.GetRemoteConfigIntValue(this.remoteConfigSetting.GetRemoteKey("interval_load_ads"), 5);
            this.InterstitialAdInterval           = this.remoteConfig.GetRemoteConfigIntValue(this.remoteConfigSetting.GetRemoteKey("interstitial_ad_interval"), 15);
            this.MinPauseSecondToShowAoaAd        = this.remoteConfig.GetRemoteConfigIntValue(this.remoteConfigSetting.GetRemoteKey("min_pause_second_to_show_aoa_ad"), 0);
            this.DelayFirstInterstitialAdInterval = this.remoteConfig.GetRemoteConfigIntValue(this.remoteConfigSetting.GetRemoteKey("delay_first_inters_ad_interval"), 45);
            this.AOAStartSession                  = this.remoteConfig.GetRemoteConfigIntValue(this.remoteConfigSetting.GetRemoteKey("aoa_start_session"), 2);
            this.InterstitialAdStartLevel         = this.remoteConfig.GetRemoteConfigIntValue(this.remoteConfigSetting.GetRemoteKey("interstitial_ad_start_level"), 1);
        }
    }
}