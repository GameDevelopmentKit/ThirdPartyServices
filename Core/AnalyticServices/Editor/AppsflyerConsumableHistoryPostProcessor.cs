#if UNITY_IOS && APPSFLYER
namespace Core.AnalyticServices.Editor
{
    using System.IO;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEditor.iOS.Xcode;
    using UnityEngine;

    /// <summary>
    /// Sets <c>SKIncludeConsumableInAppPurchaseHistory = YES</c> in the iOS Info.plist. On iOS 18+ StoreKit 2 then keeps finished
    /// consumables in the transaction history, and the AppsFlyer Purchase Connector (ROI360) logs them automatically
    /// (AppsFlyer docs: without the flag, consumables must be logged manually). Below iOS 18 the flag has no effect and
    /// <c>AppsflyerConsumableLogger</c> logs them. Unity IAP reads only current entitlements and unfinished transactions, so it
    /// is not affected.
    /// </summary>
    public static class AppsflyerConsumableHistoryPostProcessor
    {
        private const string IncludeConsumableHistoryKey = "SKIncludeConsumableInAppPurchaseHistory";

        [PostProcessBuild(100)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            if (target != BuildTarget.iOS) return;

            var plistPath = Path.Combine(path, "Info.plist");
            if (!File.Exists(plistPath))
            {
                Debug.LogWarning($"[AppsflyerConsumableHistoryPostProcessor] Info.plist not found, {IncludeConsumableHistoryKey} not set.");
                return;
            }

            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            plist.root.SetBoolean(IncludeConsumableHistoryKey, true);
            plist.WriteToFile(plistPath);
        }
    }
}
#endif
