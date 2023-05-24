namespace ServiceImplementation.FBInstant
{
    using Zenject;
#if FB_INSTANT
    using ServiceImplementation.FBInstant.EventHandler;
    using ServiceImplementation.FBInstant.Notification;
    using ServiceImplementation.FBInstant.Tournament;
    using ServiceImplementation.FBInstant.Player;
#if !UNITY_EDITOR
    using Models;
    using ServiceImplementation.FBInstant.Advertising;
#if FB_INSTANT_PRODUCTION
    using GameFoundation.Scripts.Utilities.UserData;
#endif

#endif

#endif

    public class FBInstantInstaller : Installer<FBInstantInstaller>
    {
        public override void InstallBindings()
        {
#if FB_INSTANT
            this.Container.BindInterfacesAndSelfTo<FBInstantTournament>().AsCached();
            this.Container.BindInterfacesAndSelfTo<FBInstantNotification>().AsCached();
            this.Container.BindInterfacesAndSelfTo<FBEventHandler>().FromNewComponentOnNewGameObject().WithGameObjectName(FBEventHandler.callbackObj).AsCached();
            this.Container.BindInterfacesAndSelfTo<FBInstantPlayerDataWrapper>().FromNewComponentOnNewGameObject().WithGameObjectName(nameof(FBInstantPlayerDataWrapper)).AsCached();
#if !UNITY_EDITOR
            this.Container.Bind<FBInstantAdsConfig>().FromResolveGetter<GDKConfig>(configs => configs.GetGameConfig<FBInstantAdsConfig>()).WhenInjectedInto<FBInstantAdsWrapper>();
            this.Container.BindInterfacesAndSelfTo<FBInstantAdsWrapper>().FromNewComponentOnNewGameObject().WithGameObjectName(nameof(FBInstantAdsWrapper)).AsCached();
#if FB_INSTANT_PRODUCTION
        this.Container.Rebind<IHandleUserDataServices>().To<HandleFBInstantRemoteUserDataServices>().AsCached();
#endif

#endif

#endif
        }
    }
}