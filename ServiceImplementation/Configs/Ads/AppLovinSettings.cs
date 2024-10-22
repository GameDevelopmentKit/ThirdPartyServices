namespace ServiceImplementation.Configs.Ads
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Cysharp.Threading.Tasks;
    #if APS_ENABLE
    using AmazonAds;
    #endif
    using ServiceImplementation.Configs.Common;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.Networking;
    #if UNITY_EDITOR
    using UnityEditor;
    using ServiceImplementation.Configs.Editor;
    #endif

    [Serializable]
    public class AppLovinSettings : AdNetworkSettings
    {
        #if UNITY_EDITOR
        public static async void DownloadApplovin()
        {
            var downloadURL     = "https://artifacts.applovin.com/unity/com/applovin/applovin-sdk/AppLovin-MAX-Unity-Plugin-6.5.2-Android-12.5.0-iOS-12.5.0.unitypackage";
            var path            = Path.Combine(Application.temporaryCachePath, "MaxSDK.unitypackage");
            var downloadHandler = new DownloadHandlerFile(path);
            var webRequest      = new UnityWebRequest(downloadURL) { method = UnityWebRequest.kHttpVerbGET, downloadHandler = downloadHandler };

            var operation = webRequest.SendWebRequest();

            await operation;

            if (webRequest.result == UnityWebRequest.Result.Success) AssetDatabase.ImportPackage(path, false);

            webRequest.Dispose();
        }
        #endif

        /// <summary>
        /// Gets or sets the AppLovin SDKKey.
        /// </summary>
        public string SDKKey { get => this.mSDKKey; set => this.mSDKKey = value; }

        [OnValueChanged("SaveApplovinSetting")] public bool EnableMAXAdReview;

        #if UNITY_EDITOR && APPLOVIN
        [OnInspectorInit]
        private void LoadApplovinSetting()
        {
            this.SDKKey = appLovinSettings.SdkKey;
            if (string.IsNullOrEmpty(this.SDKKey))
            {
                this.EnableMAXAdReview = appLovinSettings.QualityServiceEnabled = true; //Default by true
                EditorUtility.SetDirty(appLovinSettings);
                AssetDatabase.SaveAssets();
            }
            else
            {
                this.EnableMAXAdReview = appLovinSettings.QualityServiceEnabled;
            }
        }
        
        private void SaveApplovinSetting()
        {
            appLovinSettings.SdkKey = this.SDKKey;
            appLovinSettings.QualityServiceEnabled = this.EnableMAXAdReview;
            
            EditorUtility.SetDirty(appLovinSettings);
            AssetDatabase.SaveAssets();
        }

        private static global::AppLovinSettings appLovinSettings => global::AppLovinSettings.Instance;

        public static void UpdateGoogleAdsId(string androidAppId, string iosAppId)
        {
            appLovinSettings.AdMobAndroidAppId = androidAppId;
            appLovinSettings.AdMobIosAppId = iosAppId;

            EditorUtility.SetDirty(appLovinSettings);
            AssetDatabase.SaveAssets();
        }
        #endif

        public bool IsAdaptiveBanner => this.isAdaptiveBanner;

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
        /// Gets or sets the default AOA ad identifier.
        /// </summary>
        public AdId DefaultAOAAdId { get => this.mAOAAdId; set => this.mAOAAdId = value; }

        /// <summary>
        /// Gets or sets the default MREC ad identifier.
        /// </summary>
        public Dictionary<AdPlacement, AdId> MRECAdIds { get => this.mRECAdIds; set => this.mRECAdIds = value as Dictionary_AdPlacement_AdId; }

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

        public AmazonApplovinSetting AmazonApplovinSetting => this.amazonApplovinSetting;

        [SerializeField] [LabelText("Enable APS")] [OnValueChanged("OnSetEnableAPS")] [BoxGroup("Amazon")] private bool mEnableAPS;

        [SerializeField] [BoxGroup("Amazon")] [HideLabel] [ShowIf("mEnableAPS")] private AmazonApplovinSetting amazonApplovinSetting;

        [SerializeField] private bool isAdaptiveBanner = true;

        [SerializeField] [LabelText("SDK Key")] [OnValueChanged("SaveApplovinSetting")] private string mSDKKey;

        [SerializeField] [LabelText("Banner")] [BoxGroup("Default Id")] private AdId mDefaultBannerAdId;

        [SerializeField] [LabelText("Interstitial")] [BoxGroup("Default Id")] private AdId mDefaultInterstitialAdId;

        [SerializeField] [LabelText("Rewarded")] [BoxGroup("Default Id")] private AdId mDefaultRewardedAdId;

        [SerializeField] [LabelText("AOA")] [BoxGroup("Default Id")] private AdId mAOAAdId;

        [SerializeField] [LabelText("MREC")] [BoxGroup("Default Id")] private Dictionary_AdViewPosition_AdId mMRECAdIds;

        [SerializeField] [LabelText("MREC")] [BoxGroup("Default Id")] private Dictionary_AdPlacement_AdId mRECAdIds;

        [SerializeField] [LabelText("Banner")] [BoxGroup("Custom Placement Id")] private Dictionary_AdPlacement_AdId mCustomBannerAdIds;

        [SerializeField] [LabelText("Interstitial")] [BoxGroup("Custom Placement Id")] private Dictionary_AdPlacement_AdId mCustomInterstitialAdIds;

        [SerializeField] [LabelText("Rewarded")] [BoxGroup("Custom Placement Id")] private Dictionary_AdPlacement_AdId mCustomRewardedAdIds;

        #if UNITY_EDITOR
        private void OnSetEnableAPS()
        {
            const string APSSymbol = "APS_ENABLE";
            EditorUtils.SetDefineSymbol(APSSymbol, this.mEnableAPS);
        }
        #endif
    }

    [Serializable]
    public class AmazonApplovinSetting
    {
        #region Amazon

        public bool EnableTesting  => this.enableTesting;
        public bool EnableLogging  => this.enableLogging;
        public bool UseGeoLocation => this.useGeoLocation;

        #if APS_ENABLE
        public Amazon.MRAIDPolicy MRAIDPolicy    => this.mraidPolicy;
        #endif

        public string AppId                  => this.appId;
        public AdId   AmazonBannerAdId       { get => this.amazonBannerAdId;       set => this.amazonBannerAdId = value; }
        public AdId   AmazonMRecAdId         { get => this.amazonMRecAdId;         set => this.amazonMRecAdId = value; }
        public AdId   AmazonInterstitialAdId { get => this.amazonInterstitialAdId; set => this.amazonInterstitialAdId = value; }
        public AdId   AmazonRewardedAdId     { get => this.amazonRewardedAdId;     set => this.amazonRewardedAdId = value; }

        [SerializeField] private bool enableTesting  = true;
        [SerializeField] private bool enableLogging  = true;
        [SerializeField] private bool useGeoLocation = true;
        #if APS_ENABLE
        [SerializeField] private Amazon.MRAIDPolicy mraidPolicy = Amazon.MRAIDPolicy.CUSTOM;
        #endif

        [SerializeField] [BoxGroup("Amazon Id")] private string appId;

        [SerializeField] [LabelText("Banner")] [BoxGroup("Amazon Id")] private AdId amazonBannerAdId;

        [SerializeField] [LabelText("MREC")] [BoxGroup("Amazon Id")] private AdId amazonMRecAdId;

        [SerializeField] [LabelText("Interstitial")] [BoxGroup("Amazon Id")] private AdId amazonInterstitialAdId;

        [SerializeField] [LabelText("Rewarded")] [BoxGroup("Amazon Id")] private AdId amazonRewardedAdId;

        #endregion
    }
}