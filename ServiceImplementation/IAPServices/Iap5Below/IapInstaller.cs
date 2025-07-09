#if IAP
namespace ServiceImplementation.IAPServices.Iap5Below
{
    using GameFoundation.Scripts.Utilities.LogService;
    using ServiceImplementation.IAPServices.Signals;
    using UnityEngine;
    using Zenject;

    public class IapInstaller : Installer<IapInstaller>
    {
        public override void InstallBindings()
        {
            this.Container.Bind<IIapServices>()
                .To<UnityIapServices>()
                .AsCached()
                .OnInstantiated((ctx, _) =>
                {
                    ctx.Container.Resolve<ILogService>()
                        .LogWithColor("IAP 4 Enable, don't forget to call IIapServices.InitIapServices in your game,ignore if already done!!", Color.red);
                })
                .NonLazy();
            this.Container.DeclareSignal<OnRestorePurchaseCompleteSignal>();
            this.Container.DeclareSignal<OnStartDoingIAPSignal>();
            this.Container.DeclareSignal<OnIAPPurchaseSuccessSignal>();
            this.Container.DeclareSignal<OnIAPPurchaseFailedSignal>();
        }
    }
}

#endif