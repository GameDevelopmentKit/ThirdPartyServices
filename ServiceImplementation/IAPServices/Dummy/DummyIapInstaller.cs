namespace ServiceImplementation.IAPServices.Dummy
{
    using ServiceImplementation.IAPServices.Iap5Below;
    using Zenject;

    public class DummyIapInstaller : Installer<DummyIapInstaller>
    {
        public override void InstallBindings() { this.Container.BindInterfacesAndSelfTo<DummyIapServices>(); }
    }
}