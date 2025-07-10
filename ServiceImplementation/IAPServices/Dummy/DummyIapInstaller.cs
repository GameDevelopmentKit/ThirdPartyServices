namespace ServiceImplementation.IAPServices.Dummy
{
    using ServiceImplementation.IAPServices.Common;
    using ServiceImplementation.IAPServices.Iap5Below;

    public class DummyIapInstaller : BaseIapInstaller<DummyIapInstaller>
    {
        public override void InstallBindings()
        {
            base.InstallBindings();
            this.Container.BindInterfacesAndSelfTo<DummyIapServices>().AsCached();
        }
    }
}