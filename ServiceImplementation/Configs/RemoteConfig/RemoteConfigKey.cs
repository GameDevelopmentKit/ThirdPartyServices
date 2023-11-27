﻿namespace ServiceImplementation.FireBaseRemoteConfig
{
    public static class RemoteConfigKey
    {
        public const string EnableBannerAD               = "enable_banner_ad";
        public const string EnableInterstitialAD         = "enable_interstitial_ad";
        public const string EnableMrecAD                 = "enable_mrec_ad";
        public const string EnableAoaAD                  = "enable_aoa_ad";
        public const string EnableRewardedAD             = "enable_rewarded_ad";
        public const string EnableRewardedInterstitialAD = "enable_rewarded_interstitial_ad";
        public const string EnableNativeAD               = "enable_native_ad";
        public const string IntervalLoadAds              = "interval_load_ads";
        public const string MinPauseSecondToShowAoaAD    = "min_pause_second_to_show_aoa_ad";
        public const string AoaStartSession              = "aoa_start_session";
        public const string InterstitialADInterval       = "interstitial_ad_interval";
        public const string InterstitialADStartLevel     = "interstitial_ad_start_level";
        public const string DelayFirstIntersADInterval   = "delay_first_inters_ad_interval";
        public const string DelayFirstIntersNewSession   = "delay_first_inters_new_session";
        public const string EnableUmp                    = "enable_ump";
    }
}