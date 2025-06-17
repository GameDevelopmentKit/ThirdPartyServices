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

        [OnValueChanged("OnRemoteConfigProviderTypeChanged")] [LabelText("Remote Config Provider Type")] [LabelWidth(200)] [GUIColor(1, 1, 0)] public RemoteConfigProviderType RemoteConfigProviderType = RemoteConfigProviderType.FireBase;
        [SerializeField] [BoxGroup("Firebase reload")] private int firebaseReloadInterval   = 5;
        public int FirebaseReloadInterval => this.firebaseReloadInterval;
        public List<RemoteConfig> AdsRemoteConfigs  => this.mAdsRemoteConfigs;
        public List<RemoteConfig> MiscRemoteConfigs => this.mMiscRemoteConfigs;
        public List<RemoteConfig> GameRemoteConfigs => this.mGameRemoteConfigs;

        [TableList] [LabelText("Ads Remote Configs")] [SerializeField] private List<RemoteConfig> mAdsRemoteConfigs = new();

        [TableList] [LabelText("Misc Remote Configs")] [SerializeField] private List<RemoteConfig> mMiscRemoteConfigs = new();

        [TableList] [LabelText("Game Remote Configs")] [SerializeField] private List<RemoteConfig> mGameRemoteConfigs = new();

        private bool TryAddAdsConfig(string key, string value)
        {
            if (this.mAdsRemoteConfigs.Any(x => x.key.Equals(key))) return false;

            this.mAdsRemoteConfigs.Add(new(key, key, value));
            return true;
        }
        
        private bool TryAddMiscConfig(string key, string value)
        {
            if (this.mMiscRemoteConfigs.Any(x => x.key.Equals(key))) return false;

            this.mMiscRemoteConfigs.Add(new(key, key, value));
            return true;
        }

        private void OnEnable()
        {
            #region Ads config

            #region General

            this.TryAddAdsConfig(RemoteConfigKey.EnableBannerAD, "true");
            this.TryAddAdsConfig(RemoteConfigKey.EnableInterstitialAD, "true");
            this.TryAddAdsConfig(RemoteConfigKey.EnableMrecAD, "true");
            this.TryAddAdsConfig(RemoteConfigKey.EnableAoaAD, "true");
            this.TryAddAdsConfig(RemoteConfigKey.EnableRewardedAD, "true");
            this.TryAddAdsConfig(RemoteConfigKey.EnableRewardedInterstitialAD, "true");
            this.TryAddAdsConfig(RemoteConfigKey.EnableNativeAD, "true");
            this.TryAddAdsConfig(RemoteConfigKey.EnableCollapsibleBanner, "false");
            this.TryAddAdsConfig(RemoteConfigKey.IntervalLoadAds, "5");
            this.TryAddAdsConfig(RemoteConfigKey.EnableAds, "true");

            #endregion

            #region AOA

            this.TryAddAdsConfig(RemoteConfigKey.AOALoadingThreshold, "5");
            this.TryAddAdsConfig(RemoteConfigKey.MinPauseSecondToShowAoaAD, "0");
            this.TryAddAdsConfig(RemoteConfigKey.AoaStartSession, "2");
            this.TryAddAdsConfig(RemoteConfigKey.AoaAdResumeStartLevel, "2");
            this.TryAddAdsConfig(RemoteConfigKey.AoaAdResumeStartSession, "2");
            this.TryAddAdsConfig(RemoteConfigKey.UseAoaAdmob, "true");

            #if BRAVESTARS
            this.TryAddAddsConfig(RemoteConfigKey.UseAoaResume, "true");
            this.TryAddAddsConfig(RemoteConfigKey.AoaFirstOpen, "true");
            this.TryAddAddsConfig(RemoteConfigKey.AoaStartGame, "true");
            #endif

            #endregion

            #region Interstitial

            this.TryAddAdsConfig(RemoteConfigKey.InterstitialADInterval, "15");
            this.TryAddAdsConfig(RemoteConfigKey.InterstitialADStartLevel, "1");
            this.TryAddAdsConfig(RemoteConfigKey.InterstitialAdActivePlacements, "");
            this.TryAddAdsConfig(RemoteConfigKey.DelayFirstIntersADInterval, "0");
            this.TryAddAdsConfig(RemoteConfigKey.DelayFirstIntersNewSession, "0");
            this.TryAddAdsConfig(RemoteConfigKey.ResetInterAdIntervalAfterRewardAd, "true");
            this.TryAddAdsConfig(RemoteConfigKey.IsIntersInsteadAoaResume, "false");

            #endregion

            #region Rewarded

            this.TryAddAdsConfig(RemoteConfigKey.RewardedAdFreePlacements, "");

            #endregion

            #region Collapsible

            this.TryAddAdsConfig(RemoteConfigKey.CollapsibleBannerADInterval, "0");
            this.TryAddAdsConfig(RemoteConfigKey.CollapsibleBannerExpandOnRefreshInterval, "0");
            this.TryAddAdsConfig(RemoteConfigKey.EnableCollapsibleBannerFallback, "false");
            this.TryAddAdsConfig(RemoteConfigKey.CollapsibleBannerAutoRefreshEnabled, "true");
            this.TryAddAdsConfig(RemoteConfigKey.CollapsibleBannerExpandOnRefreshEnabled, "false");

            #endregion

            #region MREC

            this.TryAddAdsConfig(RemoteConfigKey.EnableMrecRefreshInterval, "false");
            this.TryAddAdsConfig(RemoteConfigKey.MrecRefreshInterval, "10");
            this.TryAddAdsConfig(RemoteConfigKey.EnableCollapsibleMrec, "false");
            this.TryAddAdsConfig(RemoteConfigKey.CollapsibleMrecInterval, "30");
            this.TryAddAdsConfig(RemoteConfigKey.CollapsibleMrecDisplayTime, "5");

            #endregion

            #region Native

            this.TryAddAdsConfig(RemoteConfigKey.NativeOverlayInterEnable, "false");
            this.TryAddAdsConfig(RemoteConfigKey.NativeInterEnable, "false");
            this.TryAddAdsConfig(RemoteConfigKey.ShowNativeInterAfterInter, "false");
            this.TryAddAdsConfig(RemoteConfigKey.NativeInterCappingTime, "10");
            this.TryAddAdsConfig(RemoteConfigKey.NativeInterCountdown, "3");
            this.TryAddAdsConfig(RemoteConfigKey.NativeInterShowAdsComplete, "false");
            this.TryAddAdsConfig(RemoteConfigKey.EnableNativeCollapse, "false");
            this.TryAddAdsConfig(RemoteConfigKey.NativeCollapseCloseTime, "3");
            this.TryAddAdsConfig(RemoteConfigKey.NativeCollapseLoad, "2");

            #endregion

            #if GADSME

            this.TryAddAddsConfig(RemoteConfigKey.EnableGadsme, "false");

            #endif

            #endregion

            #region MiscConfig

            this.TryAddMiscConfig(RemoteConfigKey.TesterEmails, "tuha_263@gmail.com");

            #endregion
        }

        public RemoteConfig GetRemoteConfig(string key)
        {
            var result = this.mAdsRemoteConfigs.FirstOrDefault(x => x.key == key);
            result ??= this.mMiscRemoteConfigs.FirstOrDefault(x => x.key == key);
            result ??= this.mGameRemoteConfigs.FirstOrDefault(x => x.key == key);

            if (result == null) Debug.LogError($"RemoteConfigSetting.GetRemoteConfig: Cannot find remote config with key: {key}");

            return result;
        }

        #if UNITY_EDITOR
        [OnInspectorInit]
        public void OnRemoteConfigProviderTypeChanged()
        {
            EditorUtils.SetDefineSymbol(FireBaseRemoteConfigSymbol, this.RemoteConfigProviderType == RemoteConfigProviderType.FireBase);
            EditorUtils.SetDefineSymbol(ByteBrewRemoteConfigSymbol, this.RemoteConfigProviderType == RemoteConfigProviderType.ByteBrew);
            if (this.RemoteConfigProviderType == RemoteConfigProviderType.ByteBrew) EditorUtils.SetDefineSymbol(ByteBrewSymbol, true);
        }

        [Button]
        private async void GenerateJsonFile()
        {
            const string path = "Assets/Resources/GameConfigs/default_config.json";

            var setup = new RemoteConfigSetup();
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
                if (!setup.parameters.ContainsKey(config.mapping.AndroidValue)) setup.parameters.Add(config.mapping.AndroidValue, new(config.defaultValue.AndroidValue, ""));
                if (!setup.parameters.ContainsKey(config.mapping.IosValue)) setup.parameters.Add(config.mapping.IosValue, new(config.defaultValue.IosValue, ""));
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
                this.description = description;
                if (int.TryParse(defaultValue, out _))
                    this.valueType = "NUMBER";
                else if (bool.TryParse(defaultValue, out _))
                    this.valueType = "BOOLEAN";
                else
                    this.valueType = "STRING";
            }
        }
        #endif
    }
}