namespace ServiceImplementation.IAPServices.IAP5orNewer
{
    using ServiceImplementation.IAPServices.Iap5Below;
    using GameFoundation.Scripts.Utilities.LogService;
    using ServiceImplementation.IAPServices.Common;
    using UnityEngine;
    using Zenject;

    public class Iap5orNewerInstaller : BaseIapInstaller<Iap5orNewerInstaller>
    {
        public override void InstallBindings()
        {
            base.InstallBindings();
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
        }
    }
}