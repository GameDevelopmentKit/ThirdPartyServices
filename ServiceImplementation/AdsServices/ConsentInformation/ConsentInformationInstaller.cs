namespace ServiceImplementation.AdsServices.ConsentInformation
{
    using Zenject;

    public class ConsentInformationInstaller : Installer<ConsentInformationInstaller>
    {
        public override void InstallBindings()
        {
            this.Container.BindInterfacesAndSelfTo<AppTrackingServices>().AsCached().NonLazy();
#if ADMOB
            this.Container.BindInterfacesAndSelfTo<UmpConsentInformation>().AsCached().NonLazy();
#else
            this.Container.BindInterfacesAndSelfTo<DummyConsentInformation>().AsCached().NonLazy();
#endif
        }
    }
}