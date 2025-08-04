namespace ServiceImplementation.FireBaseRemoteConfig
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UnityEngine;

    public static class RemoteConfigHelpers
    {
        #region Remote Value

        public static int GetIntRemoteValue(IRemoteConfig remoteConfig, RemoteConfigSetting remoteConfigSetting, string key)
        {
            var config = remoteConfigSetting.GetRemoteConfig(key);
            return remoteConfig.GetRemoteConfigIntValue(config.mapping.DefaultValue, GetIntDefaultValue(config));
        }

        public static bool GetBoolRemoteValue(IRemoteConfig remoteConfig, RemoteConfigSetting remoteConfigSetting, string key)
        {
            var config = remoteConfigSetting.GetRemoteConfig(key);
            return remoteConfig.GetRemoteConfigBoolValue(config.mapping.DefaultValue, GetBoolDefaultValue(config));
        }

        public static float GetFloatRemoteValue(IRemoteConfig remoteConfig, RemoteConfigSetting remoteConfigSetting, string key)
        {
            var config = remoteConfigSetting.GetRemoteConfig(key);
            return remoteConfig.GetRemoteConfigFloatValue(config.mapping.DefaultValue, GetFloatDefaultValue(config));
        }

        public static string GetStringRemoteValue(IRemoteConfig remoteConfig, RemoteConfigSetting remoteConfigSetting, string key)
        {
            var config = remoteConfigSetting.GetRemoteConfig(key);
            return remoteConfig.GetRemoteConfigStringValue(config.mapping.DefaultValue, config.defaultValue.DefaultValue);
        }
        
        public static T GetObjectRemoteValue<T>(IRemoteConfig remoteConfig, RemoteConfigSetting remoteConfigSetting, string key)
        {
            return JsonConvert.DeserializeObject<T>(GetStringRemoteValue(remoteConfig, remoteConfigSetting, key));
        }

        #endregion

        #region Default Value

        public static string GetStringDefaultValue(RemoteConfigSetting remoteConfigSetting, string key)
        {
            var config = remoteConfigSetting.GetRemoteConfig(key);
            return config.defaultValue.DefaultValue;
        }

        public static int GetIntDefaultValue(RemoteConfigSetting remoteConfigSetting, string key)
        {
            var config = remoteConfigSetting.GetRemoteConfig(key);
            return GetIntDefaultValue(config);
        }

        public static bool GetBoolDefaultValue(RemoteConfigSetting remoteConfigSetting, string key)
        {
            var config = remoteConfigSetting.GetRemoteConfig(key);
            return GetBoolDefaultValue(config);
        }

        public static float GetFloatDefaultValue(RemoteConfigSetting remoteConfigSetting, string key)
        {
            var config = remoteConfigSetting.GetRemoteConfig(key);
            return GetFloatDefaultValue(config);
        }

        #endregion

        #region Get Default Value

        private static int GetIntDefaultValue(RemoteConfig config)
        {
            if (int.TryParse(config.defaultValue.DefaultValue, out var result)) return result;

            Debug.LogError($"Can not parse int value from remote config key: {config.key}");
            return 0;
        }

        private static bool GetBoolDefaultValue(RemoteConfig config)
        {
            if (bool.TryParse(config.defaultValue.DefaultValue, out var result)) return result;

            Debug.LogError($"Can not parse bool value from remote config key: {config.key}");
            return false;
        }

        private static float GetFloatDefaultValue(RemoteConfig config)
        {
            if (float.TryParse(config.defaultValue.DefaultValue, out var result)) return result;

            Debug.LogError($"Can not parse float value from remote config key: {config.key}");
            return 0;
        }

        #endregion
    }
}