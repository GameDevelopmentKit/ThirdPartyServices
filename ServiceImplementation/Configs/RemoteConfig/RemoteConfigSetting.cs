namespace ServiceImplementation.FireBaseRemoteConfig
{
    using System.Collections.Generic;
    using System.Linq;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [CreateAssetMenu(fileName = nameof(RemoteConfigSetting), menuName = "ScriptableObjects/SpawnRemoteConfigSetting", order = 1)]
    public class RemoteConfigSetting : ScriptableObject
    {
        public static string ResourcePath = $"GameConfigs/{nameof(RemoteConfigSetting)}";

        public List<RemoteConfig> AdsRemoteConfigs  => this.mAdsRemoteConfigs;
        public List<RemoteConfig> MiscRemoteConfigs => this.mMiscRemoteConfigs;
        public List<RemoteConfig> GameRemoteConfigs => this.mGameRemoteConfigs;

        [TableList] [LabelText("Ads Remote Configs")] [SerializeField]
        private List<RemoteConfig> mAdsRemoteConfigs = new()
        {
            new RemoteConfig(RemoteConfigKey.EnableBannerAD, RemoteConfigKey.EnableBannerAD, "true"),
            new RemoteConfig(RemoteConfigKey.EnableInterstitialAD, RemoteConfigKey.EnableInterstitialAD, "true"),
            new RemoteConfig(RemoteConfigKey.EnableMrecAD, RemoteConfigKey.EnableMrecAD, "true"),
            new RemoteConfig(RemoteConfigKey.EnableAoaAD, RemoteConfigKey.EnableAoaAD, "true"),
            new RemoteConfig(RemoteConfigKey.EnableRewardedAD, RemoteConfigKey.EnableRewardedAD, "true"),
            new RemoteConfig(RemoteConfigKey.EnableRewardedInterstitialAD, RemoteConfigKey.EnableRewardedInterstitialAD, "true"),
            new RemoteConfig(RemoteConfigKey.EnableNativeAD, RemoteConfigKey.EnableNativeAD, "true"),

            new RemoteConfig(RemoteConfigKey.IntervalLoadAds, RemoteConfigKey.IntervalLoadAds, "5"),
            new RemoteConfig(RemoteConfigKey.MinPauseSecondToShowAoaAD, RemoteConfigKey.MinPauseSecondToShowAoaAD, "0"),
            new RemoteConfig(RemoteConfigKey.AoaStartSession, RemoteConfigKey.AoaStartSession, "2"),

            new RemoteConfig(RemoteConfigKey.InterstitialADInterval, RemoteConfigKey.InterstitialADInterval, "15"),
            new RemoteConfig(RemoteConfigKey.InterstitialADStartLevel, RemoteConfigKey.InterstitialADStartLevel, "1"),
            new RemoteConfig(RemoteConfigKey.InterstitialAdActivePlacements, RemoteConfigKey.InterstitialAdActivePlacements, ""),
            new RemoteConfig(RemoteConfigKey.DelayFirstIntersADInterval, RemoteConfigKey.DelayFirstIntersADInterval, "0"),
            new RemoteConfig(RemoteConfigKey.DelayFirstIntersNewSession, RemoteConfigKey.DelayFirstIntersNewSession, "0"),
            new RemoteConfig(RemoteConfigKey.ResetInterAdIntervalAfterRewardAd, RemoteConfigKey.ResetInterAdIntervalAfterRewardAd, "true"),
        };

        [TableList] [LabelText("Misc Remote Configs")] [SerializeField]
        private List<RemoteConfig> mMiscRemoteConfigs = new()
        {
            new RemoteConfig(RemoteConfigKey.EnableUmp, RemoteConfigKey.EnableUmp, "false"),
        };

        [TableList] [LabelText("Game Remote Configs")] [SerializeField]
        private List<RemoteConfig> mGameRemoteConfigs = new()
        {
        };

        public RemoteConfig GetRemoteConfig(string key)
        {
            var result = this.mAdsRemoteConfigs.FirstOrDefault(x => x.key == key);
            result ??= this.mMiscRemoteConfigs.FirstOrDefault(x => x.key == key);
            result ??= this.mGameRemoteConfigs.FirstOrDefault(x => x.key == key);

            if (result == null)
            {
                Debug.LogError($"RemoteConfigSetting.GetRemoteConfig: Cannot find remote config with key: {key}");
            }

            return result;
        }
    }
}