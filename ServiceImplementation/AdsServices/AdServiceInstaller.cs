namespace ServiceImplementation.AdsServices
{
    using System.Collections.Generic;
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using GameFoundation.Scripts.Utilities.Extension;
    using ServiceImplementation.AdsServices.AdRevenueTracker;
    using ServiceImplementation.AdsServices.EasyMobile;
    using ServiceImplementation.AdsServices.FacebookInstant;
    using ServiceImplementation.AdsServices.Signal;
    using UnityEngine;
    using Zenject;

    public class AdServiceInstaller : Installer<AdServiceInstaller>
    {
        [SerializeField] private GameObject facebookAdWrapperGameObject;
        public override void InstallBindings()
        {
            //config
            this.Container.Bind<AdServicesConfig>().AsCached().NonLazy();
#if EASY_MOBILE_PRO && (!UNITY_EDITOR || (UNITY_EDITOR && !EM_IRONSOURCE))
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
#endif

#if EM_IRONSOURCE
            this.Container.BindInterfacesAndSelfTo<IronSourceWrapper>().AsCached();
#endif

#if FB_INSTANT
            this.Container.Bind<FacebookAdsWrapper>().FromNewComponentOnNewPrefab(this.facebookAdWrapperGameObject).AsCached().NonLazy();
#endif
            this.Container.BindAllTypeDriveFrom<IAdRevenueTracker>();

            #region Ads signal

            this.Container.DeclareSignal<BannerAdPresentedSignal>();
            this.Container.DeclareSignal<BannerAdDismissedSignal>();
            this.Container.DeclareSignal<BannerAdLoadedSignal>();
            this.Container.DeclareSignal<BannerAdLoadFailedSignal>();
            this.Container.DeclareSignal<BannerAdClickedSignal>();

            this.Container.DeclareSignal<MRecAdLoadedSignal>();
            this.Container.DeclareSignal<MRecAdLoadFailedSignal>();
            this.Container.DeclareSignal<MRecAdClickedSignal>();
            this.Container.DeclareSignal<MRecAdDisplayedSignal>();
            this.Container.DeclareSignal<MRecAdDismissedSignal>();

            this.Container.DeclareSignal<InterstitialAdDownloadedSignal>();
            this.Container.DeclareSignal<InterstitialAdLoadFailedSignal>();
            this.Container.DeclareSignal<InterstitialAdClickedSignal>();
            this.Container.DeclareSignal<InterstitialAdDisplayedSignal>();
            this.Container.DeclareSignal<InterstitialAdClosedSignal>();

            this.Container.DeclareSignal<RewardedInterstitialAdCompletedSignal>();
            this.Container.DeclareSignal<RewardInterstitialAdSkippedSignal>();
            this.Container.DeclareSignal<RewardedAdLoadedSignal>();
            this.Container.DeclareSignal<RewardedAdLoadFailedSignal>();
            this.Container.DeclareSignal<RewardedAdLoadClickedSignal>();
            this.Container.DeclareSignal<RewardedAdDisplayedSignal>();
            this.Container.DeclareSignal<RewardedAdCompletedSignal>();
            this.Container.DeclareSignal<RewardedSkippedSignal>();

            this.Container.DeclareSignal<AppOpenFullScreenContentOpenedSignal>();
            this.Container.DeclareSignal<AppOpenFullScreenContentClosedSignal>();

            this.Container.DeclareSignal<AppStateChangeSignal>();

            #endregion
        }
    }
}