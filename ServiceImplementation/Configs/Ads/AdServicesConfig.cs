namespace ServiceImplementation.Configs.Ads
{
    using System;
    using ServiceImplementation.FireBaseRemoteConfig;
    using UnityEngine;
    using Zenject;

    public class AdServicesConfig : IInitializable, IDisposable
    {
        #region inject

        private readonly SignalBus           signalBus;
        private readonly IRemoteConfig       remoteConfig;
        private readonly RemoteConfigSetting remoteConfigSetting;

        #endregion

        //General setting
        public bool EnableBannerAd               { get; set; }
        public bool EnableInterstitialAd         { get; set; }
        public bool EnableMRECAd                 { get; set; }
        public bool EnableAOAAd                  { get; set; }
        public bool EnableRewardedAd             { get; set; }
        public bool EnableRewardedInterstitialAd { get; set; }
        public bool EnableNativeAd               { get; set; }
        public int  IntervalLoadAds              { get; set; }

        //AOA
        public int MinPauseSecondToShowAoaAd { get; set; }
        public int AOAStartSession           { get; set; }

        //Interstitial ads
        public int InterstitialAdInterval           { get; set; } //The interval between two interstitial ads, we also count the rewarded interstitial ads
        public int InterstitialAdStartLevel         { get; set; } //The level to start showing interstitial ads
        public int DelayFirstInterstitialAdInterval { get; set; } //This delay will be applied for the first session
        public int DelayFirstInterNewSession        { get; set; } //From the second session, this delay will be applied


        public AdServicesConfig(SignalBus signalBus, IRemoteConfig remoteConfig, RemoteConfigSetting remoteConfigSetting)
        {
            this.signalBus           = signalBus;
            this.remoteConfig        = remoteConfig;
            this.remoteConfigSetting = remoteConfigSetting;
        }

        public void Initialize()
        {
            this.signalBus.Subscribe<RemoteConfigFetchedSucceededSignal>(this.OnRemoteConfigFetchedSucceeded);

            // Init default value
            this.InitDefaultValue();
        }

        public void Dispose() { this.signalBus.Unsubscribe<RemoteConfigFetchedSucceededSignal>(this.OnRemoteConfigFetchedSucceeded); }

        private void InitDefaultValue()
        {
            this.EnableBannerAd                   = RemoteConfigHelpers.GetBoolDefaultValue(this.remoteConfigSetting, RemoteConfigKey.EnableBannerAD);
            this.EnableInterstitialAd             = RemoteConfigHelpers.GetBoolDefaultValue(this.remoteConfigSetting, RemoteConfigKey.EnableInterstitialAD);
            this.EnableMRECAd                     = RemoteConfigHelpers.GetBoolDefaultValue(this.remoteConfigSetting, RemoteConfigKey.EnableMrecAD);
            this.EnableAOAAd                      = RemoteConfigHelpers.GetBoolDefaultValue(this.remoteConfigSetting, RemoteConfigKey.EnableAoaAD);
            this.EnableRewardedAd                 = RemoteConfigHelpers.GetBoolDefaultValue(this.remoteConfigSetting, RemoteConfigKey.EnableRewardedAD);
            this.EnableRewardedInterstitialAd     = RemoteConfigHelpers.GetBoolDefaultValue(this.remoteConfigSetting, RemoteConfigKey.EnableRewardedInterstitialAD);
            this.EnableNativeAd                   = RemoteConfigHelpers.GetBoolDefaultValue(this.remoteConfigSetting, RemoteConfigKey.EnableNativeAD);
            this.IntervalLoadAds                  = RemoteConfigHelpers.GetIntDefaultValue(this.remoteConfigSetting, RemoteConfigKey.IntervalLoadAds);
            this.MinPauseSecondToShowAoaAd        = RemoteConfigHelpers.GetIntDefaultValue(this.remoteConfigSetting, RemoteConfigKey.MinPauseSecondToShowAoaAD);
            this.AOAStartSession                  = RemoteConfigHelpers.GetIntDefaultValue(this.remoteConfigSetting, RemoteConfigKey.AoaStartSession);
            this.InterstitialAdInterval           = RemoteConfigHelpers.GetIntDefaultValue(this.remoteConfigSetting, RemoteConfigKey.InterstitialADInterval);
            this.InterstitialAdStartLevel         = RemoteConfigHelpers.GetIntDefaultValue(this.remoteConfigSetting, RemoteConfigKey.InterstitialADStartLevel);
            this.DelayFirstInterstitialAdInterval = RemoteConfigHelpers.GetIntDefaultValue(this.remoteConfigSetting, RemoteConfigKey.DelayFirstIntersADInterval);
            this.DelayFirstInterNewSession        = RemoteConfigHelpers.GetIntDefaultValue(this.remoteConfigSetting, RemoteConfigKey.DelayFirstIntersNewSession);
        }

        private void OnRemoteConfigFetchedSucceeded()
        {
            this.EnableBannerAd                   = RemoteConfigHelpers.GetBoolRemoteValue(this.remoteConfig, this.remoteConfigSetting, RemoteConfigKey.EnableBannerAD);
            this.EnableInterstitialAd             = RemoteConfigHelpers.GetBoolRemoteValue(this.remoteConfig, this.remoteConfigSetting, RemoteConfigKey.EnableInterstitialAD);
            this.EnableMRECAd                     = RemoteConfigHelpers.GetBoolRemoteValue(this.remoteConfig, this.remoteConfigSetting, RemoteConfigKey.EnableMrecAD);
            this.EnableAOAAd                      = RemoteConfigHelpers.GetBoolRemoteValue(this.remoteConfig, this.remoteConfigSetting, RemoteConfigKey.EnableAoaAD);
            this.EnableRewardedAd                 = RemoteConfigHelpers.GetBoolRemoteValue(this.remoteConfig, this.remoteConfigSetting, RemoteConfigKey.EnableRewardedAD);
            this.EnableRewardedInterstitialAd     = RemoteConfigHelpers.GetBoolRemoteValue(this.remoteConfig, this.remoteConfigSetting, RemoteConfigKey.EnableRewardedInterstitialAD);
            this.EnableNativeAd                   = RemoteConfigHelpers.GetBoolRemoteValue(this.remoteConfig, this.remoteConfigSetting, RemoteConfigKey.EnableNativeAD);
            this.IntervalLoadAds                  = RemoteConfigHelpers.GetIntRemoteValue(this.remoteConfig, this.remoteConfigSetting, RemoteConfigKey.IntervalLoadAds);
            this.MinPauseSecondToShowAoaAd        = RemoteConfigHelpers.GetIntRemoteValue(this.remoteConfig, this.remoteConfigSetting, RemoteConfigKey.MinPauseSecondToShowAoaAD);
            this.AOAStartSession                  = RemoteConfigHelpers.GetIntRemoteValue(this.remoteConfig, this.remoteConfigSetting, RemoteConfigKey.AoaStartSession);
            this.InterstitialAdInterval           = RemoteConfigHelpers.GetIntRemoteValue(this.remoteConfig, this.remoteConfigSetting, RemoteConfigKey.InterstitialADInterval);
            this.InterstitialAdStartLevel         = RemoteConfigHelpers.GetIntRemoteValue(this.remoteConfig, this.remoteConfigSetting, RemoteConfigKey.InterstitialADStartLevel);
            this.DelayFirstInterstitialAdInterval = RemoteConfigHelpers.GetIntRemoteValue(this.remoteConfig, this.remoteConfigSetting, RemoteConfigKey.DelayFirstIntersADInterval);
            this.DelayFirstInterNewSession        = RemoteConfigHelpers.GetIntRemoteValue(this.remoteConfig, this.remoteConfigSetting, RemoteConfigKey.DelayFirstIntersNewSession);
        }
    }
}