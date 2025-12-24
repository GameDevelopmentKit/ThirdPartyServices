namespace ServiceImplementation.IAPServices
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using Transactions.Exceptions;
    using Unity.Services.Core;
    using Unity.Services.Core.Environments;
    using UnityEngine;
    using UnityEngine.Purchasing;


    public class UnityIAPHandler : IIapServices
    {
        private StoreController                                    storeController;
        private Dictionary<string, Queue<UniTaskCompletionSource>> pendingPurchaseTask = new Dictionary<string, Queue<UniTaskCompletionSource>>();
        private UniTaskCompletionSource<bool>                      initializeProductsSource;
        private UniTask<bool>                                      initializeProductsTask;
#region Initialization
        public async UniTask Initialize(Dictionary<string, ProductType> iapPacks, string environment = "production")
        {
            await InitializeUnityServices(environment);
            await InitializeUnityIAP(iapPacks);
        }

        private async UniTask InitializeUnityServices(string environment = "production")
        {
            try
            {
                var options = new InitializationOptions().SetEnvironmentName(environment);

                await UnityServices.InitializeAsync(options);

            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private async UniTask InitializeUnityIAP(Dictionary<string, ProductType> iapPacks)
        {
            storeController = UnityIAPServices.StoreController();

            storeController.OnProductsFetched     += OnInitialProductsFetched;
            storeController.OnProductsFetchFailed += OnInitialProductsFetchFailed;

            storeController.OnPurchasePending   += OnPurchasePending;
            storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;
            storeController.OnPurchaseFailed    += OnPurchaseFailed;

            await storeController.Connect();

            InitializeProducts(iapPacks);
        }
#endregion

#region Product Fetching
        void InitializeProducts(Dictionary<string, ProductType> iapPacks)
        {
            initializeProductsSource = new UniTaskCompletionSource<bool>();
            initializeProductsTask   = initializeProductsSource.Task.Preserve();

            var initialProductsToFetch = iapPacks.Select(kvp =>
            {
                UnityEngine.Purchasing.ProductType productType = kvp.Value switch
                {
                    ProductType.Consumable => UnityEngine.Purchasing.ProductType.Consumable,
                    ProductType.NonConsumable => UnityEngine.Purchasing.ProductType.NonConsumable,
                    ProductType.Subscription => UnityEngine.Purchasing.ProductType.Subscription,
                    _ => UnityEngine.Purchasing.ProductType.Consumable
                };

                return new ProductDefinition(kvp.Key, productType);
            }).ToList();

            storeController.FetchProducts(initialProductsToFetch);
        }

        void OnInitialProductsFetched(List<Product> products)
        {
            LogWithColor("Initial products fetched:", "green");
            foreach (var product in products)
            {
                LogWithColor($"Product ID: {product.definition.id}, Type: {product.definition.type}, Price: {product.metadata.localizedPriceString}");
            }
            initializeProductsSource.TrySetResult(true);
        }

        void OnInitialProductsFetchFailed(ProductFetchFailed failure)
        {
            LogWithColor($"Initial products fetch failed: {failure.FailureReason}", "red");
            initializeProductsSource.TrySetResult(false);
        }
#endregion

#region Purchase Handling
        public UniTask PurchaseProduct(string productId)
        {
            var tcs     = new UniTaskCompletionSource();
            var product = this.FindProduct(productId);

            if (product != null)
            {
                if (!pendingPurchaseTask.TryGetValue(productId, out var queue))
                {
                    queue                          = new Queue<UniTaskCompletionSource>();
                    pendingPurchaseTask[productId] = queue;
                }
                queue.Enqueue(tcs);
                storeController?.PurchaseProduct(product);
            }
            else
            {
                tcs.TrySetException(new IAPPurchaseFailedException($"The product service has no product with the ID {productId}"));
            }
            return tcs.Task;
        }

        void OnPurchasePending(PendingOrder order)
        {
            // Add your validations here before confirming the purchase.
            bool hasPendingOrderValid = false;
            foreach (var cartItem in order.CartOrdered.Items())
            {
                var product = cartItem.Product;

                LogWithColor($"Purchased Product: '{product.definition.id}' \n" +
                             $"Product transaction id: {order.Info.TransactionID}. \n" +
                             $"Product receipt length: {order.Info.Receipt}.\n" +
                             $"Product Type: '{product.definition.type}'");

                if (pendingPurchaseTask.TryGetValue(product.definition.id, out var queue) && queue.Count > 0)
                {
                    var tcs = queue.Dequeue();
                    tcs.TrySetResult();
                    hasPendingOrderValid = true;
                }
            }

            if (!hasPendingOrderValid)
            {
                LogWithColor($"No pending purchase task found for the order. Order ID: {order.Info.TransactionID}", "red");
                return;
            }
            storeController.ConfirmPurchase(order);
        }

        void OnPurchaseConfirmed(Order order)
        {
            switch (order)
            {
                case FailedOrder failedOrder:
                    LogWithColor($"Purchase confirmation failed: {failedOrder.CartOrdered.Items().First().Product.definition.id}, {failedOrder.FailureReason.ToString()}, {failedOrder.Details}");
                    break;
                case ConfirmedOrder:
                    LogWithColor($"Purchase completed:  {order.CartOrdered.Items().First().Product.definition.id}");
                    break;
            }
        }

        void OnPurchaseFailed(FailedOrder failedOrder)
        {
            foreach (var cartItem in failedOrder.CartOrdered.Items())
            {
                var product = cartItem.Product;

                LogWithColor($"Purchase Failed for Product: '{product.definition.id}' \n" +
                             $"FailureReason: {failedOrder.FailureReason.ToString()}.");

                if (pendingPurchaseTask.TryGetValue(product.definition.id, out var queue) && queue.Count > 0)
                {
                    var tcs = queue.Dequeue();
                    tcs.TrySetException(new IAPPurchaseFailedException($"Purchase failed for product ID {product.definition.id} with reason {failedOrder.FailureReason.ToString()}"));
                }
            }
        }
#endregion

#region Utilities
        public bool IsProductAvailable(string productId)
        {
            if (this.storeController == null)
                return false;

            var product = this.FindProduct(productId);

            return product != null && product.availableToPurchase;
        }


        public Product FindProduct(string productId)
        {
            return GetFetchedProducts()?.FirstOrDefault(product => product.definition.id == productId);
        }

        private ReadOnlyObservableCollection<Product> GetFetchedProducts()
        {
            return storeController?.GetProducts();
        }

        public async UniTask<Product> FetchProduct(string productId)
        {
            await initializeProductsTask;
            var product = this.FindProduct(productId);

            return product ?? throw new Exception($"Product with ID {productId} not found after initialization.");
        }
        
        public string GetLocalizedPriceString(string productId, string defaultValue = "")
        {
            var product = this.FindProduct(productId);
            return product != null ? product.metadata.localizedPriceString : defaultValue;
        }

        public void LogWithColor(string logContent, string c = null, string header = "[Unity IAP]")
        {
            var color = string.IsNullOrEmpty(c) ? "white" : c;
            Debug.Log($"<color={color}>{header} {logContent}</color>");
        }
#endregion
    }
}
