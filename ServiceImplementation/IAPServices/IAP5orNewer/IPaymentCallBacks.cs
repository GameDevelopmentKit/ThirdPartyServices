#if IAP_5_OR_NEWER

namespace ServiceImplementation.IAPServices.IAP5orNewer
{
    using System.Collections.Generic;
    using ServiceImplementation.IAPServices.Signals;
    using UnityEngine.Purchasing;
    using Zenject;

    public class IAPPaywallCallbacks
    {
        private readonly Iap5OrNewerServices iapServices;
        private readonly IapLogWrapped       iapLogWrapped;
        private readonly ISignalBus          signalBus;

        public IAPPaywallCallbacks(Iap5OrNewerServices iapServices, IapLogWrapped iapLogWrapped, ISignalBus signalBus)
        {
            this.iapServices   = iapServices;
            this.iapLogWrapped = iapLogWrapped;
            this.signalBus     = signalBus;
        }

        #region Fetch

        public void OnInitialProductsFetched(List<Product> products)
        {
            this.iapLogWrapped.LogConsole("===========");
            this.iapLogWrapped.LogConsole("OnInitialProductsFetched:");
            this.iapLogWrapped.LogFetchedProducts(products);
            this.iapServices.FetchExistingPurchases();
        }

        public void OnInitialProductsFetchFailed(ProductFetchFailed failure)
        {
            this.iapLogWrapped.LogConsole("===========");
            this.iapLogWrapped.LogConsole($"OnInitialProductsFetchFailed: {failure.FailureReason}");
        }

        public void OnExistingPurchasesFetched(Orders existingOrders)
        {
            this.iapLogWrapped.LogConsole("===========");
            this.iapLogWrapped.LogConsole("OnExistingPurchasesFetched:");

            // Bản cũ gọi IsReceiptAvailable() 2 lần (hàm đó có side-effect ghi ownedProducts)
            // và log trùng 2 dòng. Giờ tách rõ: cache là cache, query là query.
            this.iapServices.CacheOrders(existingOrders);

            if (!this.iapServices.IsReceiptAvailable(existingOrders))
            {
                this.iapLogWrapped.LogConsole("Notice: - No Existing Orders with receipts");

                return;
            }

            this.iapLogWrapped.LogConsole("Success - Found Existing Orders with receipts");

            foreach (var order in existingOrders.ConfirmedOrders)
            {
                foreach (var item in order.Info.PurchasedProductInfo)
                {
                    this.signalBus.Fire(new OnRestorePurchaseCompleteSignal(item.productId));
                }
            }
        }

        public void OnExistingPurchasesFetchFailed(PurchasesFetchFailureDescription failure)
        {
            this.iapLogWrapped.LogConsole("===========");
            this.iapLogWrapped.LogConsole($"OnExistingPurchasesFetchFailed: {failure.Message}");
        }

        #endregion

        #region Purchase

        public void OnPurchasePending(PendingOrder order)
        {
            foreach (var cartItem in order.CartOrdered.Items())
            {
                this.iapLogWrapped.LogCompletedPurchase(cartItem.Product, order.Info);
            }

            // Bản cũ chỉ log kết quả validate rồi grant content bất chấp.
            // Giờ receipt sai = KHÔNG grant, không confirm.
            if (!this.iapServices.IsReceiptValid(order.Info))
            {
                this.iapLogWrapped.LogConsole("[IAP] Receipt invalid - NOT granting content.");
                this.iapServices.ClearCompletePurchase();

                foreach (var cartItem in order.CartOrdered.Items())
                {
                    this.signalBus.Fire(new OnIAPPurchaseFailedSignal(cartItem.Product.definition.storeSpecificId, "InvalidReceipt"));
                }

                return;
            }

            this.iapServices.CacheOrder(order.Info);

            foreach (var cartItem in order.CartOrdered.Items())
            {
                var productId = cartItem.Product.definition.id;

                // Signal thành công — bản cũ KHÔNG có signal này, success chỉ đi qua
                // Action<string> nên mọi listener trên signal bus không nhận được gì.
                this.signalBus.Fire(new OnIAPPurchaseSuccessSignal()
                {
                    ProductId        = productId,
                    PurchasedProduct = cartItem.Product,
                });

                this.iapServices.InvokeCompletePurchase(productId);
            }

            this.iapServices.ConfirmOrderIfAutomatic(order);
        }

        // Đổi tên: bản cũ có 2 overload cùng tên OnPurchaseConfirmed(Order)/(ConfirmedOrder),
        // bind vào event qua overload resolution -> silent rebind nếu Unity đổi delegate.
        public void OnOrderConfirmed(Order order)
        {
            switch (order)
            {
                case FailedOrder failedOrder:
                    this.OnConfirmationFailed(failedOrder);

                    break;
                case ConfirmedOrder confirmedOrder:
                    this.OnOrderConfirmedSuccess(confirmedOrder);

                    break;
            }
        }

        private void OnOrderConfirmedSuccess(ConfirmedOrder order)
        {
            this.iapServices.CacheOrder(order.Info);

            foreach (var cartItem in order.CartOrdered.Items())
            {
                this.iapLogWrapped.LogConfirmedOrder(cartItem.Product, order.Info);
                this.iapServices.InvokeCompletePurchase(cartItem.Product.definition.id);
            }
        }

        private void OnConfirmationFailed(FailedOrder failedOrder)
        {
            var reason = failedOrder.FailureReason;

            this.iapServices.ClearCompletePurchase();

            foreach (var cartItem in failedOrder.CartOrdered.Items())
            {
                this.iapLogWrapped.LogFailedConfirmation(cartItem.Product, reason);
                this.signalBus.Fire(new OnIAPPurchaseFailedSignal(cartItem.Product.definition.storeSpecificId, reason.ToString()));
            }
        }

        public void OnPurchaseFailed(FailedOrder failedOrder)
        {
            var reason = failedOrder.FailureReason;

            // Bản cũ không clear callback khi fail -> callback cũ có thể bị gọi
            // cho lần mua sau, với sai product.
            this.iapServices.ClearCompletePurchase();

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

        #endregion
    }
}
#endif