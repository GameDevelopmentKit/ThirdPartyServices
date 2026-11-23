#if APPSFLYER
namespace ServiceImplementation.AppsflyerAnalyticTracker
{
    using System;
    using System.Linq;
    using AppsFlyerSDK;
    using Core.AnalyticServices;
    using ServiceImplementation.IAPServices;
    using UnityEngine;

    /// <summary>
    /// Keeps AppsFlyer ROI360 purchase revenue complete for consumables on iOS 15–17. AppsFlyer docs: with StoreKit 2,
    /// consumables are logged automatically only on iOS 18+ with <c>SKIncludeConsumableInAppPurchaseHistory = YES</c> in
    /// Info.plist (set by <c>AppsflyerConsumableHistoryPostProcessor</c>); below iOS 18 they must be logged manually.
    /// iOS 18+ is left to the connector so revenue is not counted twice.
    /// No-op on other platforms (the connector observes Google Play purchases itself) and when ROI360 is off.
    /// </summary>
    public sealed class AppsflyerConsumableLogger : IIapOrderObserver
    {
        // First iOS version that logs consumables automatically (with the Info.plist flag).
        private const int AutoLogConsumablesIosMajorVersion = 18;

        private readonly AnalyticConfig analyticConfig;

        public AppsflyerConsumableLogger(AnalyticConfig analyticConfig)
        {
            this.analyticConfig = analyticConfig;
        }

        public void OnOrderReceived(IapOrder order)
        {
            if (!this.analyticConfig.AppsflyerIsEnableRoi360) return;
#if UNITY_IOS && !UNITY_EDITOR
            if (GetIosMajorVersion() >= AutoLogConsumablesIosMajorVersion) return;
            if (string.IsNullOrEmpty(order.TransactionId)) return;
            if (!order.Products.Any(product => product.ProductType == ProductType.Consumable)) return;

            try
            {
                AppsFlyerPurchaseConnector.logConsumableTransaction(order.TransactionId);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[IAP] AppsFlyer logConsumableTransaction failed for {order.TransactionId}: {exception.Message}");
            }
#endif
        }

#if UNITY_IOS && !UNITY_EDITOR
        /// <summary>Major iOS version ("17.5.1" → 17); 0 if it cannot be read, so the consumable is still logged manually.</summary>
        private static int GetIosMajorVersion()
        {
            var version = UnityEngine.iOS.Device.systemVersion;
            if (string.IsNullOrEmpty(version)) return 0;

            var dotIndex = version.IndexOf('.');
            var major = dotIndex < 0 ? version : version.Substring(0, dotIndex);
            return int.TryParse(major, out var value) ? value : 0;
        }
#endif
    }
}
#endif
