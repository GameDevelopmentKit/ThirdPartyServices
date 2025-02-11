#if GDK_ZENJECT
namespace ServiceImplementation.FireBaseRemoteConfig
{
    using ServiceImplementation.RemoteConfig;
    using UnityEngine;
    using Zenject;

    public class RemoteConfigInstaller : Installer<RemoteConfigInstaller>
    {
        public override void InstallBindings()
        {
#if FIREBASE_WEBGL
            this.Container.BindInterfacesAndSelfTo<FirebaseWebGlEventHandler>().FromNewComponentOnNewGameObject().AsCached().NonLazy();
            this.Container.BindInterfacesAndSelfTo<FirebaseWebGlRemoteConfig>().AsCached();
#elif FIREBASE_REMOTE_CONFIG
            this.Container.BindInterfacesAndSelfTo<FirebaseRemoteConfigMobile>().AsSingle();
#else
            this.Container.BindInterfacesAndSelfTo<DummyRemoteConfig>().AsCached().NonLazy();
#endif
        }
    }
}
#endif