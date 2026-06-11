namespace ServiceImplementation.AdsServices.AppLovin
{
    using Zenject;

    public class ApplovinAdsInstaller : Installer<ApplovinAdsInstaller>
    {
        public override void InstallBindings()
        {
#if APS_ENABLE && APPLOVIN && !UNITY_EDITOR
            this.Container.BindInitializableExecutionOrder<AmazonApplovinAdsWrapper>(-2000);
            this.Container.BindInterfacesAndSelfTo<AmazonApplovinAdsWrapper>().AsCached();
#elif APPLOVIN
            this.Container.BindInitializableExecutionOrder<AppLovinAdsWrapper>(-2000);
            this.Container.BindInterfacesAndSelfTo<AppLovinAdsWrapper>().AsCached();
#endif
        }
    }
}