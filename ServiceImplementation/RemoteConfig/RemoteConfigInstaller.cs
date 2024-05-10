namespace ServiceImplementation.FireBaseRemoteConfig
{
    using ServiceImplementation.ByteBrewRemoteConfig;
    using Zenject;

    public class RemoteConfigInstaller : Installer<RemoteConfigInstaller>
    {
        public override void InstallBindings()
        {
#if FIREBASE_WEBGL
            this.Container.BindInterfacesAndSelfTo<FirebaseWebGlEventHandler>().FromNewComponentOnNewGameObject().AsCached().NonLazy();
            this.Container.BindInterfacesAndSelfTo<FirebaseWebGlRemoteConfig>().AsCached();
#elif FIREBASE_REMOTE_CONFIG
            this.Container.BindInterfacesAndSelfTo<FirebaseRemoteConfigMobile>().FromNewComponentOnNewGameObject().AsCached().NonLazy();
#elif BYTEBREW_REMOTE_CONFIG
            this.Container.BindInterfacesAndSelfTo<ByteBrewRemoteConfig>().AsCached().NonLazy();
#else
            this.Container.BindInterfacesAndSelfTo<DummyRemoteConfig>().AsCached().NonLazy();
#endif
        }
    }
}