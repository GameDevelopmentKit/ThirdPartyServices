#if GDK_ZENJECT
namespace ServiceImplementation.AdsServices.AppLovin
{
    using Zenject;

    public class ApplovinAdsInstaller : Installer<ApplovinAdsInstaller>
    {
        public override void InstallBindings()
        {
#if APS_ENABLE && APPLOVIN && !UNITY_EDITOR
            this.Container.BindInterfacesAndSelfTo<AmazonApplovinAdsWrapper>().AsCached();
#elif APPLOVIN
            this.Container.BindInterfacesAndSelfTo<AppLovinAdsWrapper>().AsCached();
#endif
        }
    }
}
#endif