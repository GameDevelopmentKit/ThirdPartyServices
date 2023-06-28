﻿namespace ServiceImplementation.Configs.Ads
{
    using System;
    using System.Collections.Generic;
    using ServiceImplementation.Configs.Common;
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

        [SerializeField] [LabelText("App Id")] private AdId mAppId;

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