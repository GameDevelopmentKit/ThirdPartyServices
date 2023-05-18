namespace Core.AdsServices
{
    using System.Collections.Generic;

    public class AdServicesConfig
    {
        public bool EnableBannerAd               { get; set; } = true;
        public bool EnableInterstitialAd         { get; set; } = true;
        public bool EnableMRECAd                 { get; set; } = true;
        public bool EnableAOAAd                  { get; set; } = true;
        public bool EnableRewardedAd             { get; set; } = true;
        public bool EnableRewardedInterstitialAd { get; set; } = true;
        public bool EnableNativeAd               { get; set; } = true;

        public int InterstitialAdInterval    { get; set; } = 10;
        public int MinPauseSecondToShowAoaAd { get; set; } = 0;

        public List<string> InterstitialAdPlacements { get; set; } = new();
        public List<string> RewardedAdPlacements     { get; set; } = new();
    }
}