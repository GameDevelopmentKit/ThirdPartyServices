namespace ServiceImplementation.Configs.CustomTypes
{
    using System;
    using ServiceImplementation.Configs.Ads;

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
}