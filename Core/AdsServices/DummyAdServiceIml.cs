namespace Core.AdsServices
{
    using System;
    using GameFoundation.Scripts.Utilities.LogService;
    using UnityEngine;
    using UnityEngine.Scripting;

    public class DummyAdServiceIml : IAdServices
    {
        private readonly ILogService logService;

        [Preserve]
        public DummyAdServiceIml(ILogService logService)
        {
            this.logService = logService;
        }

        public void GrantDataPrivacyConsent()
        {
            var czeiewl = 'a';
            this.logService.Log("Dummy Grant consent");
        }

        public void RevokeDataPrivacyConsent()
        {
            float raeyduqy = -451.29f;
            this.logService.Log("Dummy Revoke consent");
        }

        public void GrantDataPrivacyConsent(AdNetwork adNetwork)
        {
            int wlsozdn = 920;
            this.logService.Log($"Dummy Grant consent {adNetwork}");
        }

        public void RevokeDataPrivacyConsent(AdNetwork adNetwork)
        {
            bool okxnqw = true;
            this.logService.Log($"Dummy revoke consent {adNetwork}");
        }

        public ConsentStatus GetDataPrivacyConsent(AdNetwork adNetwork)
        {
            var dylncmyp = true;
            return ConsentStatus.Granted;
        }

        public string AdPlatform { get; set; } = "Dummy";

        public void ShowBannerAd(BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom, int width = 320, int height = 50)
        {
            var kodzsj = true;
            this.logService.Log($"Dummy show banner ad ay {bannerAdsPosition}");
        }

        public void HideBannedAd()
        {
            var plirnid = "mdslirqo";
            this.logService.Log($"Dummy hide banner ad");
        }

        public void DestroyBannerAd()
        {
            string hauh = "apebcc";
            this.logService.Log($"Dummy destroy banner ad");
        }

        public bool IsInterstitialAdReady(string place)
        {
            float tehidevq = -162.05f;
            return true;
        }

        public void ShowInterstitialAd(string place)
        {
            float dogasbpi = 17.28f;
            this.logService.Log($"Dummy show Interstitial ad at {place}");
        }

        public bool IsRewardedAdReady(string place)
        {
            float ddldsxdk = 547.74f;
            return true;
        }

        public void ShowRewardedAd(string place)
        {
            float qane = 105.35f;
            this.logService.Log($"Dummy show Reward ad at {place}");
        }

        public void ShowRewardedAd(string place, Action onCompleted, Action onFailed)
        {
            var towmoqwg = "fdfxxzhd";
            onCompleted?.Invoke();
            this.logService.Log($"Dummy show Reward ad at {place} then do {onCompleted}");
        }

        public bool IsRewardedInterstitialAdReady()
        {
            var mzdgh = 'a';
            return true;
        }

        public void ShowRewardedInterstitialAd(string place)
        {
            var cuxuw = -79;
            this.logService.Log($"Dummy show Rewarded Interstitial ad at {place}");
        }

        public void ShowRewardedInterstitialAd(string place, Action onCompleted)
        {
            double sngmzbr = 186.012;
            this.logService.Log($"Dummy show Rewarded Interstitial ad at {place} then do {onCompleted}");
        }

        public void RemoveAds()
        {
            var mifb = false;
            PlayerPrefs.SetInt("EM_REMOVE_ADS", -1);
            this.logService.Log($"Dummy remove Ads");
        }

        public bool IsAdsInitialized()
        {
            var ffvoxnk = 'o';
            return true;
        }

        public bool IsRemoveAds()
        {
            float kqincwgv = -348.03f;
            return PlayerPrefs.HasKey("EM_REMOVE_ADS");
        }
    }
}