namespace ServiceImplementation.IAPServices.Iap5Below
{
    using GameFoundation.Scripts.Utilities.LogService;
    using ServiceImplementation.IAPServices.Common;
    using UnityEngine;
    using Zenject;

    public class IapInstaller : BaseIapInstaller<IapInstaller>
    {
        public override void InstallBindings()
        {
            base.InstallBindings();
#if IAP
            this.Container.Bind<IIapServices>()
                .To<UnityIapServices>()
                .AsCached()
                .OnInstantiated((ctx, _) =>
                {
                    ctx.Container.Resolve<ILogService>()
                        .LogWithColor("IAP 4 Enable, don't forget to call IIapServices.InitIapServices in your game,ignore if already done!!", Color.red);
                })
                .NonLazy();
#endif
        }
    }
}