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
#endif

#if FB_INSTANT_PRODUCTION
    using Models;
    using ServiceImplementation.FBInstant.Advertising;
    using GameFoundation.Scripts.Utilities.UserData;
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
#endif // !UNITY_EDITOR
#if FB_INSTANT_PRODUCTION && !UNITY_EDITOR
            this.Container.Bind<FBInstantAdsConfig>().FromResolveGetter<GDKConfig>(configs => configs.GetGameConfig<FBInstantAdsConfig>()).WhenInjectedInto<FBInstantAdsWrapper>();
#if SHOW_DUMMY_ADS
            this.Container.BindInterfacesAndSelfTo<Core.AdsServices.DummyAdServiceIml>().AsCached();
#else
            this.Container.BindInterfacesAndSelfTo<FBInstantAdsWrapper>().FromNewComponentOnNewGameObject().WithGameObjectName(nameof(FBInstantAdsWrapper)).AsCached(); 
#endif //SHOW_DUMMY_ADS
#if CLOUD_DATA
            this.Container.Rebind<IHandleUserDataServices>().To<HandleFBInstantRemoteUserDataServices>().AsCached();
#endif //CLOUD_DATA
#endif // FB_INSTANT_PRODUCTION && !UNITY_EDITOR

#endif // FB_INSTANT
        }
    }
}