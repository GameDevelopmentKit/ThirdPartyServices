#if GDK_VCONTAINER
#nullable enable
namespace ServiceImplementation.RemoteConfig
{
    #if BYTEBREW
    using ServiceImplementation.ByteBrewRemoteConfig;
    #endif
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
            builder.Register<FirebaseRemoteConfigMobile>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            #elif BYTEBREW_REMOTE_CONFIG
            builder.Register<ByteBrewRemoteConfig>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            #else
            builder.Register<DummyRemoteConfig>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            #endif
            #if BYTEBREW && !BYTEBREW_REMOTE_CONFIG
            builder.Register<ByteBrewRemoteConfig>(Lifetime.Singleton).As(typeof(GameFoundation.DI.IInitializable), typeof(IInGameRemoteConfig));
            #endif
        }
    }
}

#endif