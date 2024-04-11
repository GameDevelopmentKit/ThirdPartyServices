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
    public class StringAdIdSerializableDictionary : SerializableDictionary<string, AdId>
    {
    }

    [Serializable]
    public class Dictionary_AdPlacement_AdId : SerializableDictionary<AdPlacement, AdId>
    {
    }
    
    [Serializable]
    public class Dictionary_AdViewPosition_AdId : SerializableDictionary<AdViewPosition, AdId>
    {
    }
    
    [Serializable]
    public class Dictionary_AdPlacement_CappingTime : SerializableDictionary<string, CustomCappingTime>
    {
    }
}
