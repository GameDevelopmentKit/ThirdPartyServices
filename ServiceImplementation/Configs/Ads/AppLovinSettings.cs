namespace ServiceImplementation.Configs.Ads
{
    using System;
    using System.Collections.Generic;
    using ServiceImplementation.Configs.Common;
    using UnityEngine;

    [Serializable]
    public class AppLovinSettings : AdNetworkSettings
    {
        /// <summary>
        /// Gets or sets the AppLovin SDKKey.
        /// </summary>
        public string SDKKey
        {
            get { return this.mSDKKey; }
            set { this.mSDKKey = value; }
        }

        /// <summary>
        /// Gets or sets the default banner identifier.
        /// </summary>
        public AdId DefaultBannerAdId
        {
            get { return this.mDefaultBannerAdId; }
            set { this.mDefaultBannerAdId = value; }
        }

        /// <summary>
        /// Gets or sets the default interstitial ad identifier.
        /// </summary>
        public AdId DefaultInterstitialAdId
        {
            get { return this.mDefaultInterstitialAdId; }
            set { this.mDefaultInterstitialAdId = value; }
        }

        /// <summary>
        /// Gets or sets the default rewarded ad identifier.
        /// </summary>
        public AdId DefaultRewardedAdId
        {
            get { return this.mDefaultRewardedAdId; }
            set { this.mDefaultRewardedAdId = value; }
        }

        /// <summary>
        /// age-restricted category.
        /// </summary>
        public bool AgeRestrictMode
        {
            get { return this.mAgeRestrictMode; }
            set { this.mAgeRestrictMode = value; }
        }

        /// <summary>
        /// Gets or sets the list of custom banner identifiers.
        /// Each identifier is associated with an ad placement.
        /// </summary>
        public Dictionary<AdPlacement, AdId> CustomBannerAdIds
        {
            get { return this.mCustomBannerAdIds; }
            set { this.mCustomBannerAdIds = value as Dictionary_AdPlacement_AdId; }
        }

        /// <summary>
        /// Gets or sets the list of custom interstitial ad identifiers.
        /// Each identifier is associated with an ad placement.
        /// </summary>
        public Dictionary<AdPlacement, AdId> CustomInterstitialAdIds
        {
            get { return this.mCustomInterstitialAdIds; }
            set { this.mCustomInterstitialAdIds = value as Dictionary_AdPlacement_AdId; }
        }

        /// <summary>
        /// Gets or sets the list of custom rewarded ad identifiers.
        /// Each identifier is associated with an ad placement.
        /// </summary>
        public Dictionary<AdPlacement, AdId> CustomRewardedAdIds
        {
            get { return this.mCustomRewardedAdIds; }
            set { this.mCustomRewardedAdIds = value as Dictionary_AdPlacement_AdId; }
        }

        [SerializeField]
        private bool mAgeRestrictMode;

        [SerializeField]
        private string mSDKKey;
        [SerializeField]
        private AdId mDefaultBannerAdId;
        [SerializeField]
        private AdId mDefaultInterstitialAdId;
        [SerializeField]
        private AdId mDefaultRewardedAdId;
        [SerializeField]
        private Dictionary_AdPlacement_AdId mCustomBannerAdIds;
        [SerializeField]
        private Dictionary_AdPlacement_AdId mCustomInterstitialAdIds;
        [SerializeField]
        private Dictionary_AdPlacement_AdId mCustomRewardedAdIds;


    }
}