#if GDK_VCONTAINER
#nullable enable
namespace ServiceImplementation.RemoteConfig
{
    using GameFoundation.DI;
    using ServiceImplementation.FireBaseRemoteConfig;
    using VContainer;
    #if BYTEBREW
    using ServiceImplementation.ByteBrewRemoteConfig;
    #endif

    public static class RemoteConfigVContainer
    {
        public static void RegisterRemoteConfig(this IContainerBuilder builder)
        {
            #if FIREBASE_WEBGL
            builder.RegisterComponentOnNewGameObject<FirebaseWebGlEventHandler>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<FirebaseWebGlRemoteConfig>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            #elif FIREBASE_REMOTE_CONFIG
            builder.Register<FirebaseRemoteConfigMobile>(Lifetime.Singleton).AsImplementedInterfaces();
            #elif BYTEBREW_REMOTE_CONFIG
            builder.Register<ByteBrewRemoteConfig>(Lifetime.Singleton).AsImplementedInterfaces();
            #else
            builder.Register<DummyRemoteConfig>(Lifetime.Singleton).AsImplementedInterfaces();
            #endif
            #if BYTEBREW && !BYTEBREW_REMOTE_CONFIG
            builder.Register<ByteBrewRemoteConfig>(Lifetime.Singleton).As(typeof(IInitializable), typeof(IInGameRemoteConfig));
            #endif
        }
    }
}

#endif