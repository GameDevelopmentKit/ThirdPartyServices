namespace Core.AdsServices
{
    using System;
    using GameFoundation.Scripts.Utilities.LogService;

    public class DummyAdServiceIml : IAdServices
    {
        private readonly ILogService logService;

        public DummyAdServiceIml(ILogService logService) { this.logService = logService; }

        public void GrantDataPrivacyConsent() { this.logService.Log("Dummy Grant consent"); }
        public void RevokeDataPrivacyConsent() { this.logService.Log("Dummy Revoke consent"); }
        public void GrantDataPrivacyConsent(AdNetwork adNetwork) { this.logService.Log($"Dummy Grant consent {adNetwork}"); }
        public void RevokeDataPrivacyConsent(AdNetwork adNetwork) { this.logService.Log($"Dummy revoke consent {adNetwork}"); }
        public ConsentStatus GetDataPrivacyConsent(AdNetwork adNetwork) { return ConsentStatus.Granted; }
        public void ShowBannerAd(BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom, int width = 320, int height = 50) { this.logService.Log($"Dummy show banner ad ay {bannerAdsPosition}"); }
        public void HideBannedAd() { this.logService.Log($"Dummy hide banner ad"); }
        public void DestroyBannerAd() { this.logService.Log($"Dummy destroy banner ad"); }
        public event Action<InterstitialAdNetwork, string> InterstitialAdCompleted;
        public bool IsInterstitialAdReady(string place) { return true; }
        public void ShowInterstitialAd(string place) { this.logService.Log($"Dummy show Interstitial ad at {place}"); }
        public event Action<RewardedAdNetwork, string> RewardedAdCompleted;
        public event Action<RewardedAdNetwork, string> RewardedAdSkipped;
        public bool IsRewardedAdReady(string place) { return true; }
        public void ShowRewardedAd(string place) { this.logService.Log($"Dummy show Reward ad at {place}"); }
        public void ShowRewardedAd(string place, Action onCompleted) { this.logService.Log($"Dummy show Reward ad at {place} then do {onCompleted}"); }
        public event Action<InterstitialAdNetwork, string> RewardedInterstitialAdCompleted;
        public event Action<InterstitialAdNetwork, string> RewardedInterstitialAdSkipped;
        public bool IsRewardedInterstitialAdReady() { return true; }
        public void ShowRewardedInterstitialAd(string place) { this.logService.Log($"Dummy show Rewarded Interstitial ad at {place}"); }
        public void ShowRewardedInterstitialAd(string place, Action onCompleted) { this.logService.Log($"Dummy show Rewarded Interstitial ad at {place} then do {onCompleted}"); }
        public event Action AdsRemoved;
        public void RemoveAds(bool revokeConsent = false) { this.logService.Log($"Dummy revoke consent"); }
        public bool IsAdsInitialized() { return true; }
        public bool IsRemoveAds() { return false; }
    }
}