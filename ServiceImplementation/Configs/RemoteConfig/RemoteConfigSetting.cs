namespace ServiceImplementation.FireBaseRemoteConfig
{
    using ServiceImplementation.Configs.Common;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [CreateAssetMenu(fileName = nameof(RemoteConfigSetting), menuName = "ScriptableObjects/SpawnRemoteMappingConfig", order = 1)]
    public class RemoteConfigSetting : ScriptableObject
    {
        public static string ResourcePath = $"GameConfigs/{nameof(RemoteConfigSetting)}";

        public StringStringSerializableDictionary RemoteKeyMapping => this.mRemoteKeyMapping;

        [SerializeField] [LabelText("Remove Config Mapping")]
        private StringStringSerializableDictionary mRemoteKeyMapping = new()
        {
            { "enable_banner_ad", "enable_banner_ad" },
            { "enable_interstitial_ad", "enable_interstitial_ad" },
            { "enable_mrec_ad", "enable_mrec_ad" },
            { "enable_aoa_ad", "enable_aoa_ad" },
            { "enable_rewarded_ad", "enable_rewarded_ad" },
            { "enable_rewarded_interstitial_ad", "enable_rewarded_interstitial_ad" },
            { "enable_native_ad", "enable_native_ad" },
            { "interval_load_ads", "interval_load_ads" },
            { "interstitial_ad_interval", "interstitial_ad_interval" },
            { "min_pause_second_to_show_aoa_ad", "min_pause_second_to_show_aoa_ad" },
            { "enable_ump", "enable_ump" },
            { "aoa_start_session", "aoa_start_session" },
            { "interstitial_ad_start_level", "interstitial_ad_start_level" },
            { "delay_first_inters_ad_interval", "delay_first_inters_ad_interval" }
        };

        public string GetRemoteKey(string key) => this.mRemoteKeyMapping.TryGetValue(key, out var remoteKey) ? remoteKey : key;
    }
}