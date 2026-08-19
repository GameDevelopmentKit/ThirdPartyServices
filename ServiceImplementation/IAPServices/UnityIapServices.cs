#if THEONE_IAP
namespace ServiceImplementation.IAPServices
{
    #if UNITY_ANDROID && !UNITY_EDITOR
    using UnityEngine.Purchasing.Security;
    #endif
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GameFoundation.Signals;
    using ServiceImplementation.IAPServices.Signals;
    using TheOne.Logging;
    using Unity.Services.Core;
    using Unity.Services.Core.Environments;
    using UnityEngine;
    using UnityEngine.Purchasing;
    using UnityEngine.Scripting;
    using ILogger = TheOne.Logging.ILogger;

    public class UnityIapServices : IIapServices
    {
        private Action<string, int>          onPurchaseComplete;
        private Action<string>               onPurchaseFailed;
        private Action                       onRestoreComplete;
        private StoreController              storeController;
        private Dictionary<string, IAPModel> iapPacks;

        private readonly HashSet<string> ownedProductIds = new();

        private bool IsInitialized => this.storeController != null;

        #region inject

        private readonly ILogger   logger;
        private readonly SignalBus signalBus;

        [Preserve]
        public UnityIapServices(ILoggerManager loggerManager, SignalBus signalBus)
        {
            this.logger    = loggerManager.GetLogger(this);
            this.signalBus = signalBus;
        }

        #endregion

        public async void InitIapServices(Dictionary<string, IAPModel> iapPack, string environment = "production")
        {
            if (this.IsInitialized) return;
            this.iapPacks = iapPack;

            try
            {
                await UnityServices.InitializeAsync(new InitializationOptions().SetEnvironmentName(environment));
            }
            catch (Exception exception)
            {
                this.logger.Error($"UnityServices init failed {exception.Message}");
            }

            try
            {
                await this.InitializePurchasing();
            }
            catch (Exception exception)
            {
                this.logger.Error($"InitializePurchasing failed {exception.Message}");
            }
        }

        private async System.Threading.Tasks.Task InitializePurchasing()
        {
            if (this.IsInitialized) return;

            this.storeController = UnityIAPServices.StoreController();

            this.storeController.OnPurchasePending   += this.OnPurchasePending;
            this.storeController.OnPurchaseConfirmed += this.OnPurchaseConfirmed;
            this.storeController.OnPurchaseFailed    += this.OnPurchaseFailed;
            this.storeController.OnPurchasesFetched  += this.OnPurchasesFetched;
            this.storeController.OnStoreDisconnected += this.OnStoreDisconnected;
            this.storeController.ProcessPendingOrdersOnPurchasesFetched(true);

            this.logger.Info("Connecting to store");
            await this.storeController.Connect();

            this.storeController.OnProductsFetched     += this.OnProductsFetched;
            this.storeController.OnProductsFetchFailed += this.OnProductsFetchFailed;

            this.logger.Info("Store connected successfully");
            this.storeController.FetchProducts(this.CreateProductDefinitions());
        }

        #region IAP Events

        private void OnPurchasePending(PendingOrder order)
        {
            var isPurchaseValid = this.IsPurchaseValid(order);

            foreach (var cartItem in order.CartOrdered.Items())
            {
                var productId = cartItem.Product.definition.id;
                var quantity  = cartItem.Quantity;

                if (isPurchaseValid)
                {
                    this.MarkOwnedIfNonConsumable(cartItem.Product);

                    if (this.onPurchaseComplete != null)
                    {
                        this.signalBus.Fire(new OnIAPPurchaseSuccessSignal(this.GetProductData(productId), quantity));
                        this.onPurchaseComplete.Invoke(productId, quantity);
                    }
                    else
                    {
                        this.signalBus.Fire(new OnRestorePurchaseCompleteSignal(productId, quantity));
                        this.onRestoreComplete?.Invoke();
                    }

                    this.logger.Info($"Purchase SUCCESS. Product: '{productId}', quantity: {quantity}");
                }
                else
                {
                    this.onPurchaseFailed?.Invoke(productId);
                    this.signalBus.Fire(new OnIAPPurchaseFailedSignal(productId, "Receipt validation invalid"));
                    this.logger.Info($"Purchase FAIL. Product: '{productId}', quantity: {quantity}, Reason: Receipt validation invalid");
                }
            }

            if (isPurchaseValid)
            {
                if (this.onPurchaseComplete == null) this.onRestoreComplete = null;
                this.onPurchaseComplete = null;
            }
            else
            {
                this.onPurchaseFailed = null;
            }

            this.storeController.ConfirmPurchase(order);
        }

        private void OnPurchaseConfirmed(Order order)
        {
            foreach (var cartItem in order.CartOrdered.Items())
            {
                this.logger.Info($"Purchase confirmed - Product: {cartItem.Product.definition.id}, quantity: {cartItem.Quantity}");
            }
        }

        private void OnPurchaseFailed(FailedOrder order)
        {
            var reason = order.FailureReason.ToString();

            foreach (var cartItem in order.CartOrdered.Items())
            {
                var productId = cartItem.Product.definition.id;
                this.onPurchaseFailed?.Invoke(productId);
                this.signalBus.Fire(new OnIAPPurchaseFailedSignal(productId, reason));
                this.logger.Info($"Purchase FAIL. Product: '{productId}', Reason: {reason}, Details: {order.Details}");
            }

            this.onPurchaseFailed = null;
        }

        private void OnPurchasesFetched(Orders orders)
        {
            foreach (var pendingOrder in orders.PendingOrders)
            {
                this.OnPurchasePending(pendingOrder);
            }

            foreach (var confirmedOrder in orders.ConfirmedOrders)
            {
                foreach (var cartItem in confirmedOrder.CartOrdered.Items())
                {
                    if (!this.MarkOwnedIfNonConsumable(cartItem.Product)) continue;

                    this.signalBus.Fire(new OnRestorePurchaseCompleteSignal(cartItem.Product.definition.id, cartItem.Quantity));
                    this.logger.Info($"Auto-restored product: {cartItem.Product.definition.id} (type: {cartItem.Product.definition.type})");
                }
            }
        }

        private void OnStoreDisconnected(StoreConnectionFailureDescription description)
        {
            this.logger.Error($"Store disconnected - Details: {description.message}");
        }

        private void OnProductsFetched(List<Product> products)
        {
            this.logger.Info($"Products fetched successfully - {products.Count} products");
            this.storeController.FetchPurchases();
        }

        private void OnProductsFetchFailed(ProductFetchFailed failure)
        {
            this.logger.Error($"Products fetch failed for {failure.FailedFetchProducts.Count} products - Reason: {failure.FailureReason}");
        }

        #endregion

        public string GetPriceById(string productId, string defaultPrice = "")
        {
            if (!this.IsInitialized || this.storeController.GetProductById(productId) is not { } product) return defaultPrice;

            var localizedPrice = product.metadata.localizedPriceString;
            return string.IsNullOrWhiteSpace(localizedPrice) ? defaultPrice : localizedPrice;
        }

        public void BuyProductID(string productId, Action<string, int> onComplete, Action<string> onFailed = null)
        {
            if (!this.IsInitialized)
            {
                this.InitializePurchasing().ContinueWith(_ => { });
                onFailed?.Invoke(productId);
                this.logger.Info("FAIL. Not initialized.");
                return;
            }

            this.signalBus.Fire<OnStartDoingIAPSignal>();

            if (this.storeController.GetProductById(productId) is { availableToPurchase: true } product)
            {
                this.logger.Info($"Purchasing product asynchronously: '{product.definition.id}'");

                this.onPurchaseComplete = onComplete;
                this.onPurchaseFailed   = onFailed;
                this.storeController.PurchaseProduct(product);
            }
            else
            {
                onFailed?.Invoke(productId);
                this.logger.Info("FAIL. Not purchasing product, either is not found or is not available for purchase");
            }
        }

        public void RestorePurchases(Action onComplete = null, Action onFailed = null)
        {
            if (!this.IsInitialized)
            {
                this.logger.Info("FAIL. Not initialized.");
                onFailed?.Invoke();
                return;
            }

            if (Application.platform is not (RuntimePlatform.IPhonePlayer or RuntimePlatform.OSXPlayer))
            {
                this.logger.Info("FAIL. Not supported on this platform. Current = " + Application.platform);
                onFailed?.Invoke();
                return;
            }

            this.signalBus.Fire<OnStartDoingIAPSignal>();
            this.logger.Info("started ...");
            this.onRestoreComplete = onComplete;

            this.storeController.RestoreTransactions((result, _) =>
            {
                this.logger.Info("Is Restore success: " + result);

                if (result) return;
                onFailed?.Invoke();
                this.onRestoreComplete = null;
            });
        }

        public bool IsProductOwned(string productId)
        {
            return !string.IsNullOrEmpty(productId) && this.ownedProductIds.Contains(productId);
        }

        public ProductData GetProductData(string productId)
        {
            if (this.IsInitialized && this.storeController.GetProductById(productId) is { } product)
            {
                return new()
                {
                    Id           = productId,
                    Price        = product.metadata.localizedPrice,
                    CurrencyCode = product.metadata.isoCurrencyCode,
                };
            }

            return new() { Id = productId };
        }

        #region Helper

        private bool MarkOwnedIfNonConsumable(Product product)
        {
            if (product.definition.type is not (UnityEngine.Purchasing.ProductType.NonConsumable or UnityEngine.Purchasing.ProductType.Subscription)) return false;

            return this.ownedProductIds.Add(product.definition.id);
        }

        private bool IsPurchaseValid(Order order)
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                // 1. Open the obfuscation window from Services > In-App Purchasing > Receipt Validation Obfuscator
                // 2. Paste your Google Play secret key. (Copy the key from the "Monetize with Play/Monetization setup/Licensing" section on the Google Play Developer Console)
                // 3. Obfuscate the key. (Auto generate GooglePlayTangle classes in your project.)

                if (!GooglePlayTangle.IsPopulated)
                {
                    this.logger.Warning("Receipt validation skipped: GooglePlayTangle holds no key, run the Receipt Validation Obfuscator");
                    return true;
                }

                var validator = new CrossPlatformValidator(GooglePlayTangle.Data(), Application.identifier);
                var result    = validator.Validate(order.Info.Receipt);

                this.logger.Info("Google Play receipt validated successfully");
                foreach (var productReceipt in result)
                {
                    this.logger.Info($"Product: {productReceipt.productID}, Transaction: {productReceipt.transactionID}");
                }

                return true;
            }
            catch (IAPSecurityException ex)
            {
                this.logger.Error($"Google Play receipt validation failed, please setup Receipt Validation Obfuscator - {ex.Message}");
                return false;
            }
            #else
            this.logger.Info("Receipt validation skipped (Apple validation deprecated in IAP v5)");
            return true;
            #endif
        }

        private List<ProductDefinition> CreateProductDefinitions()
        {
            return this.iapPacks.Select(pair => new ProductDefinition(pair.Value.Id, ConvertToUnityProductType(pair.Value.ProductType))).ToList();

            static UnityEngine.Purchasing.ProductType ConvertToUnityProductType(ProductType productType)
            {
                return productType switch
                {
                    ProductType.Consumable    => UnityEngine.Purchasing.ProductType.Consumable,
                    ProductType.Subscription  => UnityEngine.Purchasing.ProductType.Subscription,
                    ProductType.NonConsumable => UnityEngine.Purchasing.ProductType.NonConsumable,
                    _                         => UnityEngine.Purchasing.ProductType.Consumable,
                };
            }
        }

        #endregion
    }
}
#endif
