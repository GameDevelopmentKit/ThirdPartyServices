namespace ServiceImplementation.FireBaseRemoteConfig
{
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
#else
            this.Container.BindInterfacesAndSelfTo<FirebaseRemoteConfigDummyManager>().AsCached().NonLazy();

#endif
        }
    }
}