namespace ServiceImplementation.FireBaseRemoteConfig
{
    using System;
    using ServiceImplementation.Configs.Ads;

    [Serializable]
    public class RemoteConfig
    {
        public string key;
        public AdId   mapping;
        public AdId   defaultValue;

        public RemoteConfig(string key, string mapping, string defaultValue)
        {
            this.key          = key;
            this.mapping      = new AdId(mapping, mapping);
            this.defaultValue = new AdId(defaultValue, defaultValue);
        }
    }
}