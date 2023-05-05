﻿namespace ServiceImplementation.FBInstant.Adsvertising
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Newtonsoft.Json;
    using ServiceImplementation.FBInstant.Leaderboard;
    using ServiceImplementation.FBInstant.Payment;

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

        #region Ads

        [DllImport("__Internal")]
        public static extern void ShowBannerAd(string placement);

        [DllImport("__Internal")]
        public static extern void HideBannerAd();

        [DllImport("__Internal")]
        public static extern void LoadInterstitialAd(string placement, Action onSuccess, Action<string> onFail);

        [DllImport("__Internal")]
        public static extern void ShowInterstitialAd(string placement, Action onSuccess, Action<string> onFail);
        
        [DllImport("__Internal")]
        public static extern void LoadRewardedAd(string placement, Action onSuccess, Action<string> onFail);

        [DllImport("__Internal")]
        public static extern void ShowRewardedAd(string placement, Action onSuccess, Action<string> onFail);

        #endregion

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

        /// <summary>
        /// Only Support on Web & Android
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="entryPointData"></param>
        public static void switchGameAsync(string appId, Dictionary<string, object> entryPointData)
        {
            fbinstant_switchGameAsync(appId, JsonConvert.SerializeObject(entryPointData));
        }

        public static void shareAsync(Dictionary<string, object> p, Action cb)
        {
            fbinstant_shareAsync(JsonConvert.SerializeObject(p));
        }

        public static void updateAsync(Dictionary<string, object> p)
        {
            fbinstant_updateAsync(JsonConvert.SerializeObject(p));
        }
    }
}