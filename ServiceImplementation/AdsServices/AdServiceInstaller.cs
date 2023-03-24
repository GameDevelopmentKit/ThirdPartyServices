namespace ServiceImplementation.AdsServices
{
    using System.Collections.Generic;
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using GameFoundation.Scripts.Utilities.Extension;
    using ServiceImplementation.AdsServices.AdRevenueTracker;
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
            this.Container.BindInterfacesAndSelfTo<AdModWrapper>().AsCached().NonLazy();
            this.Container.Bind<AdmobAOAMono>().FromNewComponentOnNewGameObject().AsCached().NonLazy();
#else
            this.Container.Bind<IAOAAdService>().To<DummyAOAAdServiceIml>().AsCached();
#endif

#if EM_APPLOVIN
            this.Container.BindInterfacesAndSelfTo<MaxSDKWrapper>().AsCached();
            this.Container.Bind<Dictionary<AdViewPosition, string>>().FromInstance(new Dictionary<AdViewPosition, string>()).WhenInjectedInto<MaxSDKWrapper>();
#elif EM_IRONSOURCE
            this.Container.BindInterfacesAndSelfTo<IronSourceWrapper>().AsCached();
#else
            this.Container.Bind<IMRECAdService>().To<DummyMRECAdService>().AsCached();
#endif
            this.Container.BindAllTypeDriveFrom<IAdRevenueTracker>();

            #region Ads signal

            this.Container.DeclareSignal<BannerAdLoadedSignal>();
            this.Container.DeclareSignal<BannerAdLoadFailedSignal>();
            this.Container.DeclareSignal<BannerAdClickedSignal>();
            this.Container.DeclareSignal<InterstitialAdDownloadedSignal>();
            this.Container.DeclareSignal<InterstitialAdLoadFailedSignal>();
            this.Container.DeclareSignal<InterstitialAdClickedSignal>();
            this.Container.DeclareSignal<InterstitialAdDisplayedSignal>();
            this.Container.DeclareSignal<RewardedAdLoadedSignal>();
            this.Container.DeclareSignal<RewardedAdLoadFailedSignal>();
            this.Container.DeclareSignal<RewardedAdLoadClickedSignal>();
            this.Container.DeclareSignal<RewardedAdDisplayedSignal>();

            #endregion
        }
    }
}