namespace ServiceImplementation.AdsServices
{
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using Zenject;

    public class AdServiceInstaller : Installer<AdServiceInstaller>
    {
        public override void InstallBindings()
        {
#if !FB_INSTANT_PRODUCTION || UNITY_EDITOR
            this.Container.BindInterfacesTo<DummyAdServiceIml>().AsCached();
#endif

            #region Ads signal

            this.Container.DeclareSignal<BannerAdPresentedSignal>();
            this.Container.DeclareSignal<BannerAdDismissedSignal>();
            this.Container.DeclareSignal<BannerAdLoadedSignal>();
            this.Container.DeclareSignal<BannerAdLoadFailedSignal>();
            this.Container.DeclareSignal<BannerAdClickedSignal>();

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

            #endregion
        }
    }
}