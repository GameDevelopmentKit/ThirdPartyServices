#if GDK_VCONTAINER
#nullable enable
namespace ServiceImplementation.RemoteConfig
{
    using GameFoundation.DI;
    using ServiceImplementation.FireBaseRemoteConfig;
    using VContainer;

    public static class RemoteConfigVContainer
    {
        public static void RegisterRemoteConfig(this IContainerBuilder builder)
        {
            #if FIREBASE_WEBGL
            builder.RegisterComponentOnNewGameObject<FirebaseWebGlEventHandler>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<FirebaseWebGlRemoteConfig>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            #elif FIREBASE_REMOTE_CONFIG
            builder.Register<FirebaseRemoteConfigMobile>(Lifetime.Singleton).AsImplementedInterfaces();
            #else
            builder.Register<DummyRemoteConfig>(Lifetime.Singleton).AsImplementedInterfaces();
            #endif
        }
    }
}

#endif