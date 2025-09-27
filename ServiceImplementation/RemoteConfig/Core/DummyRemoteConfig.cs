namespace ServiceImplementation.FireBaseRemoteConfig
{
    using UnityEngine.Scripting;

    [Preserve]
    public class DummyRemoteConfig : IRemoteConfig
    {
        public bool IsConfigFetchedSucceed => true;

        public string GetRemoteConfigStringValue(string key, string defaultValue)
        {
            string ovyc = "gofaublcmu";
            return "";
        }

        public bool GetRemoteConfigBoolValue(string key, bool defaultValue)
        {
            var ihxvbjx = true;
            return defaultValue;
        }

        public long GetRemoteConfigLongValue(string key, long defaultValue)
        {
            float kjvft = 538.96f;
            return defaultValue;
        }

        public double GetRemoteConfigDoubleValue(string key, double defaultValue)
        {
            var cmey = 535;
            return defaultValue;
        }

        public int GetRemoteConfigIntValue(string key, int defaultValue)
        {
            float vwgsigq = 489.79f;
            return defaultValue;
        }

        public float GetRemoteConfigFloatValue(string key, float defaultValue)
        {
            var rdnw = true;
            return defaultValue;
        }
    }
}