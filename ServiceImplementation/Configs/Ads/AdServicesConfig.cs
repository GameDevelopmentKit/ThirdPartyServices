namespace ServiceImplementation.Configs.Ads
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

        //General setting
        public int  IntervalLoadAds              { get; set; }
        public bool EnableInterstitialAd         { get; set; }
        public bool EnableBannerAd               { get; set; }
        public bool EnableMRECAd                 { get; set; }
        public bool EnableAOAAd                  { get; set; }
        public bool EnableRewardedAd             { get; set; }
        public bool EnableNativeAd               { get; set; }
        public bool EnableRewardedInterstitialAd { get; set; }

        //Interstitial ads
        public int InterstitialAdInterval           { get; set; } //The interval between two interstitial ads, we also count the rewarded interstitial ads
        public int DelayFirstInterstitialAdInterval { get; set; } //This delay will be applied for the first session
        public int DelayFirstInterNewSession        { get; set; } //From the second session, this delay will be applied
        public int InterstitialAdStartLevel         { get; set; } //The level to start showing interstitial ads

        //AOA
        public int MinPauseSecondToShowAoaAd { get; set; }
        public int AOAStartSession           { get; set; }

        public AdServicesConfig(SignalBus signalBus, IRemoteConfig remoteConfig, RemoteConfigSetting remoteConfigSetting)
        {
            this.signalBus           = signalBus;
            this.remoteConfig        = remoteConfig;
            this.remoteConfigSetting = remoteConfigSetting;

            // Init default value
            this.OnRemoteConfigFetchedSucceeded();
        }

        public void Initialize() { this.signalBus.Subscribe<RemoteConfigFetchedSucceededSignal>(this.OnRemoteConfigFetchedSucceeded); }

        public void Dispose() { this.signalBus.Unsubscribe<RemoteConfigFetchedSucceededSignal>(this.OnRemoteConfigFetchedSucceeded); }

        private void OnRemoteConfigFetchedSucceeded()
        {
            //General setting
            var intervalLoadAdsSetting              = this.remoteConfigSetting.GetRemoteConfig("interval_load_ads");
            var enableBannerADSetting               = this.remoteConfigSetting.GetRemoteConfig("enable_banner_ad");
            var enableMrecADSetting                 = this.remoteConfigSetting.GetRemoteConfig("enable_mrec_ad");
            var enableRewardedADSetting             = this.remoteConfigSetting.GetRemoteConfig("enable_rewarded_ad");
            var enableNativeADSetting               = this.remoteConfigSetting.GetRemoteConfig("enable_native_ad");
            var enableInterstitialADSetting         = this.remoteConfigSetting.GetRemoteConfig("enable_interstitial_ad");
            var enableRewardedInterstitialADSetting = this.remoteConfigSetting.GetRemoteConfig("enable_rewarded_interstitial_ad");
            var interstitialADIntervalSetting       = this.remoteConfigSetting.GetRemoteConfig("interstitial_ad_interval");
            var delayFirstIntersADIntervalSetting   = this.remoteConfigSetting.GetRemoteConfig("delay_first_inters_ad_interval");
            var delayFirstIntersNewSessionSetting   = this.remoteConfigSetting.GetRemoteConfig("delay_first_inters_new_session");
            var interstitialADStartLevelSetting     = this.remoteConfigSetting.GetRemoteConfig("interstitial_ad_start_level");
            var enableAoaADSetting                  = this.remoteConfigSetting.GetRemoteConfig("enable_aoa_ad");
            var minPauseSecondToShowAoaADSetting    = this.remoteConfigSetting.GetRemoteConfig("min_pause_second_to_show_aoa_ad");
            var aoaStartSessionSetting              = this.remoteConfigSetting.GetRemoteConfig("aoa_start_session");

            this.IntervalLoadAds                  = this.remoteConfig.GetRemoteConfigIntValue(intervalLoadAdsSetting.key, int.Parse(intervalLoadAdsSetting.defaultValue));
            this.EnableBannerAd                   = this.remoteConfig.GetRemoteConfigBoolValue(enableBannerADSetting.key, bool.Parse(enableBannerADSetting.defaultValue));
            this.EnableMRECAd                     = this.remoteConfig.GetRemoteConfigBoolValue(enableMrecADSetting.key, bool.Parse(enableMrecADSetting.defaultValue));
            this.EnableRewardedAd                 = this.remoteConfig.GetRemoteConfigBoolValue(enableRewardedADSetting.key, bool.Parse(enableRewardedADSetting.defaultValue));
            this.EnableNativeAd                   = this.remoteConfig.GetRemoteConfigBoolValue(enableNativeADSetting.key, bool.Parse(enableNativeADSetting.defaultValue));
            this.EnableInterstitialAd             = this.remoteConfig.GetRemoteConfigBoolValue(enableInterstitialADSetting.key, bool.Parse(enableInterstitialADSetting.defaultValue));
            this.EnableRewardedInterstitialAd     = this.remoteConfig.GetRemoteConfigBoolValue(enableRewardedInterstitialADSetting.key, bool.Parse(enableRewardedInterstitialADSetting.defaultValue));
            this.InterstitialAdInterval           = this.remoteConfig.GetRemoteConfigIntValue(interstitialADIntervalSetting.key, int.Parse(interstitialADIntervalSetting.defaultValue));
            this.DelayFirstInterstitialAdInterval = this.remoteConfig.GetRemoteConfigIntValue(delayFirstIntersADIntervalSetting.key, int.Parse(delayFirstIntersADIntervalSetting.defaultValue));
            this.DelayFirstInterNewSession        = this.remoteConfig.GetRemoteConfigIntValue(delayFirstIntersNewSessionSetting.key, int.Parse(delayFirstIntersNewSessionSetting.defaultValue));
            this.InterstitialAdStartLevel         = this.remoteConfig.GetRemoteConfigIntValue(interstitialADStartLevelSetting.key, int.Parse(interstitialADStartLevelSetting.defaultValue));
            this.EnableAOAAd                      = this.remoteConfig.GetRemoteConfigBoolValue(enableAoaADSetting.key, bool.Parse(enableAoaADSetting.defaultValue));
            this.MinPauseSecondToShowAoaAd        = this.remoteConfig.GetRemoteConfigIntValue(minPauseSecondToShowAoaADSetting.key, int.Parse(minPauseSecondToShowAoaADSetting.defaultValue));
            this.AOAStartSession                  = this.remoteConfig.GetRemoteConfigIntValue(aoaStartSessionSetting.key, int.Parse(aoaStartSessionSetting.defaultValue));
        }
    }
}