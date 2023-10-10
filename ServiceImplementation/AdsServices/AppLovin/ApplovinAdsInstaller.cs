namespace ServiceImplementation.AdsServices.AppLovin
{
    using Zenject;

    public class ApplovinAdsInstaller : Installer<ApplovinAdsInstaller>
    {
        public override void InstallBindings()
        {
#if THEONE_APS_ENABLE
            this.Container.BindInterfacesAndSelfTo<AmazonApplovinAdsWrapper>().AsCached();
#else
            this.Container.BindInterfacesAndSelfTo<AppLovinAdsWrapper>().AsCached();
#endif
        }
    }
}