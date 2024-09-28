#if GDK_VCONTAINER
#nullable enable
namespace Core.AnalyticServices
{
    using Core.AnalyticServices.Data;
    using Core.AnalyticServices.Signal;
    using Core.AnalyticServices.Tools;
    using GameFoundation.Signals;
    using Models;
    using VContainer;
    using VContainer.Unity;

    public static class AnalyticServiceVContainer
    {
        public static void RegisterAnalyticService(this IContainerBuilder builder)
        {
            builder.Register(container => container.Resolve<GDKConfig>().GetGameConfig<AnalyticConfig>(), Lifetime.Singleton);
            builder.Register<AnalyticServices>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<DeviceInfo>(Lifetime.Singleton);
            builder.RegisterComponentOnNewGameObject<SessionController>(Lifetime.Singleton);
            builder.RegisterBuildCallback(container => container.Resolve<SessionController>().Construct(container.Resolve<IAnalyticServices>(), container.Resolve<DeviceInfo>()));
            builder.RegisterComponentOnNewGameObject<UnScaleInGameStopWatchManager>(Lifetime.Singleton);

            builder.DeclareSignal<EventTrackedSignal>();
            builder.DeclareSignal<SetUserIdSignal>();
            builder.DeclareSignal<AdRevenueSignal>();
        }
    }
}
#endif