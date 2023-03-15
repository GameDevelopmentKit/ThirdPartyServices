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
            this.Container.BindInterfacesTo<DummyAdServiceIml>().AsCached();
#endif
#if EM_ADMOB
            this.Container.BindInterfacesAndSelfTo<AdModWrapper>().AsCached();
#else
            this.Container.Bind<IAOAAdService>().To<DummyAOAAdServiceIml>().AsCached();
#endif

#if EM_APPLOVIN
            this.Container.BindInterfacesAndSelfTo<MaxSDKWrapper>().AsCached();
#elif EM_IRONSOURCE
            this.Container.BindInterfacesAndSelfTo<IronSourceWrapper>().AsCached();
#else
            this.Container.Bind<IMRECAdService>().To<DummyMRECAdService>().AsCached();
#endif
        }
    }
}