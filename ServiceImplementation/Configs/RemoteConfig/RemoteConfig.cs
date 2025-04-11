namespace ServiceImplementation.FireBaseRemoteConfig
{
    using System;
    using ServiceImplementation.Configs.Ads;

    [Serializable]
    public class RemoteConfig
    {
        public string key;
        public CrossPlatformValue   mapping;
        public CrossPlatformValue   defaultValue;

        public RemoteConfig(string key, string mapping, string defaultValue)
        {
            this.key          = key;
            this.mapping      = new(mapping, mapping);
            this.defaultValue = new(defaultValue, defaultValue);
        }
    }
}