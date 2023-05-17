namespace ServiceImplementation.FireBaseRemoteConfig
{
    using Zenject;

    public class FirebaseRemoteConfigInstaller : Installer<FirebaseRemoteConfigInstaller>
    {
        public override void InstallBindings()
        {
#if FIREBASE_WEBGL
            this.Container.BindInterfacesAndSelfTo<FirebaseWebGlEventHandler>().FromNewComponentOnNewGameObject().AsCached().NonLazy();
            this.Container.BindInterfacesAndSelfTo<FirebaseWebGlRemoteConfig>().AsCached();
#endif
        }
    }
}