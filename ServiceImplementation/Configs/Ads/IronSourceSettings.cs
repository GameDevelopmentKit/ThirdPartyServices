namespace ServiceImplementation.Configs.Ads
{
    using System;
    using System.Collections.Generic;
    using ServiceImplementation.Configs.Common;
    #if UNITY_EDITOR
    using ServiceImplementation.Configs.Editor;
    #endif
    using Sirenix.OdinInspector;
    using UnityEngine;

    [Serializable]
    public class IronSourceSettings : AdNetworkSettings
    {
        public string AppId
        {
            get
            {
                #if UNITY_ANDROID
                return this.mAppId.AndroidId;
                #else
                return this.mAppId.IosId;
                #endif
            }
        }

        /// <summary>
        /// Gets or sets the default MREC ad identifier.
        /// </summary>
        public Dictionary<AdPlacement, CrossPlatformValue> MRECAdIds { get => this.mRECAdIds; set => this.mRECAdIds = value as Dictionary_AdPlacement_AdId; }

        public CrossPlatformValue BannerId => this.bannerId;

        public bool IsAdaptiveBanner => this.isAdaptiveBanner;

        [SerializeField] [LabelText("App Id")] private CrossPlatformValue mAppId;

        [SerializeField] private bool isAdaptiveBanner = true;

        [SerializeField] [OnValueChanged("OnEnableAdQuality")] private bool enableAdQuality = true;

        [SerializeField] [LabelText("Banner")] [BoxGroup("Custom Id")] private CrossPlatformValue bannerId;

        [SerializeField] [LabelText("MREC")] [BoxGroup("Custom Id")] private Dictionary_AdPlacement_AdId mRECAdIds;

        #if UNITY_EDITOR
        private void OnEnableAdQuality()
        {
            EditorUtils.ModifyPackage(this.enableAdQuality, "com.theone.ironsource-adquality", "git@github.com:The1Studio/UnityAdQualitySDK.git");
        }
        #endif

        public enum IronSourceBannerType
        {
            /// <summary>
            /// 50 X screen width.
            /// Supports: Admob, AppLovin, Facebook, InMobi.
            /// </summary>
            Banner,

            /// <summary>
            /// 90 X screen width.
            /// Supports: Admob, Facebook.
            /// </summary>
            LargeBanner,

            /// <summary>
            /// 250 X screen width.
            /// Supports: Admob, AppLovin, Facebook, InMobi.
            /// </summary>
            RectangleBanner,

            /// <summary>
            /// 50 (screen height ≤ 720) X screen width, 90 (screen height > 720) X screen width.
            /// Supports: Admob, AppLovin, Facebook, InMobi.
            /// </summary>
            SmartBanner,
        }

        public override Dictionary<AdPlacement, CrossPlatformValue> CustomBannerAdIds       { get; set; }
        public override Dictionary<AdPlacement, CrossPlatformValue> CustomInterstitialAdIds { get; set; }
        public override Dictionary<AdPlacement, CrossPlatformValue> CustomRewardedAdIds     { get; set; }
    }
}