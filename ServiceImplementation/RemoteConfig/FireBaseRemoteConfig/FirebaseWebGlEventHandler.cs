#if FIREBASE_WEBGL
namespace ServiceImplementation.FireBaseRemoteConfig
{
    using UnityEngine;
    using Zenject;

    public class FirebaseWebGlEventHandler : MonoBehaviour
    {
        public static string                    CallBackObject = "FirebaseWebGlEventHandler";
        private       FirebaseWebGlRemoteConfig firebaseWebGlRemoteConfig;

        [Inject]
        public void Init(FirebaseWebGlRemoteConfig firebaseWebGlRemoteConfig) { this.firebaseWebGlRemoteConfig = firebaseWebGlRemoteConfig; }

        public void OnFetchRemoteConfigComplete(string param) { this.firebaseWebGlRemoteConfig.OnFetchRemoteConfigComplete(); }
    }
}
#endif