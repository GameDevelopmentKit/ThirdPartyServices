namespace ServiceImplementation.IAPServices
{
    using GameFoundation.Scripts.Utilities.Extension;
    using Zenject;

    public class IapInstaller : Installer<IapInstaller>
    {
        public override void InstallBindings()
        {
            if (!this.Container.HasBinding<UnityIapReceiptValidationKeys>())
            {
                this.Container.BindInstance(new UnityIapReceiptValidationKeys());
            }

            this.Container.BindInterfacesAndSelfToAllTypeDriveFrom<IIapReceiptValidator>();
            this.Container.Bind<IIapReceiptValidationService>().To<IapReceiptValidationService>().AsCached();

#if UNITY_IAP
            this.Container.BindInterfacesTo<UnityIAPHandler>().AsCached().NonLazy();
#else
            this.Container.Bind<IIapServices>().To<DummyIapServices>().AsCached().NonLazy();
#endif
        }
    }
}
