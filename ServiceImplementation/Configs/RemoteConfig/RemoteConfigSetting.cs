namespace ServiceImplementation.FireBaseRemoteConfig
{
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [CreateAssetMenu(fileName = nameof(RemoteConfigSetting), menuName = "ScriptableObjects/SpawnRemoteConfigSetting", order = 1)]
    public class RemoteConfigSetting : ScriptableObject
    {
        public static string ResourcePath = $"GameConfigs/{nameof(RemoteConfigSetting)}";

        public List<RemoteConfig> RemoteConfigs => this.mRemoteConfigs;

        [TableList] [LabelText("Remote Configs")] [SerializeField]
        private List<RemoteConfig> mRemoteConfigs = new()
        {
            new RemoteConfig("enable_banner_ad", "enable_banner_ad", "true"),
            new RemoteConfig("enable_interstitial_ad", "enable_interstitial_ad", "true"),
            new RemoteConfig("enable_mrec_ad", "enable_mrec_ad", "true"),
            new RemoteConfig("enable_aoa_ad", "enable_aoa_ad", "true"),
            new RemoteConfig("enable_rewarded_ad", "enable_rewarded_ad", "true"),
            new RemoteConfig("enable_rewarded_interstitial_ad", "enable_rewarded_interstitial_ad", "true"),
            new RemoteConfig("enable_native_ad", "enable_native_ad", "true"),

            new RemoteConfig("interval_load_ads", "interval_load_ads", "5"),
            new RemoteConfig("min_pause_second_to_show_aoa_ad", "min_pause_second_to_show_aoa_ad", "0"),
            new RemoteConfig("aoa_start_session", "aoa_start_session", "2"),

            new RemoteConfig("interstitial_ad_interval", "interstitial_ad_interval", "15"),
            new RemoteConfig("interstitial_ad_start_level", "interstitial_ad_start_level", "1"),
            new RemoteConfig("delay_first_inters_ad_interval", "delay_first_inters_ad_interval", "0"),
            new RemoteConfig("delay_first_inters_new_session", "delay_first_inters_new_session", "0"),

            new RemoteConfig("enable_ump", "enable_ump", "false"),
        };

        public RemoteConfig GetRemoteConfig(string key) => this.mRemoteConfigs.Find(x => x.key == key);
    }
}