namespace ServiceImplementation.AdsServices
{
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using ServiceImplementation.AdsServices.EasyMobile;
    using Zenject;

    public class AdServiceInstaller : Installer<AdServiceInstaller>
    {
        public override void InstallBindings()
        {
            this.Container.DeclareSignal<ShowInterstitialAdSignal>();
#if EASY_MOBILE_PRO
            this.Container.BindInterfacesTo<EasyMobileAdIml>().AsCached();
#else
            this.Container.Bind<IAdsServices>().To<DummyAdServiceIml>().AsCached();
#endif
#if EM_ADMOB
            this.Container.BindInterfacesAndSelfTo<AdModWrapper>().AsCached();
#else
            this.Container.Bind<IAOAAdService>().To<DummyAOAAdServiceIml>().AsCached();
#endif
        }
    }
}