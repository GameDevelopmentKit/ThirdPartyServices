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
        public AdId DefaultBannerAdId { get => this.mDefaultBannerAdId; set => this.mDefaultBannerAdId = value; }

        public AdId CollapsibleBannerAdId 
        {
#if THEONE_ADS_DEBUG || ADMOB_ADS_DEBUG
            get => !string.IsNullOrEmpty(this.mCollapsibleBannerAdId.Id) ? new AdId("ca-app-pub-3940256099942544/8388050270","ca-app-pub-3940256099942544/2014213617") : this.mCollapsibleBannerAdId; 
#else
            get => this.mCollapsibleBannerAdId; 
#endif
            set => this.mCollapsibleBannerAdId = value; 
        }

        /// <summary>
        /// Gets or sets the default interstitial ad identifier.
        /// </summary>
        public AdId DefaultInterstitialAdId
        {
#if THEONE_ADS_DEBUG || ADMOB_ADS_DEBUG
            get => !string.IsNullOrEmpty(this.mDefaultInterstitialAdId.Id) ? new AdId("ca-app-pub-3940256099942544/4411468910","ca-app-pub-3940256099942544/1033173712") : this.mDefaultInterstitialAdId; 
#else
            get => this.mDefaultInterstitialAdId; 
#endif
            set => this.mDefaultInterstitialAdId = value;
        }

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
        public AdId AOAAdId
        {
#if THEONE_ADS_DEBUG || ADMOB_ADS_DEBUG
            get => !string.IsNullOrEmpty(this.mAoaAdId.Id) ? new AdId("ca-app-pub-3940256099942544/5575463023","ca-app-pub-3940256099942544/9257395921") : this.mAoaAdId; 
#else
            get => this.mAoaAdId; 
#endif
            set => this.mAoaAdId = value;
        }

        /// <summary>
        /// Gets or sets the default native ad identifier.
        /// </summary>
        public List<AdId> NativeAdIds
        {
#if THEONE_ADS_DEBUG || ADMOB_ADS_DEBUG
            get => this.mNativeAdIds.Select(x => new AdId("ca-app-pub-3940256099942544/3986624511", "ca-app-pub-3940256099942544/2247696110")).ToList();
#else
            get => this.mNativeAdIds; 
#endif
            set => this.mNativeAdIds = value;
        }

        /// <summary>
        /// Gets or sets the default MREC ad identifier.
        /// </summary>
        public Dictionary_AdViewPosition_AdId MRECAdIds { get => this.mMRECAdIds; set => this.mMRECAdIds = value; }

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
        
        public string AdmobAndroidAppId { get => this.mAdmobAndroidAppId; set => this.mAdmobAndroidAppId = value; } // Admob App Id

        [OnInspectorInit]
        private void LoadAdmobSetting()
        {
            var googleMobileAdsSettings = Resources.Load<ScriptableObject>("GoogleMobileAdsSettings");

            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
            var settingType  = googleMobileAdsSettings.GetType();

            this.mIOSAppId     = settingType.GetField("adMobIOSAppId", bindingFlags).GetValue(googleMobileAdsSettings) as string;
            this.mAdmobAndroidAppId = settingType.GetField("adMobAndroidAppId", bindingFlags).GetValue(googleMobileAdsSettings) as string;
            
            #if APPLOVIN && UNITY_EDITOR
            AppLovinSettings.UpdateGoogleAdsId(this.mAdmobAndroidAppId, this.mIOSAppId);
            #endif
            
            this.mOptimizeInitialization = (bool)settingType.GetField("optimizeInitialization", bindingFlags).GetValue(googleMobileAdsSettings);

            this.mOptimizeAdLoading = (bool)settingType.GetField("optimizeAdLoading", bindingFlags).GetValue(googleMobileAdsSettings);

#if ADMOB_BELLOW_9_0_0
            this.mDelayAppMeasurementInit = (bool)settingType.GetField("delayAppMeasurementInit", bindingFlags).GetValue(googleMobileAdsSettings);
#endif 
            this.enableKotlinXCoroutinesPackagingOption = (bool)settingType.GetField("enableKotlinXCoroutinesPackagingOption", bindingFlags).GetValue(googleMobileAdsSettings);
            this.mValidateGradleDependencies = (bool)settingType.GetField("validateGradleDependencies", bindingFlags).GetValue(googleMobileAdsSettings);
            this.mUserTrackingUsageDescription = settingType.GetField("userTrackingUsageDescription", bindingFlags).GetValue(googleMobileAdsSettings) as string;
        }

        public UnityAction<ScriptableObject> OnDataChange;

        private void SaveAdmobSetting()
        {
            var googleMobileAdsSettings = Resources.Load<ScriptableObject>("GoogleMobileAdsSettings");

            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
            var settingType  = googleMobileAdsSettings.GetType();

            settingType.GetField("adMobIOSAppId", bindingFlags).SetValue(googleMobileAdsSettings, this.mIOSAppId);
            settingType.GetField("adMobAndroidAppId", bindingFlags).SetValue(googleMobileAdsSettings, this.mAdmobAndroidAppId);
#if APPLOVIN && UNITY_EDITOR
            AppLovinSettings.UpdateGoogleAdsId(this.mAdmobAndroidAppId, this.mIOSAppId);
#endif
            settingType.GetField("optimizeInitialization", bindingFlags).SetValue(googleMobileAdsSettings, this.mOptimizeInitialization);
            settingType.GetField("optimizeAdLoading", bindingFlags).SetValue(googleMobileAdsSettings, this.mOptimizeAdLoading);
#if ADMOB_BELLOW_9_0_0
            settingType.GetField("delayAppMeasurementInit", bindingFlags).SetValue(googleMobileAdsSettings, this.mDelayAppMeasurementInit);
#endif
            settingType.GetField("validateGradleDependencies", bindingFlags).SetValue(googleMobileAdsSettings, this.mValidateGradleDependencies);
            settingType.GetField("enableKotlinXCoroutinesPackagingOption", bindingFlags).SetValue(googleMobileAdsSettings, this.enableKotlinXCoroutinesPackagingOption);
            settingType.GetField("userTrackingUsageDescription", bindingFlags).SetValue(googleMobileAdsSettings, this.mUserTrackingUsageDescription);
            
            this.OnDataChange?.Invoke(googleMobileAdsSettings);
#if UNITY_EDITOR
            EditorUtility.SetDirty(googleMobileAdsSettings);
            AssetDatabase.SaveAssets();
#endif
        }

        private void AppIdChanged() { Debug.Log("Admob app id changed"); }

        [OnValueChanged("SaveAdmobSetting")] [Header("Google Mobile Ads App ID")] [BoxGroup("Admob Settings")] [SerializeField] [LabelText("Android App Id")]
        private string mAdmobAndroidAppId;

        [OnValueChanged("SaveAdmobSetting")] [BoxGroup("Admob Settings")] [SerializeField] [LabelText("IOS App Id")]
        private string mIOSAppId;


        [OnValueChanged("SaveAdmobSetting")] [Header("Android optimization settings")] [BoxGroup("Admob Settings")] [SerializeField] [LabelText("Optimize initialization")]
        private bool mOptimizeInitialization;

        [OnValueChanged("SaveAdmobSetting")] [BoxGroup("Admob Settings")] [SerializeField] [LabelText("Optimize ad loading")]
        private bool mOptimizeAdLoading;

#if ADMOB_BELLOW_9_0_0
        [OnValueChanged("SaveAdmobSetting")] [Header("Admob-specific settings")] [SerializeField] [BoxGroup("Admob Settings")]
        private bool mDelayAppMeasurementInit;
#endif
        [FormerlySerializedAs("mEnableKotlinXCoroutinesPackagingOption"),OnValueChanged("SaveAdmobSetting")] [Header("Admob-specific settings")] [SerializeField] [BoxGroup("Admob Settings")]
        private bool enableKotlinXCoroutinesPackagingOption;

        [OnValueChanged("SaveAdmobSetting")] [SerializeField] [BoxGroup("Admob Settings")] [LabelText("Remove property tag from GMA Android SDK")]
        private bool mValidateGradleDependencies;

        [OnValueChanged("SaveAdmobSetting")] [Header("UMP-specific settings")] [SerializeField] [BoxGroup("Admob Settings")]
        private string mUserTrackingUsageDescription;

        [SerializeField] [LabelText("Enable Test Mode")]
        private bool mEnableTestMode;

        [SerializeField] [LabelText("Banner")] [BoxGroup("Default Id")]
        private AdId mDefaultBannerAdId;

        [SerializeField] [LabelText("Collapsible Banner")] [BoxGroup("Default Id")]
        private AdId mCollapsibleBannerAdId;

        [SerializeField] [LabelText("Interstitial")] [BoxGroup("Default Id")]
        private AdId mDefaultInterstitialAdId;

        [SerializeField] [LabelText("Rewarded")] [BoxGroup("Default Id")]
        private AdId mDefaultRewardedAdId;

        [SerializeField] [LabelText("Rewarded Interstitial")] [BoxGroup("Default Id")]
        private AdId mDefaultRewardedInterstitialAdId;

        [SerializeField] [LabelText("AOA")] [BoxGroup("Default Id")]
        private AdId mAoaAdId;

        [SerializeField] [LabelText("Native")] [BoxGroup("Default Id")]
        private List<AdId> mNativeAdIds;

        [SerializeField] [LabelText("MREC")] [BoxGroup("Default Id")]
        private Dictionary_AdViewPosition_AdId mMRECAdIds;


        [SerializeField] [LabelText("Banner")] [BoxGroup("Custom Placement Id")]
        private Dictionary_AdPlacement_AdId mCustomBannerAdIds;

        [SerializeField] [LabelText("Interstitial")] [BoxGroup("Custom Placement Id")]
        private Dictionary_AdPlacement_AdId mCustomInterstitialAdIds;
        
        [SerializeField] [LabelText("Interstitial")] [BoxGroup("Custom Placement Id")]
        private Dictionary<AdPlacement, CustomCappingTime> mCustomInterstitialCappingTime;

        [SerializeField] [LabelText("Rewarded")] [BoxGroup("Custom Placement Id")]
        private Dictionary_AdPlacement_AdId mCustomRewardedAdIds;

        [SerializeField] [LabelText("Rewarded Interstitial")] [BoxGroup("Custom Placement Id")]
        private Dictionary_AdPlacement_AdId mCustomRewardedInterstitialAdIds;

        [SerializeField] [LabelText("Is Adaptive Banner")] [BoxGroup("Admob Settings")]
        private bool mIsAdaptiveBannerEnabled = true;
    }
}