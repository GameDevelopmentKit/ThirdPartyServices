#if CRAZYGAMES

namespace ServiceImplementation.AdsServices.CrazyGames
{
    using System;
    using Core.AdsServices;
    using Core.AnalyticServices.CommonEvents;
    using GameFoundation.DI;
    using global::CrazyGames;
    using UnityEngine;
    using VContainer;
    using Object = UnityEngine.Object;

    [Preserve]
    public class CrazyGameAdsWrapper : IAdServices, IInitializable
    {
        public string AdPlatform         => AdRevenueConstants.ARSourceCrazyGamesAds;
        public bool   IsAdsInitialized() => CrazySDK.IsInitialized;
        public void ShowBannerAd(BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom, int width = 320, int height = 50)
        {
            foreach (var bannerBanner in CrazySDK.Banner.Banners)
            {
                bannerBanner.gameObject.SetActive(true);
            }
        }
        public void HideBannedAd()
        {
            foreach (var bannerBanner in CrazySDK.Banner.Banners)
            {
                bannerBanner.gameObject.SetActive(false);
            }
        }
        public void DestroyBannerAd()
        {
            foreach (var bannerBanner in CrazySDK.Banner.Banners)
            {
                Object.Destroy(bannerBanner);
            }
        }
        public  bool IsInterstitialAdReady(string place) => !this.IsAdBlocked();
        public  void ShowInterstitialAd(string place)    { CrazySDK.Ad.RequestAd(CrazyAdType.Midgame, null, null, null); }
        public  bool IsRewardedAdReady(string place)     => !this.IsAdBlocked();
        private bool IsAdBlocked()                       => CrazySDK.Ad.AdblockStatus != AdblockStatus.Missing;
        public void ShowRewardedAd(string place, Action onCompleted, Action onFailed, Dictionary<string, object> metadata)
        {
            CrazySDK.Ad.RequestAd(CrazyAdType.Rewarded, () => // or CrazyAdType.Rewarded
            {
                // ad started
            }, _ =>
            {
                // ad error
                onFailed();
            }, onCompleted);
        }
        public void Initialize()  { CrazySDK.Init(() => { }); }
    }
}

#endif