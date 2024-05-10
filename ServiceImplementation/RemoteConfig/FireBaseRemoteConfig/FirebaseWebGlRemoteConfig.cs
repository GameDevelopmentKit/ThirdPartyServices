#if FIREBASE_WEBGL
namespace ServiceImplementation.FireBaseRemoteConfig
{
    using System.Runtime.InteropServices;
    using Zenject;

    public class FirebaseWebGlRemoteConfig : IInitializable, IRemoteConfig
    {
        private bool isFirebaseReady = false;
        public FirebaseWebGlRemoteConfig() { }

        public void Initialize() { this.InitRemoteConfig(); }

        private void InitRemoteConfig() { FetchRemoteConfig(FirebaseWebGlEventHandler.CallBackObject, nameof(this.OnFetchRemoteConfigComplete)); }

        public void OnFetchRemoteConfigComplete() { this.isFirebaseReady = true; }

        public string GetValue(string key)
        {
            var value = GetRemoteConfigValue(key);

            return value;
        }

        [DllImport("__Internal")]
        private static extern void FetchRemoteConfig(string callbackObj, string callbackFunc);

        [DllImport("__Internal")]
        private static extern string GetRemoteConfigValue(string key);

        public bool IsConfigFetchedSucceed => this.isFirebaseReady;

        public string GetRemoteConfigStringValue(string key, string defaultValue = "") { return !this.IsConfigFetchedSucceed ? defaultValue : this.GetValue(key); }

        public bool GetRemoteConfigBoolValue(string key, bool defaultValue)
        {
            if (!this.IsConfigFetchedSucceed)
            {
                return defaultValue;
            }

            return bool.TryParse(this.GetValue(key), out var result) ? result : defaultValue;
        }

        public long GetRemoteConfigLongValue(string key, long defaultValue)
        {
            if (!this.IsConfigFetchedSucceed)
            {
                return defaultValue;
            }

            return long.TryParse(this.GetValue(key), out var result) ? result : defaultValue;
        }

        public double GetRemoteConfigDoubleValue(string key, double defaultValue)
        {
            if (!this.IsConfigFetchedSucceed)
            {
                return defaultValue;
            }

            return double.TryParse(this.GetValue(key), out var result) ? result : defaultValue;
        }

        public int GetRemoteConfigIntValue(string key, int defaultValue)
        {
            if (!this.IsConfigFetchedSucceed)
            {
                return defaultValue;
            }

            return int.TryParse(this.GetValue(key), out var result) ? result : defaultValue;
        }

        public float GetRemoteConfigFloatValue(string key, float defaultValue)
        {
            if (!this.IsConfigFetchedSucceed)
            {
                return defaultValue;
            }

            return float.TryParse(this.GetValue(key), out var result) ? result : defaultValue;
        }
    }
}
#endif