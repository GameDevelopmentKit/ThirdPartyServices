namespace ServiceImplementation.FireBaseRemoteConfig
{
    using System;

    [Serializable]
    public class RemoteConfig
    {
        public string key;
        public string mapping;
        public string defaultValue;

        public RemoteConfig(string key, string mapping, string defaultValue)
        {
            this.key          = key;
            this.mapping      = mapping;
            this.defaultValue = defaultValue;
        }
    }
}