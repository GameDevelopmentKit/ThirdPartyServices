namespace ServiceImplementation.Configs.Ads
{
    using System;
    using System.Collections.Generic;
    using ServiceImplementation.Configs.Common;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [Serializable]
    public class AppLovinSettings : AdNetworkSettings
    {
        /// <summary>
        /// Gets or sets the AppLovin SDKKey.
        /// </summary>
        public string SDKKey { get { return this.mSDKKey; } set { this.mSDKKey = value; } }

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
        /// Gets or sets the default AOA ad identifier.
        /// </summary>
        public AdId DefaultAOAAdId { get { return this.mAOAAdId; } set { this.mAOAAdId = value; } }

        /// <summary>
        /// age-restricted category.
        /// </summary>
        public bool AgeRestrictMode { get { return this.mAgeRestrictMode; } set { this.mAgeRestrictMode = value; } }

        /// <summary>
        /// Gets or sets the list of custom banner identifiers.
        /// Each identifier is associated with an ad placement.
        /// </summary>
        public override Dictionary<AdPlacement, AdId> CustomBannerAdIds { get { return this.mCustomBannerAdIds; } set { this.mCustomBannerAdIds = value as Dictionary_AdPlacement_AdId; } }

        /// <summary>
        /// Gets or sets the list of custom interstitial ad identifiers.
        /// Each identifier is associated with an ad placement.
        /// </summary>
        public override Dictionary<AdPlacement, AdId> CustomInterstitialAdIds { get { return this.mCustomInterstitialAdIds; } set { this.mCustomInterstitialAdIds = value as Dictionary_AdPlacement_AdId; } }

        /// <summary>
        /// Gets or sets the list of custom rewarded ad identifiers.
        /// Each identifier is associated with an ad placement.
        /// </summary>
        public override Dictionary<AdPlacement, AdId> CustomRewardedAdIds { get { return this.mCustomRewardedAdIds; } set { this.mCustomRewardedAdIds = value as Dictionary_AdPlacement_AdId; } }

        [SerializeField] [LabelText("AgeRestrictMode")]
        private bool mAgeRestrictMode;

        [SerializeField] [LabelText("SDK Key")]
        private string mSDKKey;

        [SerializeField] [LabelText("Banner")] [BoxGroup("Default Id")]
        private AdId mDefaultBannerAdId;

        [SerializeField] [LabelText("Interstitial")] [BoxGroup("Default Id")]
        private AdId mDefaultInterstitialAdId;

        [SerializeField] [LabelText("Rewarded")] [BoxGroup("Default Id")]
        private AdId mDefaultRewardedAdId;
        
        [SerializeField] [LabelText("AOA")] [BoxGroup("Default Id")]
        private AdId mAOAAdId;

        [SerializeField] [LabelText("Banner")] [BoxGroup("Custom Placement Id")]
        private Dictionary_AdPlacement_AdId mCustomBannerAdIds;

        [SerializeField] [LabelText("Interstitial")] [BoxGroup("Custom Placement Id")]
        private Dictionary_AdPlacement_AdId mCustomInterstitialAdIds;

        [SerializeField] [LabelText("Rewarded")] [BoxGroup("Custom Placement Id")]
        private Dictionary_AdPlacement_AdId mCustomRewardedAdIds;
    }
}