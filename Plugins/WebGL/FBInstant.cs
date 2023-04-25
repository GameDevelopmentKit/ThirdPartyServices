namespace Packages.com.gdk._3rd.Plugins.WebGL
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Newtonsoft.Json;

    public class FBInstant
    {
        public static ContextPlayer player      = new();
        public static GameContext   context     = new();
        public static FBLeaderboard leaderboard = new();

        public static FBPayments payments = new();


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
        public static extern void fbinstant_updateAsync(string jsonStr);

        [DllImport("__Internal")]
        public static extern void fbinstant_shareAsync(string jsonStr);
        
        [DllImport("__Internal")]
        public static extern void fbinstant_getBannerAdAsync(string adId);
        [DllImport("__Internal")]
        public static extern void fbinstant_hideBannerAdAsync();
        
        [DllImport("__Internal")]
        public static extern bool fbinstant_getInterstitialAdAsync(string adId);

        [DllImport("__Internal")]
        public static extern void fbinstant_showInterstitialAdAsync();

        [DllImport("__Internal")]
        public static extern void fbinstant_getRewardedVideoAsync(string adId);

        [DllImport("__Internal")]
        public static extern void fbinstant_showRewardedVideoAsync();


        private static List<string> supportedApis = null; // use memory cache
        public static List<string> getSupportedAPIs()
        {
            if (supportedApis != null)
            {
                return supportedApis;
            }
            
            var apiJsonStr = fbinstant_getSupportedAPIs();
            var apiList = JsonConvert.DeserializeObject<string[]>(apiJsonStr);
            supportedApis = new List<string>(apiList);
            return supportedApis;
        }

        /// <summary>
        /// Only Support on Web & Android
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="entryPointData"></param>
        public static void switchGameAsync(string appId, Dictionary<string, object> entryPointData)
        {
            fbinstant_switchGameAsync(appId, /*SimpleJson.SimpleJson.SerializeObject(entryPointData)*/null);
        }

        public static void shareAsync(Dictionary<string, object> p, System.Action cb)
        {
            // IGExporter.shareAsync_Callback = cb;
            fbinstant_shareAsync(/*SimpleJson.SimpleJson.SerializeObject(p)*/"");
        }

        public static void updateAsync(Dictionary<string, object> p)
        {
            fbinstant_updateAsync(/*SimpleJson.SimpleJson.SerializeObject(p)*/null);
        }

        #region Banner

       
        public static void ShowBannerAd(string adId)
        {
            fbinstant_getBannerAdAsync(adId);
        }

        public static void HideBannerAd()
        {
            fbinstant_hideBannerAdAsync();
        }

        #endregion

        #region IntertitialAd

        public static bool PreloadInterstitialAd(string adId)
        {
            var result = fbinstant_getInterstitialAdAsync(adId);
            return result;
        }

        public static void ShowInterstitialAd(string adId)
        {
            if (!PreloadInterstitialAd(adId)) return;
            
        }

        #endregion
       
        
        public static void PreloadRewaredVideoAd(string adId, System.Action<bool> onReadCallback = null)
        {
            // IGExporter.PreloadRewaredVideoAd_Ready_Callback = onReadCallback;
            fbinstant_getRewardedVideoAsync(adId);
        }

        public static void ShowRewaredVideoAd(System.Action completeCallback, System.Action<FBError> errorCallback, System.Action preloadMethod)
        {
            // IGExporter.ShowRewaredVideoAd_Complete_Callback = completeCallback;
            // IGExporter.ShowRewaredVideoAd_Error_Callback = errorCallback;
            // IGExporter.ShowRewaredVideoAd_Preload_Method = preloadMethod;
            fbinstant_showRewardedVideoAsync();
        }

    }

}
