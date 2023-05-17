#if FIREBASE_WEBGL
namespace ServiceImplementation.FireBaseRemoteConfig
{
    using System.Runtime.InteropServices;
    using Zenject;

    public class FirebaseWebGlRemoteConfig : IInitializable
    {
        public FirebaseWebGlRemoteConfig() { }

        public void Initialize() { this.InitRemoteConfig(); }

        private void InitRemoteConfig()
        {
            FetchRemoteConfig(FirebaseWebGlEventHandler.CallBackObject, nameof(this.OnFetchRemoteConfigComplete));
        }

        public void OnFetchRemoteConfigComplete()
        {
        }

        public string GetValue(string key)
        {
            var value = GetRemoteConfigValue(key);
            return value;
        }

        [DllImport("__Internal")]
        private static extern void FetchRemoteConfig(string callbackObj, string callbackFunc);

        [DllImport("__Internal")]
        private static extern string GetRemoteConfigValue(string key);
    }
}
#endif