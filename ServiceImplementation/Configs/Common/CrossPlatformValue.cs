namespace ServiceImplementation.Configs.Common
{
    using System;
    using Sirenix.OdinInspector;
    using UnityEngine;

    /// <summary>
    /// Generic cross-platform identifier for ad resources.
    /// </summary>
    [Serializable]
    public class CrossPlatformValue
    {
        [SerializeField] [LabelText("IOS Id", SdfIconType.Apple)] protected string mIosId;

        [SerializeField] [LabelText("Android Id", SdfIconType.Google)] protected string mAndroidId;

        /// <summary>
        /// Gets the ad ID corresponding to the current platform.
        /// Returns <c>string.Empty</c> if no ID was defined for this platform.
        /// </summary>
        /// <value>The identifier.</value>
        public virtual string DefaultValue
        {
            get
            {
                #if UNITY_ANDROID || UNITY_WEBGL
                return this.AndroidValue;
                #elif UNITY_IOS
                return IosValue;
                #else
                return string.Empty;
                #endif
            }
        }

        /// <summary>
        /// Gets the ad ID for iOS platform.
        /// </summary>
        /// <value>The ios identifier.</value>
        public virtual string IosValue => this.mIosId;

        /// <summary>
        /// Gets the ad ID for Android platform.
        /// </summary>
        /// <value>The android identifier.</value>
        public virtual string AndroidValue => this.mAndroidId;

        public CrossPlatformValue(string iOSId, string androidId)
        {
            this.mIosId     = iOSId;
            this.mAndroidId = androidId;
        }

        public override string ToString()
        {
            return this.DefaultValue;
        }

        public override bool Equals(object obj)
        {
            var item = obj as CrossPlatformValue;

            if (item == null) return false;

            return this.DefaultValue.Equals(item.DefaultValue);
        }

        public override int GetHashCode()
        {
            return this.DefaultValue.GetHashCode();
        }

        public static bool operator ==(CrossPlatformValue a, CrossPlatformValue b)
        {
            if (ReferenceEquals(a, null)) return ReferenceEquals(b, null);

            return a.Equals(b);
        }

        public static bool operator !=(CrossPlatformValue a, CrossPlatformValue b)
        {
            return !(a == b);
        }
    }
}