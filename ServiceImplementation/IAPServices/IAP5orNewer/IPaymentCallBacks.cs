#if IAP_5_OR_NEWER

namespace ServiceImplementation.IAPServices.IAP5orNewer
{
    using System.Collections.Generic;
    using ServiceImplementation.IAPServices.Signals;
    using UnityEngine.Purchasing;
    using Zenject;

    public class IAPPaywallCallbacks
    {
        private          Iap5OrNewerServices iap5OrNewerServices;
        private readonly IapLogWrapped       iapLogWrapped;
        private readonly ISignalBus          signalBus;

        public IAPPaywallCallbacks(Iap5OrNewerServices paywallManager, IapLogWrapped iapLogWrapped, ISignalBus signalBus)
        {
            this.iap5OrNewerServices = paywallManager;
            this.iapLogWrapped       = iapLogWrapped;
            this.signalBus           = signalBus;
        }

        public void OnInitialProductsFetched(List<Product> products)
        {
            this.iapLogWrapped.LogConsole("===========");
            this.iapLogWrapped.LogConsole("OnInitialProductsFetched:");
            this.iapLogWrapped.LogFetchedProducts(products);
            this.iap5OrNewerServices.FetchExistingPurchases();
        }

        public void OnInitialProductsFetchFailed(ProductFetchFailed failure)
        {
            this.iapLogWrapped.LogConsole("===========");
            this.iapLogWrapped.LogConsole("OnInitialProductsFetchFailed:");
            this.iapLogWrapped.LogConsole(failure.FailureReason);
        }

        public void OnExistingPurchasesFetched(Orders existingOrders)
        {
            this.iapLogWrapped.LogConsole("===========");
            this.iapLogWrapped.LogConsole("OnExistingPurchasesFetched:");

            this.iapLogWrapped.LogConsole(this.iap5OrNewerServices.IsReceiptAvailable(existingOrders)
                ? "Success - Found Existing Orders with receipts"
                : "Notice: - No Existing Orders with receipts");
        }

        public void OnExistingPurchasesFetchFailed(PurchasesFetchFailureDescription failure)
        {
            this.iapLogWrapped.LogConsole("===========");
            this.iapLogWrapped.LogConsole("OnExistingPurchasesFetchFailed:");
            this.iapLogWrapped.LogConsole(failure.Message);
        }

        public void OnPurchasePending(PendingOrder order)
        {
            foreach (var cartItem in order.CartOrdered.Items())
            {
                var product = cartItem.Product;
                this.iap5OrNewerServices.OnCompletePurchase?.Invoke(product.definition.id);
                this.iap5OrNewerServices.OnCompletePurchase = null;
                this.iapLogWrapped.LogCompletedPurchase(product, order.Info);
                this.iap5OrNewerServices.ValidatePurchaseIfPossible(order.Info);
            }

            this.iap5OrNewerServices.ConfirmOrderIfAutomatic(order);
        }

        public void OnPurchaseConfirmed(Order order)
        {
            switch (order)
            {
                case FailedOrder failedOrder:
                    this.OnConfirmationFailed(failedOrder);

                    break;
                case ConfirmedOrder confirmedOrder:
                    this.OnPurchaseConfirmed(confirmedOrder);
                 
                    break;
            }
        }

        private void OnConfirmationFailed(FailedOrder failedOrder)
        {
            var reason = failedOrder.FailureReason;

            foreach (var cartItem in failedOrder.CartOrdered.Items())
            {
                this.iapLogWrapped.LogFailedConfirmation(cartItem.Product, reason);
            }
        }

        public void OnPurchaseConfirmed(ConfirmedOrder order)
        {
            foreach (var cartItem in order.CartOrdered.Items())
            {
                var product = cartItem.Product;
                this.iap5OrNewerServices.OnCompletePurchase?.Invoke(product.definition.id);
                this.iap5OrNewerServices.OnCompletePurchase = null;
                this.iapLogWrapped.LogConfirmedOrder(product, order.Info);
            }
        }

        public void OnPurchaseFailed(FailedOrder failedOrder)
        {
            var reason = failedOrder.FailureReason;

            foreach (var cartItem in failedOrder.CartOrdered.Items())
            {
                this.iapLogWrapped.LogFailedPurchase(cartItem.Product, reason);
                this.signalBus.Fire(new OnIAPPurchaseFailedSignal(cartItem.Product.definition.storeSpecificId, reason.ToString()));
            }
        }

        public void OnOrderDeferred(DeferredOrder deferredOrder)
        {
            foreach (var cartItem in deferredOrder.CartOrdered.Items())
            {
                this.iapLogWrapped.LogDeferredPurchase(cartItem.Product);
            }
        }
    }
}
#endif
