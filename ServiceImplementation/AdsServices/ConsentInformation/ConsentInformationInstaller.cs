namespace ServiceImplementation.AdsServices.ConsentInformation
{
    using Zenject;

    public class ConsentInformationInstaller : Installer<ConsentInformationInstaller>
    {
        public override void InstallBindings()
        {
#if ADMOB && ENABLE_UMP
            this.Container.BindInterfacesAndSelfTo<UmpConsentInformation>().AsCached().NonLazy();
#else
            this.Container.BindInterfacesAndSelfTo<DummyConsentInformation>().AsCached().NonLazy();
#endif
        }

        private void WaitToRequest()
        {
        }
    }
}