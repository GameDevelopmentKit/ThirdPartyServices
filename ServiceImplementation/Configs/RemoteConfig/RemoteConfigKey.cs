namespace ServiceImplementation.FireBaseRemoteConfig
{
    public static class RemoteConfigKey
    {
        #region General

        public const string EnableBannerAD               = "enable_banner_ad";
        public const string EnableInterstitialAD         = "enable_interstitial_ad";
        public const string EnableMrecAD                 = "enable_mrec_ad";
        public const string EnableAoaAD                  = "enable_aoa_ad";
        public const string EnableRewardedAD             = "enable_rewarded_ad";
        public const string EnableRewardedInterstitialAD = "enable_rewarded_interstitial_ad";
        public const string EnableNativeAD               = "enable_native_ad";
        public const string EnableCollapsibleBanner      = "enable_collapsible_banner";
        public const string IntervalLoadAds              = "interval_load_ads";

        #endregion

        #region AOA

        public const string MinPauseSecondToShowAoaAD = "min_pause_second_to_show_aoa_ad";
        public const string AoaStartSession           = "aoa_start_session";
        public const string UseAoaAdmob               = "use_aoa_admob";

        #endregion

        #region Interstitial

        public const string InterstitialADInterval            = "interstitial_ad_interval";
        public const string InterstitialADStartLevel          = "interstitial_ad_start_level";
        public const string InterstitialAdActivePlacements    = "interstitial_ad_active_placements";
        public const string DelayFirstIntersADInterval        = "delay_first_inters_ad_interval";
        public const string DelayFirstIntersNewSession        = "delay_first_inters_new_session";
        public const string ResetInterAdIntervalAfterRewardAd = "reset_inter_ad_interval_after_reward_ad";

        #endregion

        #region Rewarded

        public const string RewardedAdFreePlacements = "Rewared_ad_free_placements";

        #endregion

        #region Collapsible

        public const string CollapsibleBannerADInterval             = "colapsible_banner_ad_interval";
        public const string EnableCollapsibleBannerFallback         = "enable_collapsible_banner_fallback";
        public const string DelayFirstBanner             = "delay_first_banner";
        public const string CollapsibleBannerAutoRefreshEnabled     = "collapsible_banner_auto_refresh_enabled";
        public const string CollapsibleBannerExpandOnRefreshEnabled = "collapsible_banner_expand_on_refresh_enabled";

        #endregion
    }
}