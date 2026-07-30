namespace ServiceImplementation.IAPServices
{
    using Zenject;

    public class IapInstaller : Installer<IapInstaller>
    {
        public override void InstallBindings()
        {
#if UNITY_IAP
            this.Container.BindInterfacesTo<UnityIAPHandler>().AsCached().NonLazy();
#else
            this.Container.Bind<IIapServices>().To<DummyIapServices>().AsCached().NonLazy();
#endif
        }
    }
}