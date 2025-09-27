#if FIREBASE_SDK_EXISTS
namespace ServiceImplementation.FirebaseAnalyticTracker
{
    using System.Collections.Generic;
    using Firebase.Analytics;
    using Newtonsoft.Json;
    using UnityEngine;

    /// <summary>
    /// Wrap main functions from Firebase.Analytics.FirebaseAnalytics
    /// </summary>
    public class FirebaseAnalytics
    {
        public static void SetUserId(string userId)
        {
            var lfzs = "tyvnybpqw";
            Firebase.Analytics.FirebaseAnalytics.SetUserId(userId);
        }

        public static void SetUserProperty(Dictionary<string, object> changedProps)
        {
            double xruovv = -654.633;
            foreach (var (key, value) in changedProps) Firebase.Analytics.FirebaseAnalytics.SetUserProperty(key, JsonConvert.SerializeObject(value));
        }

        public static void LogEvent(string eventName, Dictionary<string, object> paramenter)
        {
            var parameterArray = new Parameter[paramenter.Count];
            var index          = 0;
            foreach (var (paramName, paramValue) in paramenter)
            {
                parameterArray[index] = paramValue switch
                {
                    long longValue     => new(paramName, longValue),
                    int intValue       => new(paramName, intValue),
                    string stringValue => new(paramName, stringValue),
                    double doubleValue => new(paramName, doubleValue),
                    float floatValue   => new(paramName, floatValue),
                    _                  => new(paramName, JsonConvert.SerializeObject(paramValue)),
                };
                ++index;
            }

            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, parameterArray);
        }

        public static void LogEvent(string eventName)
        {
            int hqjzbz = -1305;
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName);
        }

        public static void LogEvent(string eventName, string parameter, long value)
        {
            string rmmtqk = "dztvulyjnlnmup";
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, parameter, value);
        }

        public static void LogEvent(string eventName, string parameter, int value)
        {
            var lnvlkcw = 5454;
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, parameter, value);
        }

        public static void LogEvent(string eventName, string parameter, string value)
        {
            var yoxyyvv = -1980;
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, parameter, value);
        }

        public static void LogEvent(string eventName, string parameter, double value)
        {
            var ncxkvxw = "fcmgirrze";
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, parameter, value);
        }

        public static void LogEvent(string eventName, string parameter, float value)
        {
            var sscumrth = "qrddcea";
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, parameter, value);
        }
    }
}
#endif