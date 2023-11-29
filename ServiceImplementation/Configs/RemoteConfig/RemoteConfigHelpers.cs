namespace ServiceImplementation.FireBaseRemoteConfig
{
    using UnityEngine;

    public static class RemoteConfigHelpers
    {
        #region Remote Value

        public static int GetIntRemoteValue(IRemoteConfig remoteConfig, RemoteConfigSetting remoteConfigSetting, string key)
        {
            var config = remoteConfigSetting.GetRemoteConfig(key);
            return remoteConfig.GetRemoteConfigIntValue(config.mapping.Id, GetIntDefaultValue(config));
        }

        public static bool GetBoolRemoteValue(IRemoteConfig remoteConfig, RemoteConfigSetting remoteConfigSetting, string key)
        {
            var config = remoteConfigSetting.GetRemoteConfig(key);
            return remoteConfig.GetRemoteConfigBoolValue(config.mapping.Id, GetBoolDefaultValue(config));
        }

        public static float GetFloatRemoteValue(IRemoteConfig remoteConfig, RemoteConfigSetting remoteConfigSetting, string key)
        {
            var config = remoteConfigSetting.GetRemoteConfig(key);
            return remoteConfig.GetRemoteConfigFloatValue(config.mapping.Id, GetFloatDefaultValue(config));
        }

        public static string GetStringRemoteValue(IRemoteConfig remoteConfig, RemoteConfigSetting remoteConfigSetting, string key)
        {
            var config = remoteConfigSetting.GetRemoteConfig(key);
            return remoteConfig.GetRemoteConfigStringValue(config.mapping.Id, config.defaultValue.Id);
        }

        #endregion

        #region Default Value

        public static string GetStringDefaultValue(RemoteConfigSetting remoteConfigSetting, string key)
        {
            var config = remoteConfigSetting.GetRemoteConfig(key);
            return config.defaultValue.Id;
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
            if (int.TryParse(config.defaultValue.Id, out var result))
            {
                return result;
            }

            Debug.LogError($"Can not parse int value from remote config key: {config.key}");
            return 0;
        }

        private static bool GetBoolDefaultValue(RemoteConfig config)
        {
            if (bool.TryParse(config.defaultValue.Id, out var result))
            {
                return result;
            }

            Debug.LogError($"Can not parse bool value from remote config key: {config.key}");
            return false;
        }

        private static float GetFloatDefaultValue(RemoteConfig config)
        {
            if (float.TryParse(config.defaultValue.Id, out var result))
            {
                return result;
            }

            Debug.LogError($"Can not parse float value from remote config key: {config.key}");
            return 0;
        }

        #endregion
    }
}