#if IAP_5_OR_NEWER

namespace ServiceImplementation.IAPServices.IAP5orNewer
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using ServiceImplementation.IAPServices.Iap5Below;
    using ServiceImplementation.IAPServices.Signals;
    using Unity.Services.Core;
    using Unity.Services.Core.Environments;
    using UnityEngine;
    using UnityEngine.Purchasing;
    using UnityEngine.Purchasing.Extension;
    using UnityEngine.Purchasing.Security;
    using Zenject;
    using ProductType = ProductType;

    public class Iap5OrNewerServices : IIapServices, IInitializable
    {
        private readonly IapLogWrapped                iapLogWrapped;
        private readonly ISignalBus                   signalBus;
        private          List<string>                 ownedProducts = new();
        private          bool                         isInitedIap;
        private          Dictionary<string, IAPModel> iapPack;

        public Iap5OrNewerServices(IapLogWrapped iapLogWrapped, ISignalBus signalBus)
        {
            this.iapLogWrapped = iapLogWrapped;
            this.signalBus     = signalBus;
        }

        private IStoreService    mStoreService;
        private IProductService  mProductService;
        private IPurchaseService mPurchasingService;

        private         ICatalogProvider               mCatalogProvider = new CatalogProvider();
        public          Action<string>                 OnCompletePurchase { get; set; } = null;
        private         CrossPlatformValidator         mCrossPlatformValidator;
        private         IAPPaywallCallbacks            mIAPPaywallCallbacks;
        public readonly Dictionary<string, IOrderInfo> CachedOrders = new();

        public void Initialize()
        {
            this.mIAPPaywallCallbacks = new IAPPaywallCallbacks(this, this.iapLogWrapped, this.signalBus);
            this.CreateServices();
        }

        public async void InitIapServices(Dictionary<string, IAPModel> iapPack, string environment = "production")
        {
            this.iapPack = iapPack;
            this.InitCatalog(iapPack);

            try
            {
                var options = new InitializationOptions()
                    .SetEnvironmentName(environment);

                await UnityServices.InitializeAsync(options);

                this.OnInited();
            }
            catch (Exception exception)
            {
                this.OnError(exception.Message);
            }

            this.CreateCrossPlatformValidator();
            this.ConnectToStore();
        }

        private async void ConnectToStore()
        {
            await this.mStoreService.Connect();
            this.iapLogWrapped.LogConsole("===========");
            this.iapLogWrapped.LogConsole("Store Connected.");
            this.FetchInitialProducts();
        }

        private void FetchInitialProducts() { this.mCatalogProvider.FetchProducts(this.mProductService.FetchProductsWithNoRetries, DefaultStoreHelper.GetDefaultStoreName()); }

        private void CreateServices()
        {
            this.mStoreService      = UnityIAPServices.DefaultStore();
            this.mProductService    = UnityIAPServices.DefaultProduct();
            this.mPurchasingService = UnityIAPServices.DefaultPurchase();

            this.ConfigureServiceCallbacks();
        }

        private void CreateCrossPlatformValidator()
        {
#if !UNITY_EDITOR
            try
            {
                if (this.CanCrossPlatformValidate())
                {
#if !DEBUG_STOREKIT_TEST
                    this.mCrossPlatformValidator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);
#else
               this.mCrossPlatformValidator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleStoreKitTestTangle.Data(), Application.identifier);
#endif
                }
            }
            catch (NotImplementedException exception)
            {
               this.iapLogWrapped.LogConsole("===========");
               this.iapLogWrapped.LogConsole($"Cross Platform Validator Not Implemented: {exception}");
            }
#endif
        }

        private void ConfigureServiceCallbacks()
        {
            this.mProductService.OnProductsFetched         += this.mIAPPaywallCallbacks.OnInitialProductsFetched;
            this.mProductService.OnProductsFetchFailed     += this.mIAPPaywallCallbacks.OnInitialProductsFetchFailed;
            this.mPurchasingService.OnPurchasesFetched     += this.mIAPPaywallCallbacks.OnExistingPurchasesFetched;
            this.mPurchasingService.OnPurchasesFetchFailed += this.mIAPPaywallCallbacks.OnExistingPurchasesFetchFailed;
            this.mPurchasingService.OnPurchasePending      += this.mIAPPaywallCallbacks.OnPurchasePending;
            this.mPurchasingService.OnPurchaseConfirmed    += this.mIAPPaywallCallbacks.OnPurchaseConfirmed;
            this.mPurchasingService.OnPurchaseFailed       += this.mIAPPaywallCallbacks.OnPurchaseFailed;
            this.mPurchasingService.OnPurchaseDeferred     += this.mIAPPaywallCallbacks.OnOrderDeferred;
        }

        public void OnPurchaseConfirmed(IOrderInfo orderInfo) { }

        public bool IsReceiptAvailable(Orders existingOrders)
        {
            if (existingOrders != null)
            {
                foreach (var confirmedOrder in existingOrders.ConfirmedOrders)
                {
                    if (string.IsNullOrEmpty(confirmedOrder.Info.Receipt)) continue;

                    foreach (var purchasedProductInfo in confirmedOrder.Info.PurchasedProductInfo)
                    {
                        if (this.ownedProducts.Contains(purchasedProductInfo.productId)) continue;
                        this.ownedProducts.Add(purchasedProductInfo.productId);
                        this.iapLogWrapped.LogConsole($"Adding owned product: {purchasedProductInfo.productId}");
                    }
                }
            }

            return existingOrders != null &&
                   (existingOrders.ConfirmedOrders.Any(order => !string.IsNullOrEmpty(order.Info.Receipt)) ||
                    existingOrders.PendingOrders.Any(order => !string.IsNullOrEmpty(order.Info.Receipt)));
        }

        private bool IsGooglePlay() { return Application.platform == RuntimePlatform.Android && DefaultStoreHelper.GetDefaultStoreName() == GooglePlay.Name; }

        private bool CanCrossPlatformValidate()
        {
            return this.IsGooglePlay() ||
                   Application.platform == RuntimePlatform.IPhonePlayer ||
                   Application.platform == RuntimePlatform.OSXPlayer ||
                   Application.platform == RuntimePlatform.tvOS;
        }

        public void ValidatePurchaseIfPossible(IOrderInfo orderInfo)
        {
            if (this.CanCrossPlatformValidate())
            {
                this.ValidatePurchase(orderInfo);
            }
        }

        private void ValidatePurchase(IOrderInfo orderInfo)
        {
            try
            {
                var result = this.mCrossPlatformValidator.Validate(orderInfo.Receipt);

                if (this.IsGooglePlay())
                {
                    this.iapLogWrapped.LogConsole("Validated Receipt. Contents:");

                    foreach (var productReceipt in result)
                    {
                        this.iapLogWrapped.LogReceiptValidation(productReceipt);
                    }
                }
                else
                {
                    this.iapLogWrapped.LogConsole("Validated Receipt.");
                }
            }
            catch (IAPSecurityException ex)
            {
                this.iapLogWrapped.LogConsole("Invalid receipt, not unlocking content. " + ex);
            }
        }

        private void OnError(string exceptionMessage)
        {
            this.isInitedIap = false;
            this.iapLogWrapped.LogConsole($"IAP Failed: {exceptionMessage}");
        }

        private void OnInited()
        {
            this.isInitedIap = true;
            this.iapLogWrapped.LogConsole("IAP Services Initialized Successfully");
        }

        private void InitCatalog(Dictionary<string, IAPModel> iapPack)
        {
            var initialProductsToFetch = new List<ProductDefinition>();

            foreach (var (key, model) in iapPack)
            {
                initialProductsToFetch.Add(new ProductDefinition(model.Id, this.ConvertToUnityProductType(model.ProductType)));
            }

            this.mCatalogProvider.AddProducts(initialProductsToFetch);
        }

        private UnityEngine.Purchasing.ProductType ConvertToUnityProductType(ProductType productType)
        {
            return productType switch
            {
                ProductType.Consumable => UnityEngine.Purchasing.ProductType.Consumable,
                ProductType.Subscription => UnityEngine.Purchasing.ProductType.Subscription,
                ProductType.NonConsumable => UnityEngine.Purchasing.ProductType.NonConsumable,
                _ => UnityEngine.Purchasing.ProductType.Consumable
            };
        }

        public void BuyProductID(string productId, Action<string> onComplete = null, Action<string> onFailed = null)
        {
            this.signalBus.Fire(new OnStartDoingIAPSignal());
            var product = this.FindProduct(productId);

            if (product != null)
            {
                this.OnCompletePurchase = onComplete;
                this.mPurchasingService?.PurchaseProduct(product);
            }
            else
            {
                onFailed?.Invoke($"Product with ID {productId} not found.");
                this.iapLogWrapped.LogConsole($"The product service has no product with the ID {productId}");
            }
        }

        private Product                               FindProduct(string productId) { return this.GetFetchedProducts()?.FirstOrDefault(product => product.definition.id == productId); }
        private ReadOnlyObservableCollection<Product> GetFetchedProducts()          { return this.mProductService?.GetProducts(); }

        public string GetPriceById(string productId, string defaultPrice)
        {
            var s = defaultPrice;

            if (!this.isInitedIap)
            {
                return s;
            }

            try
            {
                s = this.GetFetchedProducts().First(x => x.definition.id.Equals(productId)).metadata.localizedPriceString;

                if (string.IsNullOrWhiteSpace(s))
                {
                    s = defaultPrice;
                }
            }
            catch (Exception e)
            {
                this.iapLogWrapped.LogConsole($"GetPriceById {e.Message}");
            }

            return s;
        }

        public void RestorePurchases(Action onComplete)
        {
            this.signalBus.Fire<OnStartDoingIAPSignal>();

            this.mPurchasingService.RestoreTransactions((b, e) =>
            {
                if (b)
                {
                    this.FetchExistingPurchases();

                    onComplete?.Invoke();
                }
            });
        }

        public bool IsProductOwned(string productId) { return !string.IsNullOrEmpty(productId) && this.ownedProducts.Contains(productId); }

        public ProductData GetProductData(string productId)
        {
            var product = this.GetFetchedProducts().First(x => x.definition.id.Equals(productId));

            return new ProductData()
            {
                Id           = productId,
                Price        = product.metadata.localizedPrice,
                CurrencyCode = product.metadata.isoCurrencyCode
            };
        }

        public bool IsSubscriptionActive(string productId)
        {
            if (!this.CachedOrders.TryGetValue(productId, out var orderInfo))
                return false;

            if (string.IsNullOrEmpty(orderInfo.Receipt))
                return false;

            try
            {
                var receipts = this.mCrossPlatformValidator.Validate(orderInfo.Receipt);

                foreach (var r in receipts)
                {
                    if (r.productID != productId) continue;

                    if (r is AppleInAppPurchaseReceipt apple)
                    {
                        return apple.subscriptionExpirationDate > DateTime.UtcNow &&
                               apple.cancellationDate.Ticks == 0;
                    }

                    if (r is GooglePlayReceipt google)
                    {
                        this.iapLogWrapped.LogConsole(
                            $"[IAP] Google sub receipt for {productId}, token={google.purchaseToken}. " +
                            "Use server validation to check expiryTimeMillis."
                        );
                        return true; // tạm coi active, production verify server-side
                    }
                }
            }
            catch (IAPSecurityException ex)
            {
                this.iapLogWrapped.LogConsole($"[IAP] Invalid receipt for {productId}: {ex}");
            }

            return false;
        }


        public void FetchExistingPurchases() { this.mPurchasingService.FetchPurchases(); }

        public void ConfirmOrderIfAutomatic(PendingOrder order)
        {
            if (this.ShouldConfirmOrderAutomatically(order))
            {
                this.ConfirmOrder(order);
            }
        }

        private void ConfirmOrder(PendingOrder pendingOrder) { this.mPurchasingService.ConfirmPurchase(pendingOrder); }

        private bool ShouldConfirmOrderAutomatically(PendingOrder order)
        {
            var containsItemToAutoConfirm = false;

            foreach (var cartItem in order.CartOrdered.Items())
            {
                if (!this.iapPack.TryGetValue(cartItem.Product.definition.id, out var model)) continue;

                if (model.ProductType == ProductType.Consumable)
                {
                    containsItemToAutoConfirm = true;
                }
            }

            return containsItemToAutoConfirm;
        }
    }
}
#endif