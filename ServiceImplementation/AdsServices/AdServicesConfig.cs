namespace ServiceImplementation.AdsServices
{
    public class AdServicesConfig
    {
        public bool EnableBannerAd            { get; set; } = true;
        public bool EnableInterstitialAd      { get; set; } = true;
        public bool EnableMRECAd              { get; set; } = true;
        public bool EnableAOAAd               { get; set; } = true;
        public bool EnableRewardedAd          { get; set; } = true;
        public bool EnableRewardedInterstitialAd { get; set; } = true;
        public bool EnableNativeAd            { get; set; } = true;
    }
}