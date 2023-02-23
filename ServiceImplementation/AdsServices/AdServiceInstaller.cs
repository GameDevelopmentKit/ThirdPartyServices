namespace ServiceImplementation.AdsServices
{
    using Core.AdsServices;
    using ServiceImplementation.AdsServices.EasyMobile;
    using Zenject;

    public class AdServiceInstaller : Installer<AdServiceInstaller>
    {
        public override void InstallBindings()
        {
#if EASY_MOBILE_PRO
            this.Container.Bind<IAdServices>().To<EasyMobileAdIml>().AsCached();
#else
            this.Container.Bind<IAdsServices>().To<DummyAdServiceIml>().AsCached();
#endif
#if EM_ADMOB
            this.Container.Bind<IAOAAdService>().To<AdModWrapper>().AsCached();
#else
            this.Container.Bind<IAOAAdService>().To<DummyAOAAdServiceIml>().AsCached();
#endif
        }
    }
}