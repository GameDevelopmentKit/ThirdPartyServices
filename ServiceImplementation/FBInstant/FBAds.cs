namespace ServiceImplementation.FBInstant
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Newtonsoft.Json;

    public class FBAds
    {
        public static ContextPlayer player      = new ContextPlayer();
        public static GameContext   context     = new GameContext();
        public static FBLeaderboard leaderboard = new FBLeaderboard();

        public static FBPayments payments = new FBPayments();
        
        #region javascript

        [DllImport("__Internal")]
        public static extern string fbinstant_getSupportedAPIs();

        [DllImport("__Internal")]
        public static extern void fbinstant_switchGameAsync(string appId, string entryPointDataJsonStr);

        [DllImport("__Internal")]
        public static extern string getPlatform();

        [DllImport("__Internal")]
        public static extern string getSDKVersion();

        [DllImport("__Internal")]
        public static extern void quit();

        [DllImport("__Internal")]
        public static extern void logEvent(string eventName, long valueToSum, string jsonStr);

        [DllImport("__Internal")]
        public static extern string getEntryPointData();

        [DllImport("__Internal")]
        public static extern string getLocale();


        [DllImport("__Internal")]
        public static extern void fbinstant_showBannerAdAsync(string adId);
        [DllImport("__Internal")]
        public static extern void fbinstant_hideBannerAdAsync();
        [DllImport("__Internal")]
        public static extern bool fbinstant_isInterstitialAdReady(string adId);
        [DllImport("__Internal")]
        public static extern void fbinstant_showInterstitialAdAsync();
        [DllImport("__Internal")]
        public static extern bool fbinstant_isRewardVideoReady(string adId);

        [DllImport("__Internal")]
        public static extern void fbinstant_showRewardedVideoAsync();

        [DllImport("__Internal")]
        public static extern void fbinstant_updateAsync(string jsonStr);

        [DllImport("__Internal")]
        public static extern void fbinstant_shareAsync(string jsonStr);

        
        #endregion
        
        private static List<string> supportedApis = null; // use memory cache
        public static List<string> getSupportedAPIs()
        {
            if (supportedApis != null)
            {
                return supportedApis;
            }

            var apiJsonStr = fbinstant_getSupportedAPIs();
            var apiList    = JsonConvert.DeserializeObject<string[]>(apiJsonStr);
            supportedApis = new List<string>(apiList);
            return supportedApis;
        }

        #region banner

        public static void ShowBannerAd(string adId)
        {
            fbinstant_showBannerAdAsync(adId);
        }

        public static void HideBannerAd()
        {
            fbinstant_hideBannerAdAsync();   
        }

        #endregion

        #region interstitial

        public static bool IsInterstitialAdReady(string adId) { return fbinstant_isInterstitialAdReady(adId); }

        public static void ShowInterstitialAd(string adId)
        {
            if (!fbinstant_isInterstitialAdReady(adId)) return;
            fbinstant_showInterstitialAdAsync();
        }

        #endregion

        #region reward video

        public static bool IsRewardVideoReady(string adId)
        {
            return fbinstant_isRewardVideoReady(adId);
        }

        public static void ShowRewardedVideoAd(string adId)
        {
            if (!fbinstant_isRewardVideoReady(adId)) return;
            fbinstant_showRewardedVideoAsync();
        }

        #endregion
        
        /// <summary>
        /// Only Support on Web & Android
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="entryPointData"></param>
        public static void switchGameAsync(string appId, Dictionary<string, object> entryPointData) { fbinstant_switchGameAsync(appId, JsonConvert.SerializeObject(entryPointData)); }

        public static void shareAsync(Dictionary<string, object> p, Action cb) { fbinstant_shareAsync(JsonConvert.SerializeObject(p)); }

        public static void updateAsync(Dictionary<string, object> p) { fbinstant_updateAsync(JsonConvert.SerializeObject(p)); }
    }

}