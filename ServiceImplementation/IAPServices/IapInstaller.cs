#if GDK_ZENJECT
namespace ServiceImplementation.IAPServices
{
    using GameFoundation.Scripts.Utilities.LogService;
    using GameFoundation.Signals;
    using ServiceImplementation.IAPServices.Signals;
    using UnityEngine;
    using UnityEngine.Scripting;
    using Zenject;

    [Preserve]
    public class IapInstaller : Installer<IapInstaller>
    {
        public override void InstallBindings()
        {
            #if THEONE_IAP
            this.Container.Bind<IIapServices>()
                .To<UnityIapServices>()
                .AsCached()
                .OnInstantiated((ctx, _) =>
                {
                    ctx.Container.Resolve<ILogService>()
                        .LogWithColor("IAP Enable, don't forget to call IIapServices.InitIapServices in your game,ignore if already done!!", Color.red);
                })
                .NonLazy();
            #else
            this.Container.Bind<IIapServices>().To<DummyIapServices>().AsCached().NonLazy();
            #endif

            this.Container.DeclareSignal<OnRestorePurchaseCompleteSignal>();
            this.Container.DeclareSignal<OnStartDoingIAPSignal>();
            this.Container.DeclareSignal<OnIAPPurchaseSuccessSignal>();
            this.Container.DeclareSignal<OnIAPPurchaseFailedSignal>();
        }
    }
}
#endif