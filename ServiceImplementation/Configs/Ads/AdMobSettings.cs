namespace ServiceImplementation.Configs.Ads
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using ServiceImplementation.Configs.Common;
    using Sirenix.OdinInspector;
    #if UNITY_EDITOR
    using UnityEditor;
    #endif
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.Serialization;

    [Serializable]
    public class AdMobSettings : AdNetworkSettings
    {
        public bool IsAdaptiveBannerEnabled { get => this.mIsAdaptiveBannerEnabled; set => this.mIsAdaptiveBannerEnabled = value; }

        /// <summary>
        /// Gets or sets the default banner identifier.
        /// </summary>
        public CrossPlatformValue DefaultBannerAdId
        {
            #if THEONE_ADS_DEBUG || ADMOB_ADS_DEBUG
            get => !string.IsNullOrEmpty(this.mDefaultBannerAdId.AndroidValue) ? new ("ca-app-pub-3940256099942544/2934735716","ca-app-pub-3940256099942544/6300978111") : this.mDefaultBannerAdId;
            #else
            get => this.mDefaultBannerAdId;
            #endif
            set => this.mDefaultBannerAdId = value;
        }

        public CrossPlatformValue CollapsibleBannerAdId
        {
            #if THEONE_ADS_DEBUG || ADMOB_ADS_DEBUG
            get => !string.IsNullOrEmpty(this.mCollapsibleBannerAdId.AndroidValue) ? new ("ca-app-pub-3940256099942544/8388050270","ca-app-pub-3940256099942544/2014213617") : this.mCollapsibleBannerAdId;
            #else
            get => this.mCollapsibleBannerAdId;
            #endif
            set => this.mCollapsibleBannerAdId = value;
        }

        /// <summary>
        /// Gets or sets the default interstitial ad identifier.
        /// </summary>
        public CrossPlatformValue DefaultInterstitialAdId
        {
            #if THEONE_ADS_DEBUG || ADMOB_ADS_DEBUG
            get => !string.IsNullOrEmpty(this.mDefaultInterstitialAdId.AndroidValue) ? new ("ca-app-pub-3940256099942544/4411468910","ca-app-pub-3940256099942544/1033173712") : this.mDefaultInterstitialAdId;
            #else
            get => this.mDefaultInterstitialAdId;
            #endif
            set => this.mDefaultInterstitialAdId = value;
        }

        /// <summary>
        /// Gets or sets the default rewarded ad identifier.
        /// </summary>
        public CrossPlatformValue DefaultRewardedAdId
        {
            #if THEONE_ADS_DEBUG || ADMOB_ADS_DEBUG
            get => !string.IsNullOrEmpty(this.mDefaultRewardedAdId.AndroidValue) ? new ("ca-app-pub-3940256099942544/1712485313","ca-app-pub-3940256099942544/5224354917") : this.mDefaultRewardedAdId;
            #else
            get => this.mDefaultRewardedAdId;
            #endif
            set => this.mDefaultRewardedAdId = value;
        }

        /// <summary>
        /// Gets or sets the default rewarded interstitial ad identifier.
        /// </summary>
        public CrossPlatformValue DefaultRewardedInterstitialAdId { get => this.mDefaultRewardedInterstitialAdId; set => this.mDefaultRewardedInterstitialAdId = value; }

        /// <summary>
        /// Gets or sets the default AOA ad identifier.
        /// </summary>
        public CrossPlatformValue AOAAdId
        {
            #if THEONE_ADS_DEBUG || ADMOB_ADS_DEBUG
            get => !string.IsNullOrEmpty(this.mAoaAdId.AndroidValue) ? new ("ca-app-pub-3940256099942544/5575463023","ca-app-pub-3940256099942544/9257395921") : this.mAoaAdId;
            #else
            get => this.mAoaAdId;
            #endif
            set => this.mAoaAdId = value;
        }

        /// <summary>
        /// Gets or sets the default native ad identifier.
        /// </summary>
        public List<CrossPlatformValue> NativeAdIds
        {
            #if THEONE_ADS_DEBUG || ADMOB_ADS_DEBUG
            get => this.mNativeAdIds.Select(x => !string.IsNullOrEmpty(x.AndroidValue) ? new ("ca-app-pub-3940256099942544/3986624511", "ca-app-pub-3940256099942544/2247696110") : x).ToList();
            #else
            get => this.mNativeAdIds;
            #endif
            set => this.mNativeAdIds = value;
        }

        /// <summary>
        /// Gets or sets the default MREC ad identifier.
        /// </summary>
        public Dictionary<AdPlacement, CrossPlatformValue> MRECAdIds
        {
            #if THEONE_ADS_DEBUG || ADMOB_ADS_DEBUG
            get => this.ConvertIdsToTestId(this.mRECAdIds, new("ca-app-pub-3940256099942544/2934735716", "ca-app-pub-3940256099942544/6300978111"));
            #else
            get => this.mRECAdIds;
            #endif
            set => this.mRECAdIds = value as Dictionary_AdPlacement_AdId;
        }

        private Dictionary<AdPlacement, CrossPlatformValue> ConvertIdsToTestId(Dictionary<AdPlacement, CrossPlatformValue> ids, CrossPlatformValue testId)
            => ids.Select(x => new KeyValuePair<AdPlacement, CrossPlatformValue>(x.Key, !string.IsNullOrEmpty(x.Value.DefaultValue) ? testId : x.Value))
                .ToDictionary(x => x.Key, x => x.Value);

        /// <summary>
        /// Gets or sets the default Native Overlay ad identifier.
        /// </summary>
        public Dictionary<AdPlacement, CrossPlatformValue> NativeOverlayAdIds
        {
            #if THEONE_ADS_DEBUG || ADMOB_ADS_DEBUG
            get => this.ConvertIdsToTestId(this.nativeOverlayAdIds, new ("ca-app-pub-3940256099942544/3986624511","ca-app-pub-3940256099942544/2247696110"));
            #else
            get => this.nativeOverlayAdIds;
            #endif
            set => this.nativeOverlayAdIds = value as Dictionary_AdPlacement_AdId;
        }

        public NativeOverlayStyleConfig NativeOverlayStyleConfig => this.nativeOverlayStyleConfig;

        /// <summary>
        /// Enables or disables test mode.
        /// </summary>
        public bool EnableTestMode { get => this.mEnableTestMode; set => this.mEnableTestMode = value; }

        /// <summary>
        /// Gets or sets the list of custom banner identifiers.
        /// Each identifier is associated with an ad placement.
        /// </summary>
        public override Dictionary<AdPlacement, CrossPlatformValue> CustomBannerAdIds
        {
            #if THEONE_ADS_DEBUG || ADMOB_ADS_DEBUG
            get => this.ConvertIdsToTestId(this.mCustomBannerAdIds, new("ca-app-pub-3940256099942544/2934735716", "ca-app-pub-3940256099942544/6300978111"));
            #else
            get => this.mCustomBannerAdIds;
            #endif
            set => this.mCustomBannerAdIds = value as Dictionary_AdPlacement_AdId;
        }

        /// <summary>
        /// Gets or sets the list of custom interstitial ad identifiers.
        /// Each identifier is associated with an ad placement.
        /// </summary>
        public override Dictionary<AdPlacement, CrossPlatformValue> CustomInterstitialAdIds
        {
            #if THEONE_ADS_DEBUG || ADMOB_ADS_DEBUG
            get => this.ConvertIdsToTestId(this.mCustomInterstitialAdIds, new("ca-app-pub-3940256099942544/4411468910","ca-app-pub-3940256099942544/1033173712"));
            #else
            get => this.mCustomInterstitialAdIds;
            #endif
            set => this.mCustomInterstitialAdIds = value as Dictionary_AdPlacement_AdId;
        }

        /// <summary>
        /// Gets or sets the list of custom rewarded ad identifiers.
        /// Each identifier is associated with an ad placement.
        /// </summary>
        public override Dictionary<AdPlacement, CrossPlatformValue> CustomRewardedAdIds
        {
            #if THEONE_ADS_DEBUG || ADMOB_ADS_DEBUG
            get => this.ConvertIdsToTestId(this.mCustomRewardedAdIds, new("ca-app-pub-3940256099942544/1712485313","ca-app-pub-3940256099942544/5224354917"));
            #else
            get => this.mCustomRewardedAdIds;
            #endif
            set => this.mCustomRewardedAdIds = value as Dictionary_AdPlacement_AdId;
        }

        /// <summary>
        /// Gets or sets the list of custom rewarded interstitial ad identifiers.
        /// Each identifier is associated with an ad placement.
        /// </summary>
        public Dictionary<AdPlacement, CrossPlatformValue> CustomRewardedInterstitialAdIds { get => this.mCustomRewardedInterstitialAdIds; set => this.mCustomRewardedInterstitialAdIds = value as Dictionary_AdPlacement_AdId; }

        [OnInspectorInit]
        private void LoadAdmobSetting()
        {
            var googleMobileAdsSettings = Resources.Load<ScriptableObject>("GoogleMobileAdsSettings");

            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
            var settingType  = googleMobileAdsSettings.GetType();

            this.mIOSAppId     = settingType.GetField("adMobIOSAppId", bindingFlags).GetValue(googleMobileAdsSettings) as string;
            this.mAndroidAppId = settingType.GetField("adMobAndroidAppId", bindingFlags).GetValue(googleMobileAdsSettings) as string;

            #if APPLOVIN && UNITY_EDITOR
            AppLovinSettings.UpdateGoogleAdsId(this.mAndroidAppId, this.mIOSAppId);
            #endif

            this.disableOptimizeInitialization = (bool)settingType.GetField("disableOptimizeInitialization", bindingFlags).GetValue(googleMobileAdsSettings);

            this.disableOptimizeAdLoading = (bool)settingType.GetField("disableOptimizeAdLoading", bindingFlags).GetValue(googleMobileAdsSettings);

            #if ADMOB_BELLOW_9_0_0
            this.mDelayAppMeasurementInit = (bool)settingType.GetField("delayAppMeasurementInit", bindingFlags).GetValue(googleMobileAdsSettings);
            #endif
            this.enableKotlinXCoroutinesPackagingOption = (bool)settingType.GetField("enableKotlinXCoroutinesPackagingOption", bindingFlags).GetValue(googleMobileAdsSettings);
            this.mUserTrackingUsageDescription          = settingType.GetField("userTrackingUsageDescription", bindingFlags).GetValue(googleMobileAdsSettings) as string;
        }

        public UnityAction<ScriptableObject> OnDataChange;

        private void SaveAdmobSetting()
        {
            var googleMobileAdsSettings = Resources.Load<ScriptableObject>("GoogleMobileAdsSettings");

            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
            var settingType  = googleMobileAdsSettings.GetType();

            settingType.GetField("adMobIOSAppId", bindingFlags).SetValue(googleMobileAdsSettings, this.mIOSAppId);
            settingType.GetField("adMobAndroidAppId", bindingFlags).SetValue(googleMobileAdsSettings, this.mAndroidAppId);
            #if APPLOVIN && UNITY_EDITOR
            AppLovinSettings.UpdateGoogleAdsId(this.mAndroidAppId, this.mIOSAppId);
            #endif
            settingType.GetField("disableOptimizeInitialization", bindingFlags).SetValue(googleMobileAdsSettings, this.disableOptimizeInitialization);
            settingType.GetField("disableOptimizeAdLoading", bindingFlags).SetValue(googleMobileAdsSettings, this.disableOptimizeAdLoading);
            #if ADMOB_BELLOW_9_0_0
            settingType.GetField("delayAppMeasurementInit", bindingFlags).SetValue(googleMobileAdsSettings, this.mDelayAppMeasurementInit);
            #endif
            settingType.GetField("enableKotlinXCoroutinesPackagingOption", bindingFlags).SetValue(googleMobileAdsSettings, this.enableKotlinXCoroutinesPackagingOption);
            settingType.GetField("userTrackingUsageDescription", bindingFlags).SetValue(googleMobileAdsSettings, this.mUserTrackingUsageDescription);

            this.OnDataChange?.Invoke(googleMobileAdsSettings);
            #if UNITY_EDITOR
            EditorUtility.SetDirty(googleMobileAdsSettings);
            AssetDatabase.SaveAssets();
            #endif
        }

        private void AppIdChanged()
        {
            Debug.Log("Admob app id changed");
        }

        [OnValueChanged("SaveAdmobSetting")] [Header("Google Mobile Ads App ID")] [BoxGroup("Admob Settings")] [SerializeField] [LabelText("Android App Id")] private string mAndroidAppId;

        [OnValueChanged("SaveAdmobSetting")] [BoxGroup("Admob Settings")] [SerializeField] [LabelText("IOS App Id")] private string mIOSAppId;

        [OnValueChanged("SaveAdmobSetting")]
        [Header("Android optimization settings")]
        [BoxGroup("Admob Settings")]
        [SerializeField]
        [LabelText("Disable optimize initialization")]
        private bool disableOptimizeInitialization;

        [OnValueChanged("SaveAdmobSetting")] [BoxGroup("Admob Settings")] [SerializeField] [LabelText("Disable optimize ad loading")] private bool disableOptimizeAdLoading;

        #if ADMOB_BELLOW_9_0_0
        [OnValueChanged("SaveAdmobSetting")] [Header("Admob-specific settings")] [SerializeField] [BoxGroup("Admob Settings")]
        private bool mDelayAppMeasurementInit;
        #endif
        [FormerlySerializedAs("mEnableKotlinXCoroutinesPackagingOption")]
        [OnValueChanged("SaveAdmobSetting")]
        [Header("Admob-specific settings")]
        [SerializeField]
        [BoxGroup("Admob Settings")]
        private bool enableKotlinXCoroutinesPackagingOption;

        [OnValueChanged("SaveAdmobSetting")] [SerializeField] [BoxGroup("Admob Settings")] [LabelText("Remove property tag from GMA Android SDK")] private bool mValidateGradleDependencies;

        [OnValueChanged("SaveAdmobSetting")] [Header("UMP-specific settings")] [SerializeField] [BoxGroup("Admob Settings")] private string mUserTrackingUsageDescription;

        [SerializeField] [FoldoutGroup("Native Overlay")] private Dictionary_AdPlacement_AdId nativeOverlayAdIds;

        [SerializeField] [FoldoutGroup("Native Overlay")] private NativeOverlayStyleConfig nativeOverlayStyleConfig;

        [SerializeField] [LabelText("Enable Test Mode")] private bool mEnableTestMode;

        [SerializeField] [LabelText("Banner")] [BoxGroup("Default Id")] private CrossPlatformValue mDefaultBannerAdId;

        [SerializeField] [LabelText("Collapsible Banner")] [BoxGroup("Default Id")] private CrossPlatformValue mCollapsibleBannerAdId;

        [SerializeField] [LabelText("Interstitial")] [BoxGroup("Default Id")] private CrossPlatformValue mDefaultInterstitialAdId;

        [SerializeField] [LabelText("Rewarded")] [BoxGroup("Default Id")] private CrossPlatformValue mDefaultRewardedAdId;

        [SerializeField] [LabelText("Rewarded Interstitial")] [BoxGroup("Default Id")] private CrossPlatformValue mDefaultRewardedInterstitialAdId;

        [SerializeField] [LabelText("AOA")] [BoxGroup("Default Id")] private CrossPlatformValue mAoaAdId;

        [SerializeField] [LabelText("Native")] [BoxGroup("Default Id")] private List<CrossPlatformValue> mNativeAdIds;

        [SerializeField] [LabelText("Banner")] [BoxGroup("Custom Placement Id")] private Dictionary_AdPlacement_AdId mCustomBannerAdIds;

        [SerializeField] [LabelText("Interstitial")] [BoxGroup("Custom Placement Id")] private Dictionary_AdPlacement_AdId mCustomInterstitialAdIds;

        [SerializeField] [LabelText("Interstitial")] [BoxGroup("Custom Placement Id")] private Dictionary<AdPlacement, CustomCappingTime> mCustomInterstitialCappingTime;

        [SerializeField] [LabelText("Rewarded")] [BoxGroup("Custom Placement Id")] private Dictionary_AdPlacement_AdId mCustomRewardedAdIds;

        [SerializeField] [LabelText("Rewarded Interstitial")] [BoxGroup("Custom Placement Id")] private Dictionary_AdPlacement_AdId mCustomRewardedInterstitialAdIds;

        [SerializeField] [LabelText("MREC")] [BoxGroup("Custom Placement Id")] private Dictionary_AdPlacement_AdId mRECAdIds;

        [SerializeField] [LabelText("Is Adaptive Banner")] [BoxGroup("Admob Settings")] private bool mIsAdaptiveBannerEnabled = true;
    }
}