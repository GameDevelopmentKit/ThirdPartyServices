namespace ServiceImplementation.AdsServices.EasyMobile
{
#if EASY_MOBILE_PRO
    using System;
    using Core.AdsServices;
    using Core.AdsServices.Signals;
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

        private readonly ILogService   logService;
        private readonly SignalBus     signalBus;

        #endregion

        private event Action RewardedAdCompletedOneTimeAction;
        private event Action RewardedInterstitialAdCompletedOneTimeAction;

        public EasyMobileAdIml(ILogService logService, SignalBus signalBus)
        {
            this.logService   = logService;
            this.signalBus    = signalBus;
        }

        public void Initialize()
        {
            Advertising.InterstitialAdCompleted         += this.OnAdvertisingOnInterstitialAdCompleted;
            Advertising.RewardedAdCompleted             += this.OnAdvertisingOnRewardedAdCompleted;
            Advertising.RewardedAdSkipped               += this.OnAdvertisingOnRewardedAdSkipped;
            Advertising.RewardedInterstitialAdCompleted += this.OnAdvertisingOnRewardedInterstitialAdCompleted;
            Advertising.RewardedInterstitialAdSkipped   += this.OnAdvertisingOnRewardedInterstitialAdSkipped;
            Advertising.AdsRemoved                      += this.OnAdRemoved;
        }

        public void Dispose()
        {
            Advertising.InterstitialAdCompleted         -= this.OnAdvertisingOnInterstitialAdCompleted;
            Advertising.RewardedAdCompleted             -= this.OnAdvertisingOnRewardedAdCompleted;
            Advertising.RewardedAdSkipped               -= this.OnAdvertisingOnRewardedAdSkipped;
            Advertising.RewardedInterstitialAdCompleted -= this.OnAdvertisingOnRewardedInterstitialAdCompleted;
            Advertising.RewardedInterstitialAdSkipped   -= this.OnAdvertisingOnRewardedInterstitialAdSkipped;
            Advertising.AdsRemoved                      -= this.OnAdRemoved;
        }

        private void OnAdRemoved()
        {
            this.AdsRemoved?.Invoke();
        }
        private void OnAdvertisingOnRewardedInterstitialAdSkipped(RewardedInterstitialAdNetwork network, AdPlacement place)
        {
            this.RewardedInterstitialAdSkipped?.Invoke((Core.AdsServices.InterstitialAdNetwork)network, place.Name);
        }
        private void OnAdvertisingOnRewardedInterstitialAdCompleted(RewardedInterstitialAdNetwork network, AdPlacement place)
        {
            this.RewardedInterstitialAdCompleted?.Invoke((Core.AdsServices.InterstitialAdNetwork)network, place.Name);
            this.RewardedInterstitialAdCompletedOneTimeAction?.Invoke();
            this.RewardedInterstitialAdCompletedOneTimeAction = null;
        }
        private void OnAdvertisingOnRewardedAdSkipped(RewardedAdNetwork   network, AdPlacement place) { this.RewardedAdSkipped?.Invoke((Core.AdsServices.InterstitialAdNetwork)network, place.Name); }
        private void OnAdvertisingOnRewardedAdCompleted(RewardedAdNetwork network, AdPlacement place)
        {
            this.RewardedAdCompleted?.Invoke((Core.AdsServices.InterstitialAdNetwork)network, place.Name);
            this.RewardedAdCompletedOneTimeAction?.Invoke();
            this.RewardedAdCompletedOneTimeAction = null;
        }
        private void OnAdvertisingOnInterstitialAdCompleted(InterstitialAdNetwork network, AdPlacement place)
        {
            this.InterstitialAdCompleted?.Invoke((Core.AdsServices.InterstitialAdNetwork)network, place.Name);
        }


        #region Consent

        public void          GrantDataPrivacyConsent()                     { Advertising.GrantDataPrivacyConsent(); }
        public void          RevokeDataPrivacyConsent()                    { Advertising.RevokeDataPrivacyConsent(); }
        public void          GrantDataPrivacyConsent(AdNetwork  adNetwork) { Advertising.GrantDataPrivacyConsent((global::EasyMobile.AdNetwork)adNetwork); }
        public void          RevokeDataPrivacyConsent(AdNetwork adNetwork) { Advertising.RevokeDataPrivacyConsent((global::EasyMobile.AdNetwork)adNetwork); }
        public ConsentStatus GetDataPrivacyConsent(AdNetwork    adNetwork) { return (ConsentStatus)Advertising.GetDataPrivacyConsent((global::EasyMobile.AdNetwork)adNetwork); }

        #endregion

        #region Banner

        public void ShowBannerAd(BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom) { Advertising.ShowBannerAd((BannerAdPosition)bannerAdsPosition); }
        public void HideBannedAd()                                                               { Advertising.HideBannerAd(); }
        public void DestroyBannerAd()                                                            { Advertising.DestroyBannerAd(); }

        #endregion

        #region InterstitialAdCompleteHandler

        public event Action<Core.AdsServices.InterstitialAdNetwork, string> InterstitialAdCompleted;
        public bool                                                         IsInterstitialAdReady()          { return Advertising.IsInterstitialAdReady(); }
        public void ShowInterstitialAd(string place)
        {
            this.signalBus.Fire<ShowInterstitialAdSignal>();
            Advertising.ShowInterstitialAd(AdPlacement.PlacementWithName(place));
        }

        #endregion


        public event Action<Core.AdsServices.InterstitialAdNetwork, string> RewardedAdCompleted;
        public event Action<Core.AdsServices.InterstitialAdNetwork, string> RewardedAdSkipped;
        public bool                                                         IsRewardedAdReady()                              { return Advertising.IsRewardedAdReady(); }
        public void ShowRewardedAd(string place)
        {
            this.signalBus.Fire<ShowInterstitialAdSignal>();
            Advertising.ShowRewardedAd(AdPlacement.PlacementWithName(place)); 
        }
        public void ShowRewardedAd(string place, Action onCompleted)
        {
            this.signalBus.Fire<ShowInterstitialAdSignal>();
            this.RewardedAdCompletedOneTimeAction += onCompleted;
            Advertising.ShowRewardedAd(AdPlacement.PlacementWithName(place));
        }
        public event Action<Core.AdsServices.InterstitialAdNetwork, string> RewardedInterstitialAdCompleted;
        public event Action<Core.AdsServices.InterstitialAdNetwork, string> RewardedInterstitialAdSkipped;

        public bool IsRewardedInterstitialAdReady()                              { return Advertising.IsRewardedInterstitialAdReady(); }
        public void ShowRewardedInterstitialAd(string place)                     { Advertising.ShowRewardedInterstitialAd(AdPlacement.PlacementWithName(place)); }
        public void ShowRewardedInterstitialAd(string place, Action onCompleted)
        {
            this.signalBus.Fire<ShowInterstitialAdSignal>();
            this.RewardedInterstitialAdCompletedOneTimeAction += onCompleted;
            Advertising.ShowRewardedInterstitialAd(AdPlacement.PlacementWithName(place));
        }


        public event Action AdsRemoved;

        public void RemoveAds(bool revokeConsent = false)
        {
            Advertising.RemoveAds(revokeConsent);
        }

        public bool IsAdsInitialized() { return Advertising.IsInitialized(); }
        public bool IsRemoveAds()
        {
            return Advertising.IsAdRemoved();
        }
    }
#endif
}