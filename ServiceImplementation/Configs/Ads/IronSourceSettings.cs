namespace ServiceImplementation.Configs.Ads
{
    using System;
    using System.Collections.Generic;
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

        public bool IsAdaptiveBanner => this.isAdaptiveBanner;

        [SerializeField] [LabelText("App Id")] private AdId mAppId;

        [SerializeField] private bool isAdaptiveBanner = true;

        [SerializeField] [OnValueChanged("OnEnableAdQuality")] private bool enableAdQuality = true;

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

        public override Dictionary<AdPlacement, AdId> CustomBannerAdIds       { get; set; }
        public override Dictionary<AdPlacement, AdId> CustomInterstitialAdIds { get; set; }
        public override Dictionary<AdPlacement, AdId> CustomRewardedAdIds     { get; set; }
    }
}