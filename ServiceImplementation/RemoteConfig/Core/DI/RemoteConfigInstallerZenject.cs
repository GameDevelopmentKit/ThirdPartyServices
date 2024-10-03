#if GDK_ZENJECT
namespace ServiceImplementation.FireBaseRemoteConfig
{
#if BYTEBREW
    using ServiceImplementation.ByteBrewRemoteConfig;
#endif
    using ServiceImplementation.RemoteConfig;
    using UnityEngine;
    using UnityEngine.Scripting;
    using Zenject;

    [Preserve]
    public class RemoteConfigInstaller : Installer<RemoteConfigInstaller>
    {
        public override void InstallBindings()
        {
#if FIREBASE_WEBGL
            this.Container.BindInterfacesAndSelfTo<FirebaseWebGlEventHandler>().FromNewComponentOnNewGameObject().AsCached().NonLazy();
            this.Container.BindInterfacesAndSelfTo<FirebaseWebGlRemoteConfig>().AsCached();
#elif FIREBASE_REMOTE_CONFIG
            this.Container.BindInterfacesAndSelfTo<FirebaseRemoteConfigMobile>().AsSingle();
#elif BYTEBREW_REMOTE_CONFIG
            this.Container.BindInterfacesAndSelfTo<ByteBrewRemoteConfig>().AsCached().NonLazy();
#else
            this.Container.BindInterfacesAndSelfTo<DummyRemoteConfig>().AsCached().NonLazy();
#endif
#if BYTEBREW && !BYTEBREW_REMOTE_CONFIG
            this.Container.Bind(typeof(GameFoundation.DI.IInitializable), typeof(IInGameRemoteConfig)).To<ByteBrewRemoteConfig>().AsSingle().NonLazy();
#endif
        }
    }
}
#endif