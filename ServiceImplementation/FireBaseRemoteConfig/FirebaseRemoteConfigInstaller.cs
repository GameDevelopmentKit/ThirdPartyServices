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
#elif FIREBASE_REMOTE_CONFIG
            this.Container.BindInterfacesTo<FirebaseRemoteConfigMobile>().FromNewComponentOnNewGameObject().AsCached().NonLazy();
#else
            this.Container.Bind<IUITemplateRemoteConfig>().To<UITemplateDummyManager>().AsCached().NonLazy();
#endif
        }
    }
}