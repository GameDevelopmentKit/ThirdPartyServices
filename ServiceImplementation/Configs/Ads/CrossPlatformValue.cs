namespace ServiceImplementation.Configs.Ads
{
    using System;
    using ServiceImplementation.Configs.Common;

    /// <summary>
    /// Generic cross-platform identifier for ad resources.
    /// </summary>
    [Serializable]
    public class CrossPlatformValue : Common.CrossPlatformValue
    {
        /// <summary>
        /// Gets the ad ID for iOS platform.
        /// </summary>
        /// <value>The ios identifier.</value>
        public override string IosValue => Util.AutoTrimId(this.mIosId);

        /// <summary>
        /// Gets the ad ID for Android platform.
        /// </summary>
        /// <value>The android identifier.</value>
        public override string AndroidValue => Util.AutoTrimId(this.mAndroidId);

        public CrossPlatformValue(string iOSId, string androidId)
            : base(iOSId, androidId)
        {
        }
    }
}