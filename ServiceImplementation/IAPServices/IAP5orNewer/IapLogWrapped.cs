#if IAP_5_OR_NEWER

// Cùng gate như Iap5OrNewerServices.cs — file này cũng chạm
// UnityEngine.Purchasing.Security, nên nó là CS0246 tiếp theo nếu không guard.
#if IAP_LOCAL_VALIDATION && UNITY_ANDROID && !UNITY_EDITOR
#define IAP_GOOGLE_RECEIPT_VALIDATION
#endif

namespace ServiceImplementation.IAPServices.IAP5orNewer
{
    using System.Collections.Generic;
    using GameFoundation.Scripts.Utilities.LogService;
    using UnityEngine.Purchasing;
#if IAP_GOOGLE_RECEIPT_VALIDATION
    using UnityEngine.Purchasing.Security;
#endif

    public class IapLogWrapped
    {
        private readonly ILogService logger;

        public IapLogWrapped(ILogService logger) { this.logger = logger; }

        public void LogConsole(string msg) { this.logger.Log($"IAP {msg}"); }

        public void LogFetchedProducts(List<Product> products)
        {
            if (products == null || products.Count == 0)
            {
                this.LogConsole("No Products Fetched.");

                return;
            }

            foreach (var product in products)
            {
                this.LogConsole($"Fetched {product.definition.id} - {product.metadata?.localizedPriceString}");
            }
        }

        public void LogCompletedPurchase(Product product, IOrderInfo orderInfo) { this.LogOrder("Purchased Product", product, orderInfo); }

        public void LogConfirmedOrder(Product product, IOrderInfo orderInfo) { this.LogOrder("Confirmed Product", product, orderInfo); }

        private void LogOrder(string label, Product product, IOrderInfo orderInfo)
        {
            this.LogConsole("===========");
            this.LogConsole($"{label}: '{product.definition.id}'");
            this.LogConsole($"Transaction id: {orderInfo.TransactionID}");
            this.LogConsole($"Receipt length: {orderInfo.Receipt?.Length}");
            this.LogConsole($"Product Type: '{product.definition.type}'");
        }

        public void LogFailedConfirmation(Product product, PurchaseFailureReason reason) { this.LogFailure("Purchase Confirmation Failed", product, reason); }

        public void LogFailedPurchase(Product product, PurchaseFailureReason reason) { this.LogFailure("PurchaseFailed", product, reason); }

        private void LogFailure(string label, Product product, PurchaseFailureReason reason)
        {
            this.LogConsole("===========");
            this.LogConsole(label);
            this.LogConsole($"Product: '{product.definition.storeSpecificId}'");
            this.LogConsole($"FailureReason: {reason}");
        }

        public void LogDeferredPurchase(Product product)
        {
            this.LogConsole("===========");
            this.LogConsole("PurchaseDeferred");
            this.LogConsole($"Product: '{product.definition.storeSpecificId}'");
        }

#if IAP_GOOGLE_RECEIPT_VALIDATION
        public void LogReceiptValidation(IPurchaseReceipt receipt)
        {
            this.LogConsole($"Product ID: '{receipt.productID}', Date: '{receipt.purchaseDate}', Transaction ID: '{receipt.transactionID}'");

            if (receipt is GooglePlayReceipt googleReceipt)
            {
                this.LogConsole($"GooglePlay - State: '{googleReceipt.purchaseState}', Token: '{googleReceipt.purchaseToken}'");
            }
        }

        // LogAppleReceiptValidationInfo đã xoá: IAP v5 deprecate Apple receipt validation,
        // AppleInAppPurchaseReceipt không bao giờ xuất hiện trên đường Google-only nữa.
#endif
    }
}
#endif
