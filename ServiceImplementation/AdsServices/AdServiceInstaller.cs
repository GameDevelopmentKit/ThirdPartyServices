#if GDK_ZENJECT
namespace ServiceImplementation.AdsServices
{
    using Core.AdsServices;
    using Core.AdsServices.CollapsibleBanner;
    using Core.AdsServices.Signals;
    using GameFoundation.Scripts.Utilities.Extension;
    using GameFoundation.Signals;
    using ServiceImplementation.AdsServices.AdRevenueTracker;
    using ServiceImplementation.AdsServices.ConsentInformation;
    using ServiceImplementation.AdsServices.EasyMobile;
    using ServiceImplementation.AdsServices.PreloadService;
    using ServiceImplementation.AdsServices.Signal;
    using ServiceImplementation.Configs.Ads;
    using Zenject;
#if ADMOB_NATIVE_ADS && IMMERSIVE_ADS
    using global::PubScale.SdkOne;
    using ServiceImplementation.AdsServices.PubScale;
#endif
#if APPLOVIN
    using ServiceImplementation.AdsServices.AppLovin;
#endif
#if ADMOB
    using ServiceImplementation.AdsServices.AdMob;
#endif
#if YANDEX
    using ServiceImplementation.AdsServices.Yandex;
#endif

    public class AdServiceInstaller : Installer<AdServiceInstaller>
    {
        public override void InstallBindings()
        {
            //config
            this.Container.BindInterfacesAndSelfTo<AdServicesConfig>().AsCached();
            this.Container.BindInterfacesAndSelfTo<MiscConfig>().AsCached();

#if ADMOB_NATIVE_ADS && IMMERSIVE_ADS
            this.Container.Bind<PubScaleManager>().FromNewComponentOnNewGameObject().WithGameObjectName("PubScaleManager").AsSingle().NonLazy();
            this.Container.BindInterfacesTo<PubScaleWrapper>().AsCached();
#endif
#if APPLOVIN
            ApplovinAdsInstaller.Install(this.Container);
#endif
#if IRONSOURCE && !UNITY_EDITOR
            this.Container.BindInterfacesTo<IronSourceWrapper>().AsCached();
#endif
#if YANDEX && !UNITY_EDITOR
            this.Container.BindInterfacesTo<YandexAdsWrapper>().AsCached();
#endif
#if ADMOB
            this.Container.BindInterfacesTo<AdMobAdService>().AsCached();
            this.Container.BindInterfacesTo<AdMobWrapper>().AsCached().NonLazy();
#endif
#if !APPLOVIN && (!IRONSOURCE || UNITY_EDITOR) && (!YANDEX || UNITY_EDITOR) && !ADMOB
            this.Container.BindInterfacesTo<DummyAdServiceIml>().AsCached();
#endif

#if !ADMOB
            this.Container.Bind<ICollapsibleBannerAd>().To<DummyCollapsibleBannerAdAdService>().AsCached();
            #if !APPLOVIN
            this.Container.Bind<IAOAAdService>().To<DummyAOAAdServiceIml>().AsCached();
            #endif
#endif

            this.Container.BindInterfacesAndSelfTo<PreloadAdService>().AsCached().NonLazy();
            this.Container.BindAllTypeDriveFrom<IAdRevenueTracker>();

            ConsentInformationInstaller.Install(this.Container);

            #region Ads signal

            this.Container.DeclareSignal<BannerAdPresentedSignal>();
            this.Container.DeclareSignal<BannerAdDismissedSignal>();
            this.Container.DeclareSignal<BannerAdLoadedSignal>();
            this.Container.DeclareSignal<BannerAdLoadFailedSignal>();
            this.Container.DeclareSignal<BannerAdClickedSignal>();

            this.Container.DeclareSignal<CollapsibleBannerAdPresentedSignal>();
            this.Container.DeclareSignal<CollapsibleBannerAdDismissedSignal>();
            this.Container.DeclareSignal<CollapsibleBannerAdLoadedSignal>();
            this.Container.DeclareSignal<CollapsibleBannerAdLoadFailedSignal>();
            this.Container.DeclareSignal<CollapsibleBannerAdClickedSignal>();

            this.Container.DeclareSignal<MRecAdLoadedSignal>();
            this.Container.DeclareSignal<MRecAdLoadFailedSignal>();
            this.Container.DeclareSignal<MRecAdClickedSignal>();
            this.Container.DeclareSignal<MRecAdDisplayedSignal>();
            this.Container.DeclareSignal<MRecAdDismissedSignal>();

            this.Container.DeclareSignal<InterstitialAdLoadedSignal>();
            this.Container.DeclareSignal<InterstitialAdLoadFailedSignal>();
            this.Container.DeclareSignal<InterstitialAdClickedSignal>();
            this.Container.DeclareSignal<InterstitialAdDisplayedFailedSignal>();
            this.Container.DeclareSignal<InterstitialAdDisplayedSignal>();
            this.Container.DeclareSignal<InterstitialAdClosedSignal>();
            this.Container.DeclareSignal<InterstitialAdCalledSignal>();
            this.Container.DeclareSignal<InterstitialAdEligibleSignal>();

            this.Container.DeclareSignal<RewardedInterstitialAdCompletedSignal>();
            this.Container.DeclareSignal<RewardInterstitialAdSkippedSignal>();
            this.Container.DeclareSignal<RewardInterstitialAdCalledSignal>();
            this.Container.DeclareSignal<RewardInterstitialAdClosedSignal>();
            this.Container.DeclareSignal<RewardedAdLoadedSignal>();
            this.Container.DeclareSignal<RewardedAdLoadFailedSignal>();
            this.Container.DeclareSignal<RewardedAdClickedSignal>();
            this.Container.DeclareSignal<RewardedAdDisplayedSignal>();
            this.Container.DeclareSignal<RewardedAdCompletedSignal>();
            this.Container.DeclareSignal<RewardedSkippedSignal>();
            this.Container.DeclareSignal<RewardedAdEligibleSignal>();
            this.Container.DeclareSignal<RewardedAdCalledSignal>();
            this.Container.DeclareSignal<RewardedAdOfferSignal>();
            this.Container.DeclareSignal<RewardedAdClosedSignal>();
            this.Container.DeclareSignal<RewardedAdShowFailedSignal>();

            this.Container.DeclareSignal<AppOpenFullScreenContentOpenedSignal>();
            this.Container.DeclareSignal<AppOpenFullScreenContentFailedSignal>();
            this.Container.DeclareSignal<AppOpenFullScreenContentClosedSignal>();
            this.Container.DeclareSignal<AppOpenLoadedSignal>();
            this.Container.DeclareSignal<AppOpenLoadFailedSignal>();
            this.Container.DeclareSignal<AppOpenEligibleSignal>();
            this.Container.DeclareSignal<AppOpenCalledSignal>();
            this.Container.DeclareSignal<AppOpenClickedSignal>();

            // This signal is used to all type of ad request
            this.Container.DeclareSignal<AdRequestSignal>();
            
            this.Container.DeclareSignal<AppStateChangeSignal>();

            #endregion
        }
    }
}
#endif