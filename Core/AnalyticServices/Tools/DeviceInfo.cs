namespace Core.AnalyticServices.Tools
{
    using System;
    using System.Globalization;
    using UnityEngine;

    public class DeviceInfo
    {
        public string BundleId { get; private set; }

        public string GameVersion { get; private set; }

        public string GameBuildNumber { get; private set; }

        public string Language { get; private set; }

        public string Country { get; private set; }

        public string OSVersion { get; private set; }

        public string Family { get; private set; }

        public string Make { get; private set; }

        public string Platform { get; private set; }

#if UNITY_IOS
		public const string Store = "appstore";
#elif STORE_GOOGLEPLAY
		public const string Store = "googleplay";
#elif STORE_AMAZON
		public const string Store = "amazon";
#elif STORE_SAMSUNG
		public const string Store = "samsung";
#elif UNITY_WSA_10_0
		public const string Store = "microsoft";
#else
        public const string Store = "unknown";
#endif

        /*
         * Android
         */

        public string Gaid { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string AndroidId { get; private set; }

        /*
         * iOS
         */

        public string Idfa { get; private set; }

        public string Idfv { get; private set; }

        public bool IsTestflightBuild { get; private set; }

        /*
         * UWP
         */

        public string ASHWID { get; private set; }

        public string UWPAdvertisingId { get; private set; }

        public string UWPDeviceManufacturer { get; private set; }

        public string UWPLanguage { get; private set; }

        public string UWPCountry { get; private set; }

        public string UWPOSVersion { get; private set; }

        public string UWPFamily { get; private set; }

        public string InstallId
        {
            get
            {
                if (PlayerPrefs.HasKey(InstallIdKey))
                    return PlayerPrefs.GetString(InstallIdKey);

                PlayerPrefs.SetString(InstallIdKey, Guid.NewGuid().ToString("D"));
                PlayerPrefs.SetString(InstallDateKey, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
                return PlayerPrefs.GetString(InstallIdKey);
            }
        }

        public bool IsFirstLaunch
        {
            get
            {
                if (PlayerPrefs.HasKey(FirstLaunchKey))
                    return false;

                PlayerPrefs.SetInt(FirstLaunchKey, 1);
                return true;
            }
        }

        private const string FirstLaunchKey = "sdk.game_previously_lanched";
        private const string InstallIdKey   = "sdk.install_id";
        public const  string InstallDateKey = "sdk.install_date";

        // todo - need to bind data here by implementing native tool to get device info
        internal void ScrapeDeviceData()
        {
#if UNITY_IOS && !UNITY_EDITOR
			Make = "apple";
			Platform = "iOS";
#elif UNITY_ANDROID && !UNITY_EDITOR
			Platform = "Android";
#elif UNITY_WEBGL && !UNITY_EDITOR
			Platform = "Web";
#elif UNITY_WSA_10_0 && !UNITY_EDITOR
			Platform = "Windows";
#else
            this.Platform = "Editor";
#endif
        }
    }
}