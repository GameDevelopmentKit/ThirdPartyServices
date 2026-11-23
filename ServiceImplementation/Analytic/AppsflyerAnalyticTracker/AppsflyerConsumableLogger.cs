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
    /// Keeps AppsFlyer ROI360 purchase revenue complete on iOS. With StoreKit 2 the Purchase Connector does not observe
    /// consumables bought in the app: each one is handed over by transaction id. The plugin looks it up asynchronously in
    /// <c>Transaction.all</c>, which no longer lists finished consumables, so the IAP handler confirms iOS orders after a delay.
    /// No-op on other platforms (the connector observes Google Play purchases itself) and when ROI360 is off.
    /// </summary>
    public sealed class AppsflyerConsumableLogger : IIapOrderObserver
    {
        private readonly AnalyticConfig analyticConfig;

        public AppsflyerConsumableLogger(AnalyticConfig analyticConfig)
        {
            this.analyticConfig = analyticConfig;
        }

        public void OnOrderReceived(IapOrder order)
        {
            if (!this.analyticConfig.AppsflyerIsEnableRoi360) return;
#if UNITY_IOS && !UNITY_EDITOR
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
    }
}
#endif
