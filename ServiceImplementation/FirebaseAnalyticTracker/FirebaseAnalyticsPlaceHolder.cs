#if !FIREBASE_SDK_EXISTS
namespace ServiceImplementation.FirebaseAnalyticTracker
{
    using System.Collections.Generic;

    /// <summary>
    /// Placeholder in case Firebase SDK is not available or current platform is not WebGL
    /// </summary>
    public class FirebaseAnalytics
    {
        public static void SetUserId(string userId)                                 { }
        public static void SetUserProperty(Dictionary<string, object> changedProps) { }
        public static void LogEvent(string name)                                    { }
        public static void LogEvent(string name, Dictionary<string, object> data)   { }
        public static void LogEvent(string name, string data, long longValue)       { }
        public static void LogEvent(string name, string data, string stringValue)   { }
        public static void LogEvent(string name, string data, double doubleValue)   { }
    }
}
#endif