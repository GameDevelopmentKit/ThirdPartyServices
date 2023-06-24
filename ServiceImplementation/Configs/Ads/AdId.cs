namespace ServiceImplementation.Configs.Ads
{
    using System;
    using ServiceImplementation.Configs.Common;

    /// <summary>
    /// Generic cross-platform identifier for ad resources.
    /// </summary>
    [Serializable]
    public class AdId : CrossPlatformId
    {
        /// <summary>
        /// Gets the ad ID for iOS platform.
        /// </summary>
        /// <value>The ios identifier.</value>
        public override string IosId { get { return Util.AutoTrimId(this.mIosId); } }

        /// <summary>
        /// Gets the ad ID for Android platform.
        /// </summary>
        /// <value>The android identifier.</value>
        public override string AndroidId { get { return Util.AutoTrimId(this.mAndroidId); } }

        public AdId(string iOSId, string androidId)
            : base(iOSId, androidId)
        {
        }
    }
}