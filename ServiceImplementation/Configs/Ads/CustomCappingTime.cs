namespace ServiceImplementation.Configs.Ads
{
    using System;

    [Serializable]
    public class CustomCappingTime
    {
        /// <summary>
        /// Gets the ad ID for iOS platform.
        /// </summary>
        /// <value>The ios identifier.</value>
        public int IosCapping;

        /// <summary>
        /// Gets the ad ID for Android platform.
        /// </summary>
        /// <value>The android identifier.</value>
        public int AndroidCapping;

        public int GetCappingTime
        {
            get
            {
                #if UNITY_ANDROID
                return this.AndroidCapping;
                #elif UNITY_IOS
                return IosCapping;
                #else
                return -1;
                #endif
            }
        }

        public CustomCappingTime(int iosCapping, int androidCapping)
        {
            this.IosCapping     = iosCapping;
            this.AndroidCapping = androidCapping;
        }
    }
}