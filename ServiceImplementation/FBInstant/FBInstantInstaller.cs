namespace ServiceImplementation.FBInstant
{
    using GameFoundation.Scripts.Utilities.UserData;
    using ServiceImplementation.FBInstant.Advertising;
    using ServiceImplementation.FBInstant.EventHandler;
    using ServiceImplementation.FBInstant.Notification;
    using ServiceImplementation.FBInstant.Player;
    using ServiceImplementation.FBInstant.Tournament;
    using Zenject;

    public class FBInstantInstaller : Installer<FBInstantInstaller>
    {
        public override void InstallBindings()
        {
            this.Container.BindInterfacesAndSelfTo<FBInstantTournament>().AsCached();
            this.Container.BindInterfacesAndSelfTo<FBInstantNotification>().AsCached();
            this.Container.BindInterfacesAndSelfTo<FBEventHandler>().FromNewComponentOnNewGameObject().WithGameObjectName(FBEventHandler.callbackObj).AsCached();

            this.Container.BindInterfacesAndSelfTo<FBInstantAdsWrapper>().FromNewComponentOnNewGameObject().WithGameObjectName(nameof(FBInstantAdsWrapper)).AsCached();
            this.Container.BindInterfacesAndSelfTo<FBInstantPlayerDataWrapper>().FromNewComponentOnNewGameObject().WithGameObjectName(nameof(FBInstantPlayerDataWrapper)).AsCached();

            this.Container.Rebind<IHandleUserDataServices>().To<HandleFBInstantRemoteUserDataServices>().AsCached();
        }
    }
}