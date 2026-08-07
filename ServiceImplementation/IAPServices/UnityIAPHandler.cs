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


    public class UnityIAPHandler : IIapServices, IDisposable
    {
        private const string GooglePlayStoreName = "GooglePlay";
        private const string AppleAppStoreName   = "AppleAppStore";
        private const string MacAppStoreName     = "MacAppStore";

        public event Action<Order> OnPurchaseConfirmed;

        private readonly IIapReceiptValidationService receiptValidationService;

        private StoreController storeController;

        private Dictionary<string, Queue<UniTaskCompletionSource>> pendingPurchaseTask =
            new Dictionary<string, Queue<UniTaskCompletionSource>>();

        private UniTaskCompletionSource<bool> initializeProductsSource;
        private UniTask<bool> initializeProductsTask;

        public UnityIAPHandler(IIapReceiptValidationService receiptValidationService)
        {
            this.receiptValidationService = receiptValidationService;
        }

        #region Initialization

        public async UniTask Initialize(Dictionary<string, ProductType> iapPacks)
        {
            await InitializeUnityServices();
            await InitializeUnityIAP(iapPacks);
        }

        private async UniTask InitializeUnityServices(string environment = "production")
        {
            try
            {
                if (UnityServices.State != ServicesInitializationState.Initialized)
                {
                    var options = new InitializationOptions().SetEnvironmentName(environment);

                    await UnityServices.InitializeAsync(options);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private async UniTask InitializeUnityIAP(Dictionary<string, ProductType> iapPacks)
        {
            var storeName = GetUnityIAPStoreName();
            storeController = string.IsNullOrEmpty(storeName)
                ? UnityIAPServices.StoreController()
                : UnityIAPServices.StoreController(storeName);

            storeController.OnProductsFetched += OnInitialProductsFetched;
            storeController.OnProductsFetchFailed += OnInitialProductsFetchFailed;

            storeController.OnPurchasePending += OnPurchasePending;
            storeController.OnPurchaseConfirmed += HandlePurchaseConfirmed;
            storeController.OnPurchaseFailed += OnPurchaseFailed;
            storeController.OnPurchaseDeferred += OnPurchaseDeferred;

            await storeController.Connect();

            InitializeProducts(iapPacks);
        }
        
        public void Dispose()
        {
            if (storeController == null)
                return;
            storeController.OnProductsFetched     -= OnInitialProductsFetched;
            storeController.OnProductsFetchFailed -= OnInitialProductsFetchFailed;

            storeController.OnPurchasePending   -= OnPurchasePending;
            storeController.OnPurchaseConfirmed -= HandlePurchaseConfirmed;
            storeController.OnPurchaseFailed    -= OnPurchaseFailed;
            storeController.OnPurchaseDeferred  -= OnPurchaseDeferred;
        }

        private string GetUnityIAPStoreName()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return GooglePlayStoreName;
#elif UNITY_IOS && !UNITY_EDITOR
            return AppleAppStoreName;
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
            return MacAppStoreName;
#else
            return null;
#endif
        }

        #endregion

        #region Product Fetching

        void InitializeProducts(Dictionary<string, ProductType> iapPacks)
        {
            initializeProductsSource = new UniTaskCompletionSource<bool>();
            initializeProductsTask = initializeProductsSource.Task.Preserve();

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
                LogWithColor(
                    $"Product ID: {product.definition.id}, Type: {product.definition.type}, Price: {product.metadata.localizedPriceString}");
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
            var tcs = new UniTaskCompletionSource();
            var product = this.FindProduct(productId);

            if (product != null)
            {
                if (!pendingPurchaseTask.TryGetValue(productId, out var queue))
                {
                    queue = new Queue<UniTaskCompletionSource>();
                    pendingPurchaseTask[productId] = queue;
                }

                queue.Enqueue(tcs);
                storeController?.PurchaseProduct(product);
            }
            else
            {
                tcs.TrySetException(
                    new IAPPurchaseFailedException($"The product service has no product with the ID {productId}"));
            }

            return tcs.Task;
        }

        void OnPurchasePending(PendingOrder order)
        {
            this.HandlePurchasePending(order).Forget();
        }

        private async UniTask HandlePurchasePending(PendingOrder order)
        {
            IapReceiptValidationResult validationResult;
            try
            {
                var validationRequest = this.CreateValidationRequest(order);
                validationResult = await this.receiptValidationService.ValidateAsync(validationRequest);
            }
            catch (Exception exception)
            {
                this.FailPendingPurchaseTasks(order, $"Receipt validation failed unexpectedly: {exception.Message}");
                Debug.LogException(exception);
                return;
            }

            if (!validationResult.IsValid)
            {
                this.FailPendingPurchaseTasks(order,
                    $"Receipt validation {validationResult.Status} via '{validationResult.ProviderId}': {validationResult.Error}");
                return;
            }

            bool hasPendingOrderValid = false;
            foreach (var cartItem in order.CartOrdered.Items())
            {
                var product = cartItem.Product;

                LogWithColor($"Purchased Product: '{product.definition.id}' \n" +
                             $"Product transaction id: {order.Info.TransactionID}. \n" +
                             $"Product receipt length: {order.Info.Receipt?.Length ?? 0}.\n" +
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
                LogWithColor($"No pending purchase task found for the order. Order ID: {order.Info.TransactionID}",
                    "red");
                return;
            }

            storeController.ConfirmPurchase(order);
        }

        private IapReceiptValidationRequest CreateValidationRequest(PendingOrder order)
        {
            var products = order.CartOrdered.Items().Select(cartItem =>
            {
                var productType = cartItem.Product.definition.type switch
                {
                    UnityEngine.Purchasing.ProductType.NonConsumable => ProductType.NonConsumable,
                    UnityEngine.Purchasing.ProductType.Subscription => ProductType.Subscription,
                    _ => ProductType.Consumable
                };

                return new IapReceiptProduct(cartItem.Product.definition.id, productType, cartItem.Quantity);
            }).ToList();

            return new IapReceiptValidationRequest(order.Info?.TransactionID, order.Info?.Receipt, products);
        }

        private void FailPendingPurchaseTasks(PendingOrder order, string error)
        {
            foreach (var cartItem in order.CartOrdered.Items())
            {
                var product = cartItem.Product;
                if (pendingPurchaseTask.TryGetValue(product.definition.id, out var queue) && queue.Count > 0)
                {
                    queue.Dequeue().TrySetException(new IAPPurchaseFailedException(error));
                }
            }
        }

        void HandlePurchaseConfirmed(Order order)
        {
            switch (order)
            {
                case FailedOrder failedOrder:
                    LogWithColor(
                        $"Purchase confirmation failed: {failedOrder.CartOrdered.Items().First().Product.definition.id}, {failedOrder.FailureReason.ToString()}, {failedOrder.Details}");
                    break;
                case ConfirmedOrder:
                    LogWithColor($"Purchase completed:  {order.CartOrdered.Items().First().Product.definition.id}");
                    break;
            }

            OnPurchaseConfirmed?.Invoke(order);
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
                    tcs.TrySetException(new IAPPurchaseFailedException(
                        $"Purchase failed for product ID {product.definition.id} with reason {failedOrder.FailureReason.ToString()}"));
                }
            }
        }
        
        void OnPurchaseDeferred(DeferredOrder order)
        {
            var product = order.CartOrdered.Items().FirstOrDefault()?.Product;
            LogWithColor($"Purchase deferred for: {product?.definition.id ?? "unknown"}. " +
                         "Waiting for parental approval.", "yellow");
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

        public decimal GetLocalizedPrice(string productId, decimal defaultValue = 0)
        {
            var product = this.FindProduct(productId);
            return product != null ? product.metadata.localizedPrice : defaultValue;
        }

        public void LogWithColor(string logContent, string c = null, string header = "[Unity IAP]")
        {
            var color = string.IsNullOrEmpty(c) ? "white" : c;
            Debug.Log($"<color={color}>{header} {logContent}</color>");
        }
        
        #endregion

    }
}
