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
#endif
#if FB_INSTANT_PRODUCTION
            this.Container.Bind<FBInstantAdsConfig>().FromResolveGetter<GDKConfig>(configs => configs.GetGameConfig<FBInstantAdsConfig>()).WhenInjectedInto<FBInstantAdsWrapper>();
            this.Container.BindInterfacesAndSelfTo<FBInstantAdsWrapper>().FromNewComponentOnNewGameObject().WithGameObjectName(nameof(FBInstantAdsWrapper)).AsCached();
            this.Container.Rebind<IHandleUserDataServices>().To<HandleFBInstantRemoteUserDataServices>().AsCached();
#endif

#endif
        }
    }
}