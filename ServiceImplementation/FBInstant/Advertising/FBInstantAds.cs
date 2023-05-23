namespace ServiceImplementation.FBInstant.Advertising
{
    using System.Runtime.InteropServices;

    internal static class FBInstantAds
    {
        [DllImport("__Internal")]
        internal static extern void ShowBannerAd(string placement);

        [DllImport("__Internal")]
        internal static extern void HideBannerAd();

        [DllImport("__Internal")]
        internal static extern bool IsInterstitialAdReady(string placement);

        [DllImport("__Internal")]
        internal static extern void LoadInterstitialAd(string placement, string callbackObj, string callbackFunc);

        [DllImport("__Internal")]
        internal static extern void ShowInterstitialAd(string placement, string callbackObj, string callbackFunc);

        [DllImport("__Internal")]
        internal static extern bool IsRewardedAdReady(string placement);

        [DllImport("__Internal")]
        internal static extern void LoadRewardedAd(string placement, string callbackObj, string callbackFunc);

        [DllImport("__Internal")]
        internal static extern void ShowRewardedAd(string placement, string callbackObj, string callbackFunc);
    }
}