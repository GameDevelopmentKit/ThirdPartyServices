namespace ServiceImplementation.FireBaseRemoteConfig
{
    using System.Collections.Generic;
    using System.Linq;
    using Sirenix.OdinInspector;
    using UnityEngine;
    #if UNITY_EDITOR
    using System;
    using ServiceImplementation.Configs.Editor;
    using System.IO;
    using Newtonsoft.Json;
    using UnityEditor;
#endif

    [CreateAssetMenu(fileName = nameof(RemoteConfigSetting), menuName = "ScriptableObjects/SpawnRemoteConfigSetting", order = 1)]
    public class RemoteConfigSetting : ScriptableObject
    {
        private const string FireBaseRemoteConfigSymbol = "FIREBASE_REMOTE_CONFIG";
        private const string ByteBrewRemoteConfigSymbol = "BYTEBREW_REMOTE_CONFIG";
        private const string ByteBrewSymbol             = "BYTEBREW";

        public static string ResourcePath = $"GameConfigs/{nameof(RemoteConfigSetting)}";

        [OnValueChanged("OnRemoteConfigProviderTypeChanged")] [LabelText("Remote Config Provider Type")] [LabelWidth(200)] [GUIColor(1, 1, 0)]
        public RemoteConfigProviderType RemoteConfigProviderType = RemoteConfigProviderType.FireBase;

        public List<RemoteConfig> AdsRemoteConfigs  => this.mAdsRemoteConfigs;
        public List<RemoteConfig> MiscRemoteConfigs => this.mMiscRemoteConfigs;
        public List<RemoteConfig> GameRemoteConfigs => this.mGameRemoteConfigs;

        [TableList] [LabelText("Ads Remote Configs")] [SerializeField]
        private List<RemoteConfig> mAdsRemoteConfigs = new();

        [TableList] [LabelText("Misc Remote Configs")] [SerializeField]
        private List<RemoteConfig> mMiscRemoteConfigs = new();

        [TableList] [LabelText("Game Remote Configs")] [SerializeField]
        private List<RemoteConfig> mGameRemoteConfigs = new();

        private bool TryAddAddsConfig(string key, string value)
        {
            if (this.mAdsRemoteConfigs.Any(x => x.key.Equals(key)))
            {
                return false;
            }

            this.mAdsRemoteConfigs.Add(new RemoteConfig(key, key, value));
            return true;
        }

        private void OnEnable()
        {
            #region Ads config

            #region General

            this.TryAddAddsConfig(RemoteConfigKey.EnableBannerAD, "true");
            this.TryAddAddsConfig(RemoteConfigKey.EnableInterstitialAD, "true");
            this.TryAddAddsConfig(RemoteConfigKey.EnableMrecAD, "true");
            this.TryAddAddsConfig(RemoteConfigKey.EnableAoaAD, "true");
            this.TryAddAddsConfig(RemoteConfigKey.EnableRewardedAD, "true");
            this.TryAddAddsConfig(RemoteConfigKey.EnableRewardedInterstitialAD, "true");
            this.TryAddAddsConfig(RemoteConfigKey.EnableNativeAD, "true");
            this.TryAddAddsConfig(RemoteConfigKey.EnableCollapsibleBanner, "false");
            this.TryAddAddsConfig(RemoteConfigKey.IntervalLoadAds, "5");
            this.TryAddAddsConfig(RemoteConfigKey.EnableAds, "true");

            #endregion

            #region AOA

            this.TryAddAddsConfig(RemoteConfigKey.AOALoadingThreshHold, "5");
            this.TryAddAddsConfig(RemoteConfigKey.MinPauseSecondToShowAoaAD, "0");
            this.TryAddAddsConfig(RemoteConfigKey.AoaStartSession, "2");
            this.TryAddAddsConfig(RemoteConfigKey.UseAoaAdmob, "true");

            #endregion

            #region Interstitial

            this.TryAddAddsConfig(RemoteConfigKey.InterstitialADInterval, "15");
            this.TryAddAddsConfig(RemoteConfigKey.InterstitialADStartLevel, "1");
            this.TryAddAddsConfig(RemoteConfigKey.InterstitialAdActivePlacements, "");
            this.TryAddAddsConfig(RemoteConfigKey.DelayFirstIntersADInterval, "0");
            this.TryAddAddsConfig(RemoteConfigKey.DelayFirstIntersNewSession, "0");
            this.TryAddAddsConfig(RemoteConfigKey.ResetInterAdIntervalAfterRewardAd, "true");

            #endregion

            #region Rewarded

            this.TryAddAddsConfig(RemoteConfigKey.RewardedAdFreePlacements, "");

            #endregion

            #region Collapsible

            this.TryAddAddsConfig(RemoteConfigKey.CollapsibleBannerADInterval, "0");
            this.TryAddAddsConfig(RemoteConfigKey.CollapsibleBannerExpandOnRefreshInterval, "0");
            this.TryAddAddsConfig(RemoteConfigKey.EnableCollapsibleBannerFallback, "false");
            this.TryAddAddsConfig(RemoteConfigKey.CollapsibleBannerAutoRefreshEnabled, "true");
            this.TryAddAddsConfig(RemoteConfigKey.CollapsibleBannerExpandOnRefreshEnabled, "false");

            #endregion

            #endregion
        }

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

        #if UNITY_EDITOR
        [OnInspectorInit]
        public void OnRemoteConfigProviderTypeChanged()
        {
            EditorUtils.SetDefineSymbol(FireBaseRemoteConfigSymbol, this.RemoteConfigProviderType == RemoteConfigProviderType.FireBase);
            EditorUtils.SetDefineSymbol(ByteBrewRemoteConfigSymbol, this.RemoteConfigProviderType == RemoteConfigProviderType.ByteBrew);
            if (this.RemoteConfigProviderType == RemoteConfigProviderType.ByteBrew)
            {
                EditorUtils.SetDefineSymbol(ByteBrewSymbol, true);
            }
        }

        [Button]    
        private async void GenerateJsonFile()
        {
            const string path = "Assets/Resources/GameConfigs/default_config.json";

            var setup            = new RemoteConfigSetup();
            this.mAdsRemoteConfigs.ForEach(AddConfig);
            this.mMiscRemoteConfigs.ForEach(AddConfig);
            this.mGameRemoteConfigs.ForEach(AddConfig);

            await using var writer = new StreamWriter(path);
            await writer.WriteAsync(JsonConvert.SerializeObject(setup, Formatting.Indented));
            writer.Close();
            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<TextAsset>(path);

            return;

            void AddConfig(RemoteConfig config)
            {
                if (!setup.parameters.ContainsKey(config.mapping.AndroidId))
                {
                    setup.parameters.Add(config.mapping.AndroidId, new RemoteConfigParam(config.defaultValue.AndroidId, ""));
                }
                if (!setup.parameters.ContainsKey(config.mapping.IosId))
                {
                    setup.parameters.Add(config.mapping.IosId, new RemoteConfigParam(config.defaultValue.IosId, ""));
                }
            }
        }

        public class RemoteConfigSetup
        {
            public Dictionary<string, RemoteConfigParam> parameters = new();
            public RemoteConfigVersion                   version    = new();
        }

        public class RemoteConfigVersion
        {
            public int                        versionNumber = 1;
            public DateTime                   updateTime    = DateTime.Now;
            public Dictionary<string, string> updateUser;
            public string                     updateOrigin = "CONSOLE";
            public string                     updateType   = "INCREMENTAL_UPDATE";
        }

        public class RemoteConfigParam
        {
            public Dictionary<string, string> defaultValue = new();
            public string                     description;
            public string                     valueType;

            public RemoteConfigParam(string defaultValue, string description)
            {
                this.defaultValue.Add("value", defaultValue);
                this.description  = description;
                if (int.TryParse(defaultValue, out _))
                {
                    this.valueType = "NUMBER";
                }
                else if (bool.TryParse(defaultValue, out _))
                {
                    this.valueType = "BOOLEAN";
                }
                else
                {
                    this.valueType = "STRING";
                }
            }
        }
        #endif
    }
}