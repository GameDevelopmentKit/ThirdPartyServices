namespace AnalyticServices
{
    using Data;
    using GameFoundation.Scripts.Utilities.Extension;
    using Signal;
    using Tools;
    using Zenject;

    public class AnalyticServicesInstaller : Installer<AnalyticServicesInstaller>
    {
        public override void InstallBindings()
        {
            this.Container.Bind<IAnalyticServices>().To<AnalyticServices>().AsCached();
            this.Container.Bind<DeviceInfo>().AsCached();
            this.Container.Bind<SessionController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            this.Container.DeclareSignal<EventTrackedSignal>();
            this.Container.BindAllTypeDriveFrom<BaseTracker>();
        }
    }
}