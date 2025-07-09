
namespace ServiceImplementation.IAPServices.IAP5orNewer
{
    using ServiceImplementation.IAPServices.Iap5Below;
    using ServiceImplementation.IAPServices.Signals;
    using GameFoundation.Scripts.Utilities.LogService;
    using UnityEngine;
    using Zenject;

    public class Iap5orNewerInstaller : Installer<Iap5orNewerInstaller>
    {
        public override void InstallBindings()
        {
#if IAP_5_OR_NEWER
            this.Container.Bind(typeof(IIapServices), typeof(IInitializable))
                .To<Iap5OrNewerServices>()
                .AsCached()
                .OnInstantiated((ctx, _) =>
                {
                    ctx.Container.Resolve<ILogService>()
                        .LogWithColor("IAP Enable, don't forget to call IIapServices.InitIapServices in your game,ignore if already done!!", Color.red);
                })
                .NonLazy();

            this.Container.Bind<IapLogWrapped>().AsCached();
#endif

            this.Container.DeclareSignal<OnRestorePurchaseCompleteSignal>();
            this.Container.DeclareSignal<OnStartDoingIAPSignal>();
            this.Container.DeclareSignal<OnIAPPurchaseSuccessSignal>();
            this.Container.DeclareSignal<OnIAPPurchaseFailedSignal>();
        }
    }
}
