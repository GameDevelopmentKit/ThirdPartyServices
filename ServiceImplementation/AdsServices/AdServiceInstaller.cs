namespace ServiceImplementation.AdsServices
{
    using System.Collections.Generic;
    using Core.AdsServices;
    using Core.AdsServices.CollapsibleBanner;
    using Core.AdsServices.Signals;
    using GameFoundation.Scripts.Utilities.Extension;
    using ServiceImplementation.AdsServices.AdRevenueTracker;
    using ServiceImplementation.AdsServices.ConsentInformation;
    using ServiceImplementation.AdsServices.EasyMobile;
    using ServiceImplementation.AdsServices.PreloadService;
    using ServiceImplementation.AdsServices.Signal;
    using ServiceImplementation.Configs.Ads;
    using Zenject;
#if APPLOVIN
    using ServiceImplementation.AdsServices.AppLovin;
#endif
#if ADMOB
    using ServiceImplementation.AdsServices.AdMob;
#endif

    public class AdServiceInstaller : Installer<AdServiceInstaller>
    {
        public override void InstallBindings()
        {
            //config
            this.Container.BindInterfacesAndSelfTo<AdServicesConfig>().AsCached();
            this.Container.BindInterfacesAndSelfTo<MiscConfig>().AsCached();

#if APPLOVIN
            ApplovinAdsInstaller.Install(this.Container);
            // this.Container.Bind<Dictionary<AdViewPosition, string>>().FromInstance(new Dictionary<AdViewPosition, string>()).WhenInjectedInto<AppLovinAdsWrapper>();
#elif IRONSOURCE && !UNITY_EDITOR
            this.Container.BindInterfacesTo<IronSourceWrapper>().AsCached();
#elif ADMOB
            this.Container.BindInterfacesTo<AdMobAdService>().AsCached();
#else
            this.Container.BindInterfacesTo<DummyAdServiceIml>().AsCached();
#endif

#if ADMOB
            this.Container.BindInterfacesAndSelfTo<AdMobWrapper>().AsCached().NonLazy();
            if (!this.Container.HasBinding<IBackFillAdsService>())
            {
                this.Container.Bind(typeof(IInitializable), typeof(ICollapsibleBannerAd), typeof(IAdLoadService), typeof(IBackFillAdsService)).To<AdMobAdService>().AsCached();
            }
#else
            this.Container.Bind<ICollapsibleBannerAd>().To<DummyCollapsibleBannerAdAdService>().AsCached();
            #if !APPLOVIN
            this.Container.Bind<IAOAAdService>().To<DummyAOAAdServiceIml>().AsCached();
            #endif
            this.Container.Bind<IBackFillAdsService>().To<DummyIBackFillService>().AsCached();
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

            this.Container.DeclareSignal<AppStateChangeSignal>();

            #endregion
        }
    }
}