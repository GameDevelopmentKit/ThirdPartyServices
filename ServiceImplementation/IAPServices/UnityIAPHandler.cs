namespace ServiceImplementation.IAPServices
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using Transactions.Exceptions;
    using Unity.Services.Core;
    using Unity.Services.Core.Environments;
    using UnityEngine;
    using UnityEngine.Purchasing;
    using Zenject;


    public class UnityIAPHandler : IIapServices, IDisposable
    {
        private const string GooglePlayStoreName = "GooglePlay";
        private const string AppleAppStoreName   = "AppleAppStore";
        private const string MacAppStoreName     = "MacAppStore";

        // Backoff for parked orders (validation/fulfilment/confirmation retries). The last value repeats.
        private static readonly int[] RetryDelaysSeconds = { 5, 15, 30, 60, 120, 300 };

        public event Action<Order> OnPurchaseConfirmed;

        private readonly IIapPurchaseFulfiller purchaseFulfiller;
        private readonly IapOrderProcessor     orderProcessor;

        private StoreController storeController;

        private UniTaskCompletionSource<bool> initializeProductsSource;
        private UniTask<bool> initializeProductsTask;

        private string                  lastRequestedProductId;
        private CancellationTokenSource retryLoopCts;
        private bool                    isDisposed;

        public UnityIAPHandler(IIapReceiptValidationService receiptValidationService,
            [InjectOptional] IIapPurchaseFulfiller purchaseFulfiller = null)
        {
            this.purchaseFulfiller = purchaseFulfiller;
            this.orderProcessor = new IapOrderProcessor(
                receiptValidationService,
                purchaseFulfiller,
                this.ConfirmOrder,
                message => LogWithColor(message, "cyan"),
                message => LogWithColor(message, "yellow"));

            this.orderProcessor.OrderParked += this.StartRetryLoop;
            Application.focusChanged         += this.OnApplicationFocusChanged;

            if (this.purchaseFulfiller != null)
            {
                this.purchaseFulfiller.ReadyChanged += this.OnFulfillerReadyChanged;
            }
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
            storeController.OnPurchasesFetched += OnPurchasesFetched;
            storeController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;

            await storeController.Connect();

            InitializeProducts(iapPacks);
        }

        public void Dispose()
        {
            this.isDisposed = true;
            this.StopRetryLoop();
            Application.focusChanged -= this.OnApplicationFocusChanged;
            this.orderProcessor.OrderParked -= this.StartRetryLoop;

            if (this.purchaseFulfiller != null)
            {
                this.purchaseFulfiller.ReadyChanged -= this.OnFulfillerReadyChanged;
            }

            if (storeController == null)
                return;
            storeController.OnProductsFetched     -= OnInitialProductsFetched;
            storeController.OnProductsFetchFailed -= OnInitialProductsFetchFailed;

            storeController.OnPurchasePending      -= OnPurchasePending;
            storeController.OnPurchaseConfirmed    -= HandlePurchaseConfirmed;
            storeController.OnPurchaseFailed       -= OnPurchaseFailed;
            storeController.OnPurchaseDeferred     -= OnPurchaseDeferred;
            storeController.OnPurchasesFetched     -= OnPurchasesFetched;
            storeController.OnPurchasesFetchFailed -= OnPurchasesFetchFailed;
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

            // Unfinished orders from previous sessions are otherwise only re-delivered on the next focus change.
            this.FetchUnfinishedPurchases();
        }

        void OnInitialProductsFetchFailed(ProductFetchFailed failure)
        {
            LogWithColor($"Initial products fetch failed: {failure.FailureReason}", "red");
            initializeProductsSource.TrySetResult(false);
        }

        #endregion

        #region Purchase Fetching

        private void FetchUnfinishedPurchases()
        {
            if (storeController == null) return;

            try
            {
                storeController.FetchPurchases();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        void OnPurchasesFetched(Orders orders)
        {
            // Pending orders among the fetched ones are delivered through OnPurchasePending by Unity IAP.
            LogWithColor("Purchases fetched.", "green");
        }

        void OnPurchasesFetchFailed(PurchasesFetchFailureDescription failure)
        {
            LogWithColor($"Purchases fetch failed: {failure.FailureReason} {failure.Message}", "yellow");
        }

        #endregion

        #region Purchase Handling

        public UniTask PurchaseProduct(string productId)
        {
            var product = this.FindProduct(productId);

            if (product == null)
            {
                return UniTask.FromException(
                    new IAPPurchaseFailedException($"The product service has no product with the ID {productId}"));
            }

            var task = this.orderProcessor.RegisterPurchaseRequest(productId);
            this.lastRequestedProductId = productId;
            storeController?.PurchaseProduct(product);

            return task;
        }

        void OnPurchasePending(PendingOrder order)
        {
            this.orderProcessor.ProcessAsync(this.ToIapOrder(order)).Forget(Debug.LogException);
        }

        private IapOrder ToIapOrder(PendingOrder order)
        {
            var products = order.CartOrdered.Items().Select(cartItem =>
            {
                var productType = cartItem.Product.definition.type switch
                {
                    UnityEngine.Purchasing.ProductType.NonConsumable => ProductType.NonConsumable,
                    UnityEngine.Purchasing.ProductType.Subscription => ProductType.Subscription,
                    _ => ProductType.Consumable
                };

                LogWithColor($"Pending Product: '{cartItem.Product.definition.id}' \n" +
                             $"Product transaction id: {order.Info?.TransactionID}. \n" +
                             $"Product receipt length: {order.Info?.Receipt?.Length ?? 0}.\n" +
                             $"Product Type: '{cartItem.Product.definition.type}'");

                return new IapReceiptProduct(cartItem.Product.definition.id, productType, cartItem.Quantity);
            }).ToList();

            return new IapOrder(order.Info?.TransactionID, order.Info?.Receipt, products, order);
        }

        private void ConfirmOrder(IapOrder order)
        {
            if (storeController == null || order.Handle is not PendingOrder pendingOrder)
            {
                this.orderProcessor.OnConfirmResult(order.TransactionId, false);
                return;
            }

            storeController.ConfirmPurchase(pendingOrder);
        }

        void HandlePurchaseConfirmed(Order order)
        {
            switch (order)
            {
                case FailedOrder failedOrder:
                    LogWithColor(
                        $"Purchase confirmation failed: {failedOrder.CartOrdered.Items().FirstOrDefault()?.Product.definition.id}, {failedOrder.FailureReason.ToString()}, {failedOrder.Details}", "red");
                    this.orderProcessor.OnConfirmResult(order.Info?.TransactionID, false);
                    break;
                case ConfirmedOrder:
                    LogWithColor($"Purchase completed:  {order.CartOrdered.Items().FirstOrDefault()?.Product.definition.id}");
                    this.orderProcessor.OnConfirmResult(order.Info?.TransactionID, true);
                    break;
            }

            OnPurchaseConfirmed?.Invoke(order);
        }

        void OnPurchaseFailed(FailedOrder failedOrder)
        {
            foreach (var cartItem in failedOrder.CartOrdered.Items())
            {
                var productId = cartItem.Product.definition.id;

                LogWithColor($"Purchase Failed for Product: '{productId}' \n" +
                             $"FailureReason: {failedOrder.FailureReason.ToString()}. {failedOrder.Details}", "red");

                // Synchronous failures for an invalid cart report a placeholder product that the store does not know;
                // fall back to the product that was just requested so its purchase request does not hang. A failure
                // for a real product without a request (e.g. a cancelled deferred order) must not fail another purchase.
                if (!this.orderProcessor.HasPurchaseRequest(productId) && this.FindProduct(productId) == null)
                {
                    productId = this.lastRequestedProductId;
                }

                Exception exception = failedOrder.FailureReason == PurchaseFailureReason.DuplicateTransaction
                    ? new IAPDuplicatePurchaseException(
                        $"Product {productId} has an unfinished earlier purchase: {failedOrder.Details}")
                    : new IAPPurchaseFailedException(
                        $"Purchase failed for product ID {productId} with reason {failedOrder.FailureReason.ToString()}");

                this.orderProcessor.FailPurchaseRequest(productId, exception);
            }

            if (failedOrder.FailureReason == PurchaseFailureReason.DuplicateTransaction)
            {
                // Google: the product is still owned by an unconsumed order. Fetch it so it gets granted and consumed.
                this.FetchUnfinishedPurchases();
            }
        }

        void OnPurchaseDeferred(DeferredOrder order)
        {
            foreach (var cartItem in order.CartOrdered.Items())
            {
                var productId = cartItem.Product.definition.id;
                LogWithColor($"Purchase deferred for: {productId}. Waiting for payment/approval.", "yellow");

                // The completed order arrives later through OnPurchasePending and is granted by the fulfiller.
                this.orderProcessor.FailPurchaseRequest(productId,
                    new IAPPurchaseDeferredException($"Purchase of {productId} is pending payment approval."));
            }
        }

        #endregion

        #region Retry

        private void OnApplicationFocusChanged(bool hasFocus)
        {
            if (hasFocus && this.orderProcessor.HasParkedOrders)
            {
                this.orderProcessor.RetryParkedOrdersAsync().Forget(Debug.LogException);
            }
        }

        private void OnFulfillerReadyChanged()
        {
            if (this.purchaseFulfiller.IsReady && this.orderProcessor.HasParkedOrders)
            {
                this.orderProcessor.RetryParkedOrdersAsync().Forget(Debug.LogException);
            }
        }

        private void StartRetryLoop()
        {
            if (this.isDisposed || this.retryLoopCts != null) return;

            this.retryLoopCts = new CancellationTokenSource();
            this.RetryLoopAsync(this.retryLoopCts.Token).Forget(Debug.LogException);
        }

        private void StopRetryLoop()
        {
            this.retryLoopCts?.Cancel();
            this.retryLoopCts?.Dispose();
            this.retryLoopCts = null;
        }

        private async UniTask RetryLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                var attempt = 0;
                while (this.orderProcessor.HasParkedOrders && !cancellationToken.IsCancellationRequested)
                {
                    var delaySeconds = RetryDelaysSeconds[Math.Min(attempt, RetryDelaysSeconds.Length - 1)];
                    await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds), DelayType.Realtime,
                        cancellationToken: cancellationToken);

                    await this.orderProcessor.RetryParkedOrdersAsync();
                    attempt++;
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    this.retryLoopCts?.Dispose();
                    this.retryLoopCts = null;

                    // An order may have been parked after the loop condition was last checked.
                    if (this.orderProcessor.HasParkedOrders) this.StartRetryLoop();
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
