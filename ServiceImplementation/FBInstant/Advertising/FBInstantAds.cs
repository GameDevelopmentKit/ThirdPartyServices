namespace ServiceImplementation.FBInstant.Advertising
{
    using System.Runtime.InteropServices;

    internal static class FBInstantAds
    {
        [DllImport("__Internal")]
        internal static extern void ShowBannerAd(string adId, string callbackObj, string callbackFunc);

        [DllImport("__Internal")]
        internal static extern void HideBannerAd(string callbackObj, string callbackFunc);

        [DllImport("__Internal")]
        internal static extern bool IsInterstitialAdReady();

        [DllImport("__Internal")]
        internal static extern void LoadInterstitialAd(string adId, string callbackObj, string callbackFunc);

        [DllImport("__Internal")]
        internal static extern void ShowInterstitialAd(string placement, string callbackObj, string callbackFunc);

        [DllImport("__Internal")]
        internal static extern bool IsRewardedAdReady();

        [DllImport("__Internal")]
        internal static extern void LoadRewardedAd(string adId, string callbackObj, string callbackFunc);

        [DllImport("__Internal")]
        internal static extern void ShowRewardedAd(string placement, string callbackObj, string callbackFunc);
    }
}