namespace ExtendModules.FirebaseAnalyticTracker.FirebaseWebGL
{
    public class FirebaseAnalytics
    {
        public static void SetUserId(string userId) => AnalyticsSetUserIdWeb(userId);

        public static void SetUserProperty(string name, string property) => AnalyticsSetUserPropertyWeb1(name, property);
        
        public static void SetUserProperty(Dictionary<string, object> properties) => AnalyticsSetUserPropertyWeb2(JsonConvert.SerializeObject((object)properties));

        public static void LogEvent(string name) => AnalyticsLogEventWeb3(name);

        public static void LogEvent(string name, string param, string value) => AnalyticsLogEventWeb1(name, param, value);

        public static void LogEvent(string name, string param, int value) => AnalyticsLogEventWeb1(name, param, $"{value}");

        public static void LogEvent(string name, string param, double value) => AnalyticsLogEventWeb1(name, param, $"{value}");

        public static void LogEvent(string name, string param, float value) => AnalyticsLogEventWeb1(name, param, $"{value}");

        public static void LogEvent(string name, string param, long value) => AnalyticsLogEventWeb1(name, param, $"{value}");
        
        public static void LogEvent(string name, Dictionary<string, object> parameters) => AnalyticsLogEventWeb2(name, JsonConvert.SerializeObject((object)parameters));
        
        [DllImport("__Internal")]
        private static extern void AnalyticsSetUserIdWeb(string id);

        [DllImport("__Internal")]
        private static extern void AnalyticsSetUserPropertyWeb1(string prop, string value);
        
        [DllImport("__Internal")]
        private static extern void AnalyticsSetUserPropertyWeb2(string properties);

        [DllImport("__Internal")]
        private static extern void AnalyticsLogEventWeb1(string name, string param, string value);

 		[DllImport("__Internal")]
        private static extern void AnalyticsLogEventWeb2(string name, string parameters);
        
        [DllImport("__Internal")]
        private static extern void AnalyticsLogEventWeb3(string name);
    }
}

