namespace ServiceImplementation.IAPServices
{
    using System.Collections.Generic;
    using GameFoundation.Scripts.Utilities.LogService;
    using Zenject;

    public class UnityIapInstaller : Installer<UnityIapInstaller>
    {
        public override void InstallBindings()
        {
#if !ENABLE_IAP
            this.Container.Bind<IUnityIapServices>().To<DummyUnityIapServices>().AsCached().NonLazy();
            this.Container.Bind<IUnityRemoveAdsServices>().To<DummyUnityRemoveAdsIapServices>().AsCached().NonLazy();
#else
            this.Container.Bind<IUnityIapServices>().To<UnityIapServices>().AsCached().NonLazy();
            this.Container.Bind<IUnityRemoveAdsServices>().To<UnityRemoveAdsIapServices>().AsCached().NonLazy();
            this.Container.Resolve<ILogService>().Error("IAP Enable, don't forget to call IUnityIapServices.InitIapServices in your game,ignore if already done!!");
#endif
            this.Container.Bind<RemoveAdData>().FromInstance(new RemoveAdData()
            {
                listIdRemoveAds = new List<string>() { "RemoveAds" }
            }).AsCached().NonLazy();

            this.Container.DeclareSignal<UnityIAPOnPurchaseCompleteSignal>();
        }
    }
}