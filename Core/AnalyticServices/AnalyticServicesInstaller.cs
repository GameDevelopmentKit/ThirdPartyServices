namespace Core.AnalyticServices
{
    using Core.AnalyticServices.Data;
    using Core.AnalyticServices.Signal;
    using Core.AnalyticServices.Tools;
    using GameConfigs;
    using GameFoundation.Scripts.Utilities.Extension;
    using UnityEngine;
    using Zenject;

    public class AnalyticServicesInstaller : Installer<AnalyticServicesInstaller>
    {
        public override void InstallBindings()
        {
            this.Container.Bind<AnalyticConfig>().FromResolveGetter<GDKConfig>(config => config.GetGameConfig<AnalyticConfig>()).AsCached();
            this.Container.Bind<IAnalyticServices>().To<AnalyticServices>().AsCached();
            this.Container.Bind<DeviceInfo>().AsCached();
            this.Container.Bind<SessionController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            this.Container.BindAllDerivedTypes<BaseTracker>(true);
            this.Container.Bind<AnalyticsEventCustomizationConfig>().AsCached();
            this.Container.DeclareSignal<EventTrackedSignal>();
            this.Container.DeclareSignal<SetUserIdSignal>();
            this.Container.DeclareSignal<AdRevenueSignal>();
            var unScaleInGameTimerManager = new GameObject("UnScaleInGameTimerManager").AddComponent<UnScaleInGameStopWatchManager>();
            Object.DontDestroyOnLoad(unScaleInGameTimerManager);
            this.Container.Bind<UnScaleInGameStopWatchManager>().FromInstance(unScaleInGameTimerManager).AsSingle();
        }
    }
}