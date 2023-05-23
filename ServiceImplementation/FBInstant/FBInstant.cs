namespace ServiceImplementation.FBInstant
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Newtonsoft.Json;

    internal static class FBInstant
    {
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