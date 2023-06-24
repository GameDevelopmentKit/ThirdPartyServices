namespace ServiceImplementation.Configs
{
    using ServiceImplementation.Configs.Ads;
    using ServiceImplementation.Configs.Common;
    using UnityEngine;

    [CreateAssetMenu(fileName = "ThirdPartiesConfig", menuName = "ScriptableObjects/SpawnThirdPartiesConfig", order = 1)]
    public class ThirdPartiesConfig : ScriptableObject
    {
        public static ThirdPartiesConfig Instance
        {
            get
            {
                if (sInstance == null)
                {
                    sInstance = LoadSettingsAsset();

                    if (sInstance == null)
                    {
#if !UNITY_EDITOR
                        Debug.LogError("Easy Mobile settings not found! " +
                            "Please go to menu Windows > Easy Mobile > Settings to setup the plugin.");
#endif
                        sInstance = CreateInstance<ThirdPartiesConfig>(); // Create a dummy scriptable object for temporary use.
                    }
                }

                return sInstance;
            }
        }

        public static ThirdPartiesConfig LoadSettingsAsset() { return Resources.Load("ThirdPartiesConfig") as ThirdPartiesConfig; }

        #region Module Settings

        public static AdSettings Advertising { get { return Instance.mAdvertisingSettings; } }


        #endregion

        #region Private members

        private static ThirdPartiesConfig sInstance;

        [SerializeField] private AdSettings mAdvertisingSettings = null;

        #endregion
    }
}