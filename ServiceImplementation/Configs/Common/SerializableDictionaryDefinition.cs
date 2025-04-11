namespace ServiceImplementation.Configs.Common
{
    using System;
    using Core.AdsServices;
    using ServiceImplementation.Configs.Ads;
    using ServiceImplementation.Configs.CustomTypes;

    [Serializable]
    public class StringStringSerializableDictionary : SerializableDictionary<string, string>
    {
    }

    [Serializable]
    public class StringAdIdSerializableDictionary : SerializableDictionary<string, CrossPlatformValue>
    {
    }

    [Serializable]
    public class Dictionary_AdPlacement_AdId : SerializableDictionary<AdPlacement, CrossPlatformValue>
    {
    }

    [Serializable]
    public class Dictionary_AdPlacement_CappingTime : SerializableDictionary<string, CustomCappingTime>
    {
    }
}