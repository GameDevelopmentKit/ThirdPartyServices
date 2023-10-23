namespace ServiceImplementation.Configs.Ads
{
    using System;
    using System.Collections.Generic;
    using ServiceImplementation.Configs.Common;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.Serialization;

    [Serializable]
    public class AdMobSettings : AdNetworkSettings
    {
        /// <summary>
        /// Gets or sets the default banner identifier.
        /// </summary>
        public AdId DefaultBannerAdId { get => this.mDefaultBannerAdId; set => this.mDefaultBannerAdId = value; }

        /// <summary>
        /// Gets or sets the default interstitial ad identifier.
        /// </summary>
        public AdId DefaultInterstitialAdId { get => this.mDefaultInterstitialAdId; set => this.mDefaultInterstitialAdId = value; }

        /// <summary>
        /// Gets or sets the default rewarded ad identifier.
        /// </summary>
        public AdId DefaultRewardedAdId { get => this.mDefaultRewardedAdId; set => this.mDefaultRewardedAdId = value; }

        /// <summary>
        /// Gets or sets the default rewarded interstitial ad identifier.
        /// </summary>
        public AdId DefaultRewardedInterstitialAdId { get => this.mDefaultRewardedInterstitialAdId; set => this.mDefaultRewardedInterstitialAdId = value; }
        
        /// <summary>
        /// Gets or sets the default AOA ad identifier.
        /// </summary>
        public AdId AOAAdId { get => this.mAoaAdId; set => this.mAoaAdId = value; }
        
        /// <summary>
        /// Gets or sets the default native ad identifier.
        /// </summary>
        public List<AdId> NativeAdIds { get => this.mNativeAdIds; set => this.mNativeAdIds = value; }

        /// <summary>
        /// Enables or disables test mode.
        /// </summary>
        public bool EnableTestMode { get => this.mEnableTestMode; set => this.mEnableTestMode = value; }

        /// <summary>
        /// Gets or sets the list of custom banner identifiers.
        /// Each identifier is associated with an ad placement.
        /// </summary>
        public override Dictionary<AdPlacement, AdId> CustomBannerAdIds { get => this.mCustomBannerAdIds; set => this.mCustomBannerAdIds = value as Dictionary_AdPlacement_AdId; }

        /// <summary>
        /// Gets or sets the list of custom interstitial ad identifiers.
        /// Each identifier is associated with an ad placement.
        /// </summary>
        public override Dictionary<AdPlacement, AdId> CustomInterstitialAdIds { get => this.mCustomInterstitialAdIds; set => this.mCustomInterstitialAdIds = value as Dictionary_AdPlacement_AdId; }

        /// <summary>
        /// Gets or sets the list of custom rewarded ad identifiers.
        /// Each identifier is associated with an ad placement.
        /// </summary>
        public override Dictionary<AdPlacement, AdId> CustomRewardedAdIds { get => this.mCustomRewardedAdIds; set => this.mCustomRewardedAdIds = value as Dictionary_AdPlacement_AdId; }

        /// <summary>
        /// Gets or sets the list of custom rewarded interstitial ad identifiers.
        /// Each identifier is associated with an ad placement.
        /// </summary>
        public Dictionary<AdPlacement, AdId> CustomRewardedInterstitialAdIds
        {
            get => this.mCustomRewardedInterstitialAdIds;
            set => this.mCustomRewardedInterstitialAdIds = value as Dictionary_AdPlacement_AdId;
        }

        [SerializeField] [LabelText("Enable Test Mode")]
        private bool mEnableTestMode;

        [SerializeField] [LabelText("Banner")] [BoxGroup("Default Id")]
        private AdId mDefaultBannerAdId;

        [SerializeField] [LabelText("Interstitial")] [BoxGroup("Default Id")]
        private AdId mDefaultInterstitialAdId;

        [SerializeField] [LabelText("Rewarded")] [BoxGroup("Default Id")]
        private AdId mDefaultRewardedAdId;

        [SerializeField] [LabelText("Rewarded Interstitial")] [BoxGroup("Default Id")]
        private AdId mDefaultRewardedInterstitialAdId;
        
        [SerializeField] [LabelText("AOA")] [BoxGroup("Default Id")]
        private AdId mAoaAdId;
        
        [FormerlySerializedAs("mNativeAdId")] [SerializeField] [LabelText("Native")] [BoxGroup("Default Id")]
        private List<AdId> mNativeAdIds;

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