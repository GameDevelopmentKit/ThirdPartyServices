#if IAP_5_OR_NEWER


namespace ServiceImplementation.IAPServices.IAP5orNewer
{
    using System.Collections.Generic;
    using GameFoundation.Scripts.Utilities.LogService;
    using UnityEngine.Purchasing;
    using UnityEngine.Purchasing.Security;

    public class IapLogWrapped
    {
        private readonly ILogService logger;

        public IapLogWrapped(ILogService logger) { this.logger = logger; }

        public void LogFetchedProducts(List<Product> products)
        {
            if (products.Count > 0)
            {
                foreach (var product in products)
                {
                    this.LogConsole($"Fetched {product.definition.id}");
                }
            }
            else
            {
                this.LogConsole("No Products Fetched.");
            }
        }

        public void LogConfirmedOrder(Product product, IOrderInfo orderInfo)
        {
            this.LogConsole("===========");
            this.LogConsole($"Confirmed Product: '{product.definition.id}'");
            this.LogConsole($"Product transaction id: {orderInfo.TransactionID}.");
            this.LogConsole($"Product receipt length: {orderInfo.Receipt?.Length}.");
            this.LogConsole($"Product Type: '{product.definition.type}'");
        }

        public void LogReceiptValidation(IPurchaseReceipt productReceipt)
        {
            this.LogConsole($"Product ID: '{productReceipt.productID}', Date: '{productReceipt.purchaseDate}', Transaction ID: '{productReceipt.transactionID}'");
            this.LogGooglePlayReceiptValidationInfo(productReceipt);
            this.LogAppleReceiptValidationInfo(productReceipt);
        }

        public void LogGooglePlayReceiptValidationInfo(IPurchaseReceipt productReceipt)
        {
            GooglePlayReceipt googleReceipt = productReceipt as GooglePlayReceipt;

            if (googleReceipt != null)
            {
                this.LogConsole($"GooglePlay - State: '{googleReceipt.purchaseState}', Token: '{googleReceipt.purchaseToken}'");
            }
        }

        public void LogAppleReceiptValidationInfo(IPurchaseReceipt productReceipt)
        {
            if (productReceipt is AppleInAppPurchaseReceipt appleReceipt)
            {
                this.LogConsole(
                    $"Apple - Original Transaction: '{appleReceipt.originalTransactionIdentifier}', Expiration Date : '{appleReceipt.subscriptionExpirationDate}', Cancellation Date : '{appleReceipt.cancellationDate}', Quandtity : '{appleReceipt.quantity}'");
            }
        }

        public void LogCompletedPurchase(Product product, IOrderInfo orderInfo)
        {
            this.LogConsole("===========");
            this.LogConsole($"Purchased Product: '{product.definition.id}'");
            this.LogConsole($"Product transaction id: {orderInfo.TransactionID}.");
            this.LogConsole($"Product receipt length: {orderInfo.Receipt?.Length}.");
            this.LogConsole($"Product Type: '{product.definition.type}'");
        }

        public void LogFailedConfirmation(Product product, PurchaseFailureReason reason)
        {
            this.LogConsole("===========");
            this.LogConsole("Purchase Confirmation Failed");
            this.LogConsole($"Product: '{product.definition.storeSpecificId}'");
            this.LogConsole($"FailureReason: {reason.ToString()}.");
        }

        public void LogFailedPurchase(Product product, PurchaseFailureReason reason)
        {
            this.LogConsole("===========");
            this.LogConsole("PurchaseFailed");
            this.LogConsole($"Product: '{product.definition.storeSpecificId}'");
            this.LogConsole($"FailureReason: {reason.ToString()}.");
        }

        public void LogDeferredPurchase(Product product)
        {
            this.LogConsole("===========");
            this.LogConsole("PurchaseDeferred");
            this.LogConsole($"Product: '{product.definition.storeSpecificId}'");
        }

        public void LogConsole(string msg) { this.logger.Log($"IAP {msg}"); }
    }
}
#endif