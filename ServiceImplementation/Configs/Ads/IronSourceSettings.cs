namespace ServiceImplementation.Configs.Ads
{
    using System;
    using ServiceImplementation.Configs.Common;
    using UnityEngine;

    [Serializable]
    public class IronSourceSettings : AdNetworkSettings
    {
        /// <summary>
        /// Gets or sets the IronSource app identifier.
        /// </summary>
        public AdId AppId
        {
            get { return this.mAppId; }
            set { this.mAppId = value; }
        }

        [SerializeField]
        private AdId mAppId;

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
    }
}
