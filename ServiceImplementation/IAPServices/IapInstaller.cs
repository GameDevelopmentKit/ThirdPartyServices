namespace ServiceImplementation.IAPServices
{
    using GameFoundation.Scripts.Utilities.LogService;
    using UnityEngine;
    using Zenject;

    public class IapInstaller : Installer<IapInstaller>
    {
        public override void InstallBindings()
        {
#if IAP
            this.Container.Bind<IIapServices>().To<UnityIapServices>().AsCached().NonLazy();
            this.Container.Resolve<ILogService>().LogWithColor("IAP Enable, don't forget to call IIapServices.InitIapServices in your game,ignore if already done!!", Color.red);
#endif

            this.Container.DeclareSignal<OnRestorePurchaseCompleteSignal>();
        }
    }
}