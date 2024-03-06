namespace ServiceImplementation.Configs.Ads
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using ServiceImplementation.Configs.Common;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.Events;
    using Object = UnityEngine.Object;

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
        public List<AdId> NativeAdIds { get => this.mNativeAdIds; set => this.mNativeAdIds = value; }

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

        [OnInspectorInit]
        private void LoadAdmobSetting()
        {
            var googleMobileAdsSettings = Resources.Load<Object>("GoogleMobileAdsSettings");

            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
            var settingType  = googleMobileAdsSettings.GetType();

            this.mIOSAppId     = settingType.GetField("adMobIOSAppId", bindingFlags).GetValue(googleMobileAdsSettings) as string;
            this.mAndroidAppId = settingType.GetField("adMobAndroidAppId", bindingFlags).GetValue(googleMobileAdsSettings) as string;

            this.mOptimizeInitialization = (bool)settingType.GetField("optimizeInitialization", bindingFlags).GetValue(googleMobileAdsSettings);

            this.mOptimizeAdLoading = (bool)settingType.GetField("optimizeAdLoading", bindingFlags).GetValue(googleMobileAdsSettings);

            this.mDelayAppMeasurementInit = (bool)settingType.GetField("delayAppMeasurementInit", bindingFlags).GetValue(googleMobileAdsSettings);

            this.mUserTrackingUsageDescription = settingType.GetField("userTrackingUsageDescription", bindingFlags).GetValue(googleMobileAdsSettings) as string;
        }

        public UnityAction<ScriptableObject> OnDataChange;

        private void SaveAdmobSetting()
        {
            var googleMobileAdsSettings = Resources.Load<ScriptableObject>("GoogleMobileAdsSettings");

            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
            var settingType  = googleMobileAdsSettings.GetType();

            settingType.GetField("adMobIOSAppId", bindingFlags).SetValue(googleMobileAdsSettings, this.mIOSAppId);
            settingType.GetField("adMobAndroidAppId", bindingFlags).SetValue(googleMobileAdsSettings, this.mAndroidAppId);
            settingType.GetField("optimizeInitialization", bindingFlags).SetValue(googleMobileAdsSettings, this.mOptimizeInitialization);
            settingType.GetField("optimizeAdLoading", bindingFlags).SetValue(googleMobileAdsSettings, this.mOptimizeAdLoading);
            settingType.GetField("delayAppMeasurementInit", bindingFlags).SetValue(googleMobileAdsSettings, this.mDelayAppMeasurementInit);
            settingType.GetField("userTrackingUsageDescription", bindingFlags).SetValue(googleMobileAdsSettings, this.mUserTrackingUsageDescription);
            
            this.OnDataChange?.Invoke(googleMobileAdsSettings);
        }

        private void AppIdChanged() { Debug.Log("Admob app id changed"); }

        [OnValueChanged("SaveAdmobSetting")] [Header("Google Mobile Ads App ID")] [BoxGroup("Admob Settings")] [SerializeField] [LabelText("Android App Id")]
        private string mAndroidAppId;

        [OnValueChanged("SaveAdmobSetting")] [BoxGroup("Admob Settings")] [SerializeField] [LabelText("IOS App Id")]
        private string mIOSAppId;


        [OnValueChanged("SaveAdmobSetting")] [Header("Android optimization settings")] [BoxGroup("Admob Settings")] [SerializeField] [LabelText("Optimize initialization")]
        private bool mOptimizeInitialization;

        [OnValueChanged("SaveAdmobSetting")] [BoxGroup("Admob Settings")] [SerializeField] [LabelText("Optimize ad loading")]
        private bool mOptimizeAdLoading;

        [OnValueChanged("SaveAdmobSetting")] [Header("Admob-specific settings")] [SerializeField] [BoxGroup("Admob Settings")]
        private bool mDelayAppMeasurementInit;

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

        [SerializeField] [LabelText("Rewarded")] [BoxGroup("Custom Placement Id")]
        private Dictionary_AdPlacement_AdId mCustomRewardedAdIds;

        [SerializeField] [LabelText("Rewarded Interstitial")] [BoxGroup("Custom Placement Id")]
        private Dictionary_AdPlacement_AdId mCustomRewardedInterstitialAdIds;

        [SerializeField] [LabelText("Is Adaptive Banner")] [BoxGroup("Admob Settings")]
        private bool mIsAdaptiveBannerEnabled = true;
    }
}