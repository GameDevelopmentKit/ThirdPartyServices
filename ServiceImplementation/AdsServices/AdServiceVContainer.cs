#if GDK_VCONTAINER
#nullable enable
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
    using VContainer;
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

    public static class AdServiceVContainer
    {
        public static void RegisterAdService(this IContainerBuilder builder)
        {
            //config
            builder.Register<AdServicesConfig>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<MiscConfig>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

            #if ADMOB_NATIVE_ADS && IMMERSIVE_ADS
            builder.RegisterComponentOnNewGameObject<PubScaleManager>(Lifetime.Singleton);
            builder.Register<PubScaleWrapper>(Lifetime.Singleton).AsImplementedInterfaces();
            #endif
            #if APPLOVIN
            #if APS_ENABLE && !UNITY_EDITOR
            builder.Register<AmazonApplovinAdsWrapper>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            #else
            builder.Register<AppLovinAdsWrapper>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            #endif
            #elif IRONSOURCE && !UNITY_EDITOR
            builder.Register<IronSourceWrapper>(Lifetime.Singleton).AsImplementedInterfaces();
            #elif YANDEX && !UNITY_EDITOR
            builder.Register<YandexAdsWrapper>(Lifetime.Singleton).AsImplementedInterfaces();
            #elif ADMOB
            builder.Register<AdMobAdService>(Lifetime.Singleton).AsImplementedInterfaces();
            #else
            builder.Register<DummyAdServiceIml>(Lifetime.Singleton).AsImplementedInterfaces();
            #endif

            #if ADMOB
            builder.Register<AdMobWrapper>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            if (!builder.Exists(typeof(IBackFillAdsService), true))
            {
                builder.Register<AdMobAdService>(Lifetime.Singleton).As(typeof(GameFoundation.DI.IInitializable), typeof(ICollapsibleBannerAd), typeof(IAdLoadService), typeof(IBackFillAdsService));
            }
            #else
            builder.Register<DummyCollapsibleBannerAdAdService>(Lifetime.Singleton).AsImplementedInterfaces();
            #if !APPLOVIN
            builder.Register<DummyAOAAdServiceIml>(Lifetime.Singleton).AsImplementedInterfaces();
            #endif
            builder.Register<DummyIBackFillService>(Lifetime.Singleton).AsImplementedInterfaces();
            #endif

            builder.Register<PreloadAdService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            typeof(IAdRevenueTracker).GetDerivedTypes().ForEach(type => builder.Register(type, Lifetime.Singleton));

            builder.Register<AppTrackingServices>(Lifetime.Singleton).AsImplementedInterfaces();
            #if ADMOB
            builder.Register<UmpConsentInformation>(Lifetime.Singleton).AsImplementedInterfaces();
            #else
            builder.Register<DummyConsentInformation>(Lifetime.Singleton).AsImplementedInterfaces();
            #endif

            #region Ads signal

            builder.DeclareSignal<BannerAdPresentedSignal>();
            builder.DeclareSignal<BannerAdDismissedSignal>();
            builder.DeclareSignal<BannerAdLoadedSignal>();
            builder.DeclareSignal<BannerAdLoadFailedSignal>();
            builder.DeclareSignal<BannerAdClickedSignal>();

            builder.DeclareSignal<CollapsibleBannerAdPresentedSignal>();
            builder.DeclareSignal<CollapsibleBannerAdDismissedSignal>();
            builder.DeclareSignal<CollapsibleBannerAdLoadedSignal>();
            builder.DeclareSignal<CollapsibleBannerAdLoadFailedSignal>();
            builder.DeclareSignal<CollapsibleBannerAdClickedSignal>();

            builder.DeclareSignal<MRecAdLoadedSignal>();
            builder.DeclareSignal<MRecAdLoadFailedSignal>();
            builder.DeclareSignal<MRecAdClickedSignal>();
            builder.DeclareSignal<MRecAdDisplayedSignal>();
            builder.DeclareSignal<MRecAdDismissedSignal>();

            builder.DeclareSignal<InterstitialAdLoadedSignal>();
            builder.DeclareSignal<InterstitialAdLoadFailedSignal>();
            builder.DeclareSignal<InterstitialAdClickedSignal>();
            builder.DeclareSignal<InterstitialAdDisplayedFailedSignal>();
            builder.DeclareSignal<InterstitialAdDisplayedSignal>();
            builder.DeclareSignal<InterstitialAdClosedSignal>();
            builder.DeclareSignal<InterstitialAdCalledSignal>();
            builder.DeclareSignal<InterstitialAdEligibleSignal>();

            builder.DeclareSignal<RewardedInterstitialAdCompletedSignal>();
            builder.DeclareSignal<RewardInterstitialAdSkippedSignal>();
            builder.DeclareSignal<RewardInterstitialAdCalledSignal>();
            builder.DeclareSignal<RewardInterstitialAdClosedSignal>();
            builder.DeclareSignal<RewardedAdLoadedSignal>();
            builder.DeclareSignal<RewardedAdLoadFailedSignal>();
            builder.DeclareSignal<RewardedAdClickedSignal>();
            builder.DeclareSignal<RewardedAdDisplayedSignal>();
            builder.DeclareSignal<RewardedAdCompletedSignal>();
            builder.DeclareSignal<RewardedSkippedSignal>();
            builder.DeclareSignal<RewardedAdEligibleSignal>();
            builder.DeclareSignal<RewardedAdCalledSignal>();
            builder.DeclareSignal<RewardedAdOfferSignal>();
            builder.DeclareSignal<RewardedAdClosedSignal>();
            builder.DeclareSignal<RewardedAdShowFailedSignal>();

            builder.DeclareSignal<AppOpenFullScreenContentOpenedSignal>();
            builder.DeclareSignal<AppOpenFullScreenContentFailedSignal>();
            builder.DeclareSignal<AppOpenFullScreenContentClosedSignal>();
            builder.DeclareSignal<AppOpenLoadedSignal>();
            builder.DeclareSignal<AppOpenLoadFailedSignal>();
            builder.DeclareSignal<AppOpenEligibleSignal>();
            builder.DeclareSignal<AppOpenCalledSignal>();
            builder.DeclareSignal<AppOpenClickedSignal>();

            builder.DeclareSignal<AppStateChangeSignal>();

            #endregion
        }
    }
}
#endif