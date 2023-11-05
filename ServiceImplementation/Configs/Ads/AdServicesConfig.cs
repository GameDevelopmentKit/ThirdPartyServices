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
        public int  IntervalLoadAds              { get; set; } = 5;
        public bool EnableInterstitialAd         { get; set; } = true;
        public bool EnableBannerAd               { get; set; } = true;
        public bool EnableMRECAd                 { get; set; } = true;
        public bool EnableAOAAd                  { get; set; } = true;
        public bool EnableRewardedAd             { get; set; } = true;
        public bool EnableNativeAd               { get; set; } = true;
        public bool EnableRewardedInterstitialAd { get; set; } = true;


        //Interstitial ads
        public int  InterstitialAdInterval           { get; set; } = 15; //The interval between two interstitial ads, we also count the rewarded interstitial ads
        public int  DelayFirstInterstitialAdInterval { get; set; } = 0; //This delay will be applied for the first session
        public int  DelayFirstInterNewSession        { get; set; } = 0; //From the second session, this delay will be applied
        public int  InterstitialAdStartLevel         { get; set; } = 1; //The level to start showing interstitial ads


        //AOA
        public int MinPauseSecondToShowAoaAd { get; set; } = 0;
        public int AOAStartSession           { get; set; } = 2;

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
            //General setting
            this.IntervalLoadAds                  = this.remoteConfig.GetRemoteConfigIntValue(this.remoteConfigSetting.GetRemoteKey("interval_load_ads"), 5);
            this.EnableBannerAd                   = this.remoteConfig.GetRemoteConfigBoolValue(this.remoteConfigSetting.GetRemoteKey("enable_banner_ad"), true);
            this.EnableMRECAd                     = this.remoteConfig.GetRemoteConfigBoolValue(this.remoteConfigSetting.GetRemoteKey("enable_mrec_ad"), true);
            this.EnableRewardedAd                 = this.remoteConfig.GetRemoteConfigBoolValue(this.remoteConfigSetting.GetRemoteKey("enable_rewarded_ad"), true);
            this.EnableNativeAd                   = this.remoteConfig.GetRemoteConfigBoolValue(this.remoteConfigSetting.GetRemoteKey("enable_native_ad"), true);
            this.EnableInterstitialAd             = this.remoteConfig.GetRemoteConfigBoolValue(this.remoteConfigSetting.GetRemoteKey("enable_interstitial_ad"), true);
            
            //Interstitial
            this.EnableRewardedInterstitialAd     = this.remoteConfig.GetRemoteConfigBoolValue(this.remoteConfigSetting.GetRemoteKey("enable_rewarded_interstitial_ad"), true);
            this.InterstitialAdInterval           = this.remoteConfig.GetRemoteConfigIntValue(this.remoteConfigSetting.GetRemoteKey("interstitial_ad_interval"), 15);
            this.DelayFirstInterstitialAdInterval = this.remoteConfig.GetRemoteConfigIntValue(this.remoteConfigSetting.GetRemoteKey("delay_first_inters_ad_interval"), 0);
            this.DelayFirstInterNewSession        = this.remoteConfig.GetRemoteConfigIntValue(this.remoteConfigSetting.GetRemoteKey("delay_first_inters_new_session"), 0);
            this.InterstitialAdStartLevel         = this.remoteConfig.GetRemoteConfigIntValue(this.remoteConfigSetting.GetRemoteKey("interstitial_ad_start_level"), 1);
            
            //AOA
            this.EnableAOAAd                      = this.remoteConfig.GetRemoteConfigBoolValue(this.remoteConfigSetting.GetRemoteKey("enable_aoa_ad"), true);
            this.MinPauseSecondToShowAoaAd        = this.remoteConfig.GetRemoteConfigIntValue(this.remoteConfigSetting.GetRemoteKey("min_pause_second_to_show_aoa_ad"), 0);
            this.AOAStartSession                  = this.remoteConfig.GetRemoteConfigIntValue(this.remoteConfigSetting.GetRemoteKey("aoa_start_session"), 2);
        }
    }
}