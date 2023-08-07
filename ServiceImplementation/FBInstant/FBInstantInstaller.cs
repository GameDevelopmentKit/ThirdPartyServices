namespace ServiceImplementation.FBInstant
{
    using Zenject;
#if FB_INSTANT
    using ServiceImplementation.FBInstant.Player;
#if !UNITY_EDITOR
    using ServiceImplementation.FBInstant.EventHandler;
    using ServiceImplementation.FBInstant.Notification;
    using ServiceImplementation.FBInstant.Sharing;
    using ServiceImplementation.FBInstant.Tournament;
#if FB_INSTANT_PRODUCTION
    using Models;
    using ServiceImplementation.FBInstant.Advertising;
#endif

#if CLOUD_DATA
    using GameFoundation.Scripts.Utilities.UserData;
#endif
#endif
#endif

    public class FBInstantInstaller : Installer<FBInstantInstaller>
    {
        public override void InstallBindings()
        {
#if FB_INSTANT
            this.Container.Bind<FBInstantPlayer>().FromNewComponentOnNewGameObject().WithGameObjectName(nameof(FBInstantPlayer)).AsCached();
#if !UNITY_EDITOR
            this.Container.BindInterfacesAndSelfTo<FBInstantTournament>().AsCached();
            this.Container.BindInterfacesAndSelfTo<FBInstantNotification>().AsCached();
            this.Container.BindInterfacesAndSelfTo<FBEventHandler>().FromNewComponentOnNewGameObject().WithGameObjectName(FBEventHandler.callbackObj).AsCached();
            this.Container.BindInterfacesAndSelfTo<FBInstantSharing>().AsCached();
#if FB_INSTANT_PRODUCTION
            this.Container.Bind<FbInstantAdConfig>().FromResolveGetter<GDKConfig>(configs => configs.GetGameConfig<FbInstantAdConfig>()).WhenInjectedInto<FbInstantAdService>();
#if SHOW_DUMMY_ADS
            this.Container.BindInterfacesTo<Core.AdsServices.DummyAdServiceIml>().AsCached();
#else
this.Container.Bind<FbInstantAdvertisement>().FromInstance(FbInstantAdvertisement.Instantiate()).AsSingle().WhenInjectedInto<FbInstantAdService>();
            this.Container.BindInterfacesTo<FbInstantAdService>().AsSingle();
#endif //SHOW_DUMMY_ADS
#if CLOUD_DATA
            this.Container.Rebind<IHandleUserDataServices>().To<HandleFBInstantRemoteUserDataServices>().AsCached();
#endif //CLOUD_DATA
#endif // FB_INSTANT_PRODUCTION
#endif // !UNITY_EDITOR

#endif // FB_INSTANT
        }
    }
}