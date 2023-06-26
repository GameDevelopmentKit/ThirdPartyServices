namespace ServiceImplementation.Configs.Ads
{
    using System;
    using System.Collections.Generic;
    using ServiceImplementation.Configs.Common;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [Serializable]
    public class AdMobSettings : AdNetworkSettings
    {
        [Obsolete("AppId has been deprecated since Easy Mobile Pro version 2.6.0 because the GoogleMobileAds SDK no longer allows access to this value in runtime.")]
        /// <summary>
        /// Gets or sets the AdMob app identifier.
        /// </summary>
        public AdId AppId { get { return this.mAppId; } set { this.mAppId = value; } }

        /// <summary>
        /// Gets or sets the default banner identifier.
        /// </summary>
        public AdId DefaultBannerAdId { get { return this.mDefaultBannerAdId; } set { this.mDefaultBannerAdId = value; } }

        /// <summary>
        /// Gets or sets the default interstitial ad identifier.
        /// </summary>
        public AdId DefaultInterstitialAdId { get { return this.mDefaultInterstitialAdId; } set { this.mDefaultInterstitialAdId = value; } }

        /// <summary>
        /// Gets or sets the default rewarded ad identifier.
        /// </summary>
        public AdId DefaultRewardedAdId { get { return this.mDefaultRewardedAdId; } set { this.mDefaultRewardedAdId = value; } }

        /// <summary>
        /// Gets or sets the default rewarded interstitial ad identifier.
        /// </summary>
        public AdId DefaultRewardedInterstitialAdId { get { return this.mDefaultRewardedInterstitialAdId; } set { this.mDefaultRewardedInterstitialAdId = value; } }

        /// <summary>
        /// Enables or disables test mode.
        /// </summary>
        public bool EnableTestMode { get { return this.mEnableTestMode; } set { this.mEnableTestMode = value; } }

        /// <summary>
        /// Use google mobile ad adaptive banner when calling for smart banner
        /// </summary>
        public bool UseAdaptiveBanner { get { return this.mUseAdaptiveBanner; } set { this.mUseAdaptiveBanner = value; } }

        /// <summary>
        /// Gets or sets the list of custom banner identifiers.
        /// Each identifier is associated with an ad placement.
        /// </summary>
        public Dictionary<AdPlacement, AdId> CustomBannerAdIds { get { return this.mCustomBannerAdIds; } set { this.mCustomBannerAdIds = value as Dictionary_AdPlacement_AdId; } }

        /// <summary>
        /// Gets or sets the list of custom interstitial ad identifiers.
        /// Each identifier is associated with an ad placement.
        /// </summary>
        public Dictionary<AdPlacement, AdId> CustomInterstitialAdIds { get { return this.mCustomInterstitialAdIds; } set { this.mCustomInterstitialAdIds = value as Dictionary_AdPlacement_AdId; } }

        /// <summary>
        /// Gets or sets the list of custom rewarded ad identifiers.
        /// Each identifier is associated with an ad placement.
        /// </summary>
        public Dictionary<AdPlacement, AdId> CustomRewardedAdIds { get { return this.mCustomRewardedAdIds; } set { this.mCustomRewardedAdIds = value as Dictionary_AdPlacement_AdId; } }

        /// <summary>
        /// Gets or sets the list of custom rewarded interstitial ad identifiers.
        /// Each identifier is associated with an ad placement.
        /// </summary>
        public Dictionary<AdPlacement, AdId> CustomRewardedInterstitialAdIds
        {
            get { return this.mCustomRewardedInterstitialAdIds; }
            set { this.mCustomRewardedInterstitialAdIds = value as Dictionary_AdPlacement_AdId; }
        }

        [SerializeField] [LabelText("Enable Test Mode")]
        private bool mEnableTestMode;

        [SerializeField] [LabelText("Use Adaptive Banner")]
        private bool mUseAdaptiveBanner;

        [SerializeField] [LabelText("App Id")] private AdId mAppId;

        [SerializeField] [LabelText("Banner")] [BoxGroup("Default Id")]
        private AdId mDefaultBannerAdId;

        [SerializeField] [LabelText("Interstitial")] [BoxGroup("Default Id")]
        private AdId mDefaultInterstitialAdId;

        [SerializeField] [LabelText("Rewarded")] [BoxGroup("Default Id")]
        private AdId mDefaultRewardedAdId;

        [SerializeField] [LabelText("Rewarded Interstitial")] [BoxGroup("Default Id")]
        private AdId mDefaultRewardedInterstitialAdId;

        [SerializeField] [LabelText("Banner")] [BoxGroup("Custom Placement Id")]
        private Dictionary_AdPlacement_AdId mCustomBannerAdIds;

        [SerializeField] [LabelText("Interstitial")] [BoxGroup("Custom Placement Id")]
        private Dictionary_AdPlacement_AdId mCustomInterstitialAdIds;

        [SerializeField] [LabelText("Rewarded")] [BoxGroup("Custom Placement Id")]
        private Dictionary_AdPlacement_AdId mCustomRewardedAdIds;

        [SerializeField] [LabelText("Rewarded Interstitial")] [BoxGroup("Custom Placement Id")]
        private Dictionary_AdPlacement_AdId mCustomRewardedInterstitialAdIds;
    }
}