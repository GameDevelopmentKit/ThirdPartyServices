namespace ServiceImplementation.AdsServices.AppLovin
{
    using Zenject;

    public class ApplovinAdsInstaller : Installer<ApplovinAdsInstaller>
    {
        public override void InstallBindings()
        {
#if APS_ENABLE && APPLOVIN
            this.Container.BindInterfacesAndSelfTo<AmazonApplovinAdsWrapper>().AsCached();
#else
            this.Container.BindInterfacesAndSelfTo<AppLovinAdsWrapper>().AsCached();
#endif
        }
    }
}