namespace ServiceImplementation.AdsServices.EasyMobile
{
#if EASY_MOBILE_PRO
    using System;
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Utilities.LogService;
    using global::EasyMobile;
    using Zenject;
    using AdNetwork = Core.AdsServices.AdNetwork;
    using ConsentStatus = Core.AdsServices.ConsentStatus;
    using InterstitialAdNetwork = global::EasyMobile.InterstitialAdNetwork;
    using RewardedAdNetwork = global::EasyMobile.RewardedAdNetwork;
    using RewardedInterstitialAdNetwork = global::EasyMobile.RewardedInterstitialAdNetwork;

    public class EasyMobileAdIml : IAdServices, IInitializable, IDisposable
    {
        #region inject

        private readonly ILogService      logService;
        private readonly SignalBus        signalBus;
        private readonly AdServicesConfig adServicesConfig;

        #endregion

        private event Action RewardedAdCompletedOneTimeAction;
        private event Action RewardedInterstitialAdCompletedOneTimeAction;

        public EasyMobileAdIml(ILogService logService, SignalBus signalBus, AdServicesConfig adServicesConfig)
        {
            this.logService       = logService;
            this.signalBus        = signalBus;
            this.adServicesConfig = adServicesConfig;
        }

        public void Initialize()
        {
            Advertising.InterstitialAdCompleted += this.OnAdvertisingOnInterstitialAdCompleted;

            Advertising.RewardedAdCompleted += this.OnAdvertisingOnRewardedAdCompleted;
            Advertising.RewardedAdSkipped   += this.OnAdvertisingOnRewardedAdSkipped;

            Advertising.RewardedInterstitialAdCompleted += this.OnAdvertisingOnRewardedInterstitialAdCompleted;
            Advertising.RewardedInterstitialAdSkipped   += this.OnAdvertisingOnRewardedInterstitialAdSkipped;

            Advertising.AdsRemoved += this.OnAdRemoved;
        }

        public void Dispose()
        {
            Advertising.InterstitialAdCompleted -= this.OnAdvertisingOnInterstitialAdCompleted;

            Advertising.RewardedAdCompleted             -= this.OnAdvertisingOnRewardedAdCompleted;
            Advertising.RewardedAdSkipped               -= this.OnAdvertisingOnRewardedAdSkipped;
            Advertising.RewardedInterstitialAdCompleted -= this.OnAdvertisingOnRewardedInterstitialAdCompleted;
            Advertising.RewardedInterstitialAdSkipped   -= this.OnAdvertisingOnRewardedInterstitialAdSkipped;

            Advertising.AdsRemoved -= this.OnAdRemoved;
        }

        private async void OnAdRemoved() { await UniTask.SwitchToMainThread(); }

        private async void OnAdvertisingOnRewardedInterstitialAdSkipped(RewardedInterstitialAdNetwork network, AdPlacement place)
        {
            await UniTask.SwitchToMainThread();
            this.signalBus.Fire(new RewardInterstitialAdSkippedSignal(place.Name));
        }

        private async void OnAdvertisingOnRewardedInterstitialAdCompleted(RewardedInterstitialAdNetwork network, AdPlacement place)
        {
            await UniTask.SwitchToMainThread();
            this.RewardedInterstitialAdCompletedOneTimeAction?.Invoke();
            this.RewardedInterstitialAdCompletedOneTimeAction = null;
            this.signalBus.Fire(new RewardedInterstitialAdCompletedSignal(place.Name));
        }

        private async void OnAdvertisingOnRewardedAdSkipped(RewardedAdNetwork network, AdPlacement place)
        {
            await UniTask.SwitchToMainThread();
            this.signalBus.Fire(new RewardedSkippedSignal(place.Name));
        }

        private async void OnAdvertisingOnRewardedAdCompleted(RewardedAdNetwork network, AdPlacement place)
        {
            await UniTask.SwitchToMainThread();
            this.RewardedAdCompletedOneTimeAction?.Invoke();
            this.RewardedAdCompletedOneTimeAction = null;
            this.signalBus.Fire(new RewardedAdCompletedSignal(place.Name));
        }

        private async void OnAdvertisingOnInterstitialAdCompleted(InterstitialAdNetwork network, AdPlacement place)
        {
            await UniTask.SwitchToMainThread();
            this.InterstitialAdCompleted?.Invoke((Core.AdsServices.InterstitialAdNetwork)network, place.Name);
            this.signalBus.Fire(new InterstitialAdClosedSignal(place.Name));
        }

        #region Consent

        public void GrantDataPrivacyConsent() { Advertising.GrantDataPrivacyConsent(); }

        public void RevokeDataPrivacyConsent() { Advertising.RevokeDataPrivacyConsent(); }

        public void GrantDataPrivacyConsent(AdNetwork adNetwork) { Advertising.GrantDataPrivacyConsent((global::EasyMobile.AdNetwork)adNetwork); }

        public void RevokeDataPrivacyConsent(AdNetwork adNetwork) { Advertising.RevokeDataPrivacyConsent((global::EasyMobile.AdNetwork)adNetwork); }

        public ConsentStatus GetDataPrivacyConsent(AdNetwork adNetwork) { return (ConsentStatus)Advertising.GetDataPrivacyConsent((global::EasyMobile.AdNetwork)adNetwork); }

        #endregion

        #region Banner

        public void ShowBannerAd(BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom, int width = 320, int height = 50)
        {
            if (!this.adServicesConfig.EnableBannerAd)
            {
                return;
            }

            Advertising.ShowBannerAd((BannerAdPosition)bannerAdsPosition, new BannerAdSize(320, 50));
        }

        public void HideBannedAd() { Advertising.HideBannerAd(); }

        public void DestroyBannerAd() { Advertising.DestroyBannerAd(); }

        #endregion

        #region InterstitialAdCompleteHandler

        public event Action<Core.AdsServices.InterstitialAdNetwork, string> InterstitialAdCompleted;

        public bool IsInterstitialAdReady(string place) { return Advertising.IsInterstitialAdReady(AdPlacement.PlacementWithName(place)); }

        public void ShowInterstitialAd(string place)
        {
            if (!this.adServicesConfig.EnableInterstitialAd)
            {
                return;
            }

            Advertising.ShowInterstitialAd(AdPlacement.PlacementWithName(place));
        }

        #endregion

        public bool IsRewardedAdReady(string place) { return Advertising.IsRewardedAdReady(AdPlacement.PlacementWithName(place)); }

        public void ShowRewardedAd(string place)
        {
            if (!this.adServicesConfig.EnableRewardedAd)
            {
                return;
            }

            Advertising.ShowRewardedAd(AdPlacement.PlacementWithName(place));
        }

        public void ShowRewardedAd(string place, Action onCompleted)
        {
            if (!this.adServicesConfig.EnableRewardedAd)
            {
                return;
            }

            this.RewardedAdCompletedOneTimeAction += onCompleted;
            Advertising.ShowRewardedAd(AdPlacement.PlacementWithName(place));
        }

        public bool IsRewardedInterstitialAdReady() { return Advertising.IsRewardedInterstitialAdReady(); }

        public void ShowRewardedInterstitialAd(string place)
        {
            if (!this.adServicesConfig.EnableRewardedInterstitialAd)
            {
                return;
            }

            Advertising.ShowRewardedInterstitialAd(AdPlacement.PlacementWithName(place));
        }

        public void ShowRewardedInterstitialAd(string place, Action onCompleted)
        {
            if (!this.adServicesConfig.EnableRewardedInterstitialAd)
            {
                return;
            }

            this.RewardedInterstitialAdCompletedOneTimeAction += onCompleted;
            Advertising.ShowRewardedInterstitialAd(AdPlacement.PlacementWithName(place));
        }

        public void RemoveAds(bool revokeConsent = false) { Advertising.RemoveAds(revokeConsent); }

        public bool IsAdsInitialized() { return Advertising.IsInitialized(); }

        public bool IsRemoveAds() { return Advertising.IsAdRemoved(); }
    }
#endif
}