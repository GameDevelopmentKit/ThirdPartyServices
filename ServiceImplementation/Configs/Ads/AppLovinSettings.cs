namespace ServiceImplementation.Configs.Ads
{
    using System;
    using System.Collections.Generic;
    using AmazonAds;
    using ServiceImplementation.Configs.Common;
    using Sirenix.OdinInspector;
    using UnityEngine;
#if UNITY_EDITOR
    using ServiceImplementation.Configs.Editor;
#endif

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
        /// Gets or sets the default MREC ad identifier.
        /// </summary>
        public Dictionary_AdViewPosition_AdId MRECAdIds { get => this.mMRECAdIds; set => this.mMRECAdIds = value; }

        /// <summary>
        /// age-restricted category.
        /// </summary>
        public bool AgeRestrictMode { get { return this.mAgeRestrictMode; } set { this.mAgeRestrictMode = value; } }

        public bool CreativeDebugger => this.mCreatveiDebugger;
        
        public bool MediationDebugger => this.mMediationDebugger;

        /// <summary>
        /// Gets or sets the list of custom banner identifiers.
        /// Each identifier is associated with an ad placement.
        /// </summary>
        public override Dictionary<AdPlacement, AdId> CustomBannerAdIds { get { return this.mCustomBannerAdIds; } set { this.mCustomBannerAdIds = value as Dictionary_AdPlacement_AdId; } }

        /// <summary>
        /// Gets or sets the list of custom interstitial ad identifiers.
        /// Each identifier is associated with an ad placement.
        /// </summary>
        public override Dictionary<AdPlacement, AdId> CustomInterstitialAdIds
        {
            get { return this.mCustomInterstitialAdIds; }
            set { this.mCustomInterstitialAdIds = value as Dictionary_AdPlacement_AdId; }
        }


        /// <summary>
        /// Gets or sets the list of custom rewarded ad identifiers.
        /// Each identifier is associated with an ad placement.
        /// </summary>
        public override Dictionary<AdPlacement, AdId> CustomRewardedAdIds { get { return this.mCustomRewardedAdIds; } set { this.mCustomRewardedAdIds = value as Dictionary_AdPlacement_AdId; } }

        public AmazonApplovinSetting AmazonApplovinSetting => this.amazonApplovinSetting;

        [SerializeField, LabelText("Enable APS"), OnValueChanged("OnSetEnableAPS"), BoxGroup("Amazon")]
        private bool mEnableAPS;

        [SerializeField, BoxGroup("Amazon"), HideLabel, ShowIf("mEnableAPS")]
        private AmazonApplovinSetting amazonApplovinSetting;

        [SerializeField] [LabelText("AgeRestrictMode")]
        private bool mAgeRestrictMode;

        [SerializeField] [LabelText("Creative Debugger")]
        private bool mCreatveiDebugger;

        [SerializeField] [LabelText("Mediation Debugger")]
        private bool mMediationDebugger;

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

        [SerializeField] [LabelText("MREC")] [BoxGroup("Default Id")]
        private Dictionary_AdViewPosition_AdId mMRECAdIds;

        [SerializeField] [LabelText("Banner")] [BoxGroup("Custom Placement Id")]
        private Dictionary_AdPlacement_AdId mCustomBannerAdIds;

        [SerializeField] [LabelText("Interstitial")] [BoxGroup("Custom Placement Id")]
        private Dictionary_AdPlacement_AdId mCustomInterstitialAdIds;

        [SerializeField] [LabelText("Rewarded")] [BoxGroup("Custom Placement Id")]
        private Dictionary_AdPlacement_AdId mCustomRewardedAdIds;

#if UNITY_EDITOR
        private void OnSetEnableAPS()
        {
            const string APSSymbol = "APS_ENABLE";
            DefineSymbolEditorUtils.SetDefineSymbol(APSSymbol, this.mEnableAPS);
        }
#endif
    }

    [Serializable]
    public class AmazonApplovinSetting
    {
        #region Amazon

        public bool               EnableTesting  => this.enableTesting;
        public bool               EnableLogging  => this.enableLogging;
        public bool               UseGeoLocation => this.useGeoLocation;
        public Amazon.MRAIDPolicy MRAIDPolicy    => this.mraidPolicy;

        public string AppId                  => this.appId;
        public AdId   AmazonBannerAdId       { get => this.amazonBannerAdId;       set => this.amazonBannerAdId = value; }
        public AdId   AmazonMRecAdId         { get => this.amazonMRecAdId;         set => this.amazonMRecAdId = value; }
        public AdId   AmazonInterstitialAdId { get => this.amazonInterstitialAdId; set => this.amazonInterstitialAdId = value; }
        public AdId   AmazonRewardedAdId     { get => this.amazonRewardedAdId;     set => this.amazonRewardedAdId = value; }

        [SerializeField] private bool               enableTesting  = true;
        [SerializeField] private bool               enableLogging  = true;
        [SerializeField] private bool               useGeoLocation = true;
        [SerializeField] private Amazon.MRAIDPolicy mraidPolicy    = Amazon.MRAIDPolicy.CUSTOM;

        [SerializeField] [BoxGroup("Amazon Id")]
        private string appId;

        [SerializeField] [LabelText("Banner")] [BoxGroup("Amazon Id")]
        private AdId amazonBannerAdId;

        [SerializeField] [LabelText("MREC")] [BoxGroup("Amazon Id")]
        private AdId amazonMRecAdId;

        [SerializeField] [LabelText("Interstitial")] [BoxGroup("Amazon Id")]
        private AdId amazonInterstitialAdId;

        [SerializeField] [LabelText("Rewarded")] [BoxGroup("Amazon Id")]
        private AdId amazonRewardedAdId;

        #endregion
    }
}