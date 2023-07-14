namespace ServiceImplementation.IAPServices
{
    using System.Collections.Generic;
    using GameFoundation.Scripts.Utilities.LogService;
    using UnityEngine;
    using Zenject;

    public class IapInstaller : Installer<IapInstaller>
    {
        public override void InstallBindings()
        {
#if !IAP
            this.Container.Bind<IUnityIapServices>().To<DummyUnityIapServices>().AsCached().NonLazy();
            this.Container.Bind<IUnityRemoveAdsServices>().To<DummyUnityRemoveAdsIapServices>().AsCached().NonLazy();
#else
            this.Container.Bind<IIapServices>().To<UnityIapServices>().AsCached().NonLazy();
            this.Container.Bind<IRemoveAdsServices>().To<RemoveAdsIapServices>().AsCached().NonLazy();
            this.Container.Resolve<ILogService>().LogWithColor("IAP Enable, don't forget to call IIapServices.InitIapServices in your game,ignore if already done!!", Color.red);
#endif
            this.Container.Bind<RemoveAdData>().FromInstance(new RemoveAdData()
            {
                listIdRemoveAds = new List<string>() { "RemoveAds" }
            }).AsCached().NonLazy();

            this.Container.DeclareSignal<OnRestorePurchaseCompleteSignal>();
        }
    }
}