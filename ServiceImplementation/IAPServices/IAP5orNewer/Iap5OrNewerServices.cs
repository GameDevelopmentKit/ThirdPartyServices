#if IAP_5_OR_NEWER

// ============================================================================
//  Iap5OrNewerServices — Unity IAP v5
//
//  RECEIPT VALIDATION theo doc v5 (đã verify):
//    - "Receipt validation for Apple App Store receipts has been deprecated.
//       Receipt validation is supported only for Google Play Store."
//    - "To fetch receipts, use Order.Info.Receipt instead of Product.Receipt."
//    - Local validation trên Apple giờ do StoreKit 2 tự làm, không có API cho mình gọi.
//    - Doc v5 KHÔNG còn nhắc CrossPlatformValidator / GooglePlayTangle / AppleTangle
//      ở đâu cả; hướng chính thức là REMOTE (server-side) validation.
//
//  => Mặc định file này KHÔNG dùng CrossPlatformValidator. Đó là lý do biến mất
//     CS0246 "The type or namespace name 'CrossPlatformValidator' could not be found":
//     type đó nằm trong assembly platform-constrained, IAP v5 lại đổi tên assembly
//     từ UnityEngine.Purchasing.* thành Unity.Purchasing.* (changelog 5.0.0-pre.1),
//     nên asmdef cũ trỏ sai tên -> Editor qua, build Android chết.
//
//  Nếu vẫn muốn local validation Google Play:
//    1. Player Settings > Scripting Define Symbols: thêm IAP_LOCAL_VALIDATION
//    2. asmdef của com.3rd.core: thêm reference "Unity.Purchasing.Security"
//       (tên MỚI, không phải UnityEngine.Purchasing.Security)
//    3. Services > In-App Purchasing > Receipt Validation Obfuscator: generate
//       GooglePlayTangle (chỉ Google, Apple không còn generate nữa)
// ============================================================================

#if IAP_LOCAL_VALIDATION && UNITY_ANDROID && !UNITY_EDITOR
#define IAP_GOOGLE_RECEIPT_VALIDATION
#endif

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
#if IAP_GOOGLE_RECEIPT_VALIDATION
    using UnityEngine.Purchasing.Security;
#endif
    using Zenject;
    using ProductType = ServiceImplementation.IAPServices.ProductType;

    public class Iap5OrNewerServices : IIapServices, IInitializable, IDisposable
    {
        private readonly IapLogWrapped                iapLogWrapped;
        private readonly ISignalBus                   signalBus;
        private readonly List<string>                 ownedProducts = new();
        private          Dictionary<string, IAPModel> iapPack       = new();
        private          bool                         isInitedIap;
        private          bool                         isStoreConnected;

        private IStoreService    mStoreService;
        private IProductService  mProductService;
        private IPurchaseService mPurchasingService;

        private readonly ICatalogProvider mCatalogProvider = new CatalogProvider();
        private          IAPPaywallCallbacks mIAPPaywallCallbacks;

        public Action<string>                 OnCompletePurchase { get; set; }
        public readonly Dictionary<string, IOrderInfo> CachedOrders = new();

#if IAP_GOOGLE_RECEIPT_VALIDATION
        private CrossPlatformValidator mCrossPlatformValidator;
#endif

        public Iap5OrNewerServices(IapLogWrapped iapLogWrapped, ISignalBus signalBus)
        {
            this.iapLogWrapped = iapLogWrapped;
            this.signalBus     = signalBus;
        }

        #region Init

        public void Initialize()
        {
            this.mIAPPaywallCallbacks = new IAPPaywallCallbacks(this, this.iapLogWrapped, this.signalBus);
            this.CreateServices();
        }

        public void Dispose() { this.RemoveServiceCallbacks(); }

        public async void InitIapServices(Dictionary<string, IAPModel> iapPack, string environment = "production")
        {
            this.iapPack = iapPack ?? new Dictionary<string, IAPModel>();
            this.InitCatalog(this.iapPack);

            try
            {
                await UnityServices.InitializeAsync(new InitializationOptions().SetEnvironmentName(environment));
                this.OnInited();
            }
            catch (Exception exception)
            {
                // Init fail thì DỪNG. Bản cũ vẫn chạy tiếp Connect() lên service chưa init.
                this.OnError($"UnityServices.InitializeAsync failed: {exception.Message}");

                return;
            }

            this.CreateReceiptValidator();
            this.ConnectToStore();
        }

        private void CreateServices()
        {
            this.mStoreService      = UnityIAPServices.DefaultStore();
            this.mProductService    = UnityIAPServices.DefaultProduct();
            this.mPurchasingService = UnityIAPServices.DefaultPurchase();

            this.ConfigureServiceCallbacks();
        }

        private void ConfigureServiceCallbacks()
        {
            this.mProductService.OnProductsFetched         += this.mIAPPaywallCallbacks.OnInitialProductsFetched;
            this.mProductService.OnProductsFetchFailed     += this.mIAPPaywallCallbacks.OnInitialProductsFetchFailed;
            this.mPurchasingService.OnPurchasesFetched     += this.mIAPPaywallCallbacks.OnExistingPurchasesFetched;
            this.mPurchasingService.OnPurchasesFetchFailed += this.mIAPPaywallCallbacks.OnExistingPurchasesFetchFailed;
            this.mPurchasingService.OnPurchasePending      += this.mIAPPaywallCallbacks.OnPurchasePending;
            this.mPurchasingService.OnPurchaseConfirmed    += this.mIAPPaywallCallbacks.OnOrderConfirmed;
            this.mPurchasingService.OnPurchaseFailed       += this.mIAPPaywallCallbacks.OnPurchaseFailed;
            this.mPurchasingService.OnPurchaseDeferred     += this.mIAPPaywallCallbacks.OnOrderDeferred;
        }

        private void RemoveServiceCallbacks()
        {
            if (this.mProductService != null)
            {
                this.mProductService.OnProductsFetched     -= this.mIAPPaywallCallbacks.OnInitialProductsFetched;
                this.mProductService.OnProductsFetchFailed -= this.mIAPPaywallCallbacks.OnInitialProductsFetchFailed;
            }

            if (this.mPurchasingService == null) return;

            this.mPurchasingService.OnPurchasesFetched     -= this.mIAPPaywallCallbacks.OnExistingPurchasesFetched;
            this.mPurchasingService.OnPurchasesFetchFailed -= this.mIAPPaywallCallbacks.OnExistingPurchasesFetchFailed;
            this.mPurchasingService.OnPurchasePending      -= this.mIAPPaywallCallbacks.OnPurchasePending;
            this.mPurchasingService.OnPurchaseConfirmed    -= this.mIAPPaywallCallbacks.OnOrderConfirmed;
            this.mPurchasingService.OnPurchaseFailed       -= this.mIAPPaywallCallbacks.OnPurchaseFailed;
            this.mPurchasingService.OnPurchaseDeferred     -= this.mIAPPaywallCallbacks.OnOrderDeferred;
        }

        private async void ConnectToStore()
        {
            // Bản cũ: async void + không try/catch. Connect() throw là nuốt im lặng,
            // không log, không signal -> IAP treo vĩnh viễn, không ai debug được.
            try
            {
                await this.mStoreService.Connect();
            }
            catch (Exception exception)
            {
                this.OnError($"Store Connect failed: {exception.Message}");

                return;
            }

            this.isStoreConnected = true;
            this.iapLogWrapped.LogConsole("===========");
            this.iapLogWrapped.LogConsole("Store Connected.");
            this.FetchInitialProducts();
        }

        private void FetchInitialProducts()
        {
            this.mCatalogProvider.FetchProducts(this.mProductService.FetchProductsWithNoRetries, DefaultStoreHelper.GetDefaultStoreName());
        }

        private void InitCatalog(Dictionary<string, IAPModel> pack)
        {
            var initialProductsToFetch = new List<ProductDefinition>();

            foreach (var (_, model) in pack)
            {
                initialProductsToFetch.Add(new ProductDefinition(model.Id, ConvertToUnityProductType(model.ProductType)));
            }

            this.mCatalogProvider.AddProducts(initialProductsToFetch);
        }

        private static UnityEngine.Purchasing.ProductType ConvertToUnityProductType(ProductType productType)
        {
            return productType switch
            {
                ProductType.Consumable => UnityEngine.Purchasing.ProductType.Consumable,
                ProductType.Subscription => UnityEngine.Purchasing.ProductType.Subscription,
                ProductType.NonConsumable => UnityEngine.Purchasing.ProductType.NonConsumable,
                _ => UnityEngine.Purchasing.ProductType.Consumable
            };
        }

        private void OnInited()
        {
            this.isInitedIap = true;
            this.iapLogWrapped.LogConsole("IAP Services Initialized Successfully");
        }

        private void OnError(string message)
        {
            this.isInitedIap = false;
            this.iapLogWrapped.LogConsole($"IAP Failed: {message}");
            this.signalBus.Fire(new OnIAPPurchaseFailedSignal(string.Empty, message));
        }

        #endregion

        #region Receipt validation

        private void CreateReceiptValidator()
        {
#if IAP_GOOGLE_RECEIPT_VALIDATION
            try
            {
                if (this.IsGooglePlay())
                {
                    // v5: chỉ còn GooglePlayTangle. AppleTangle không còn được generate
                    // (changelog 5.0.0-pre.3: "Receipt obfuscation for Apple has been removed").
                    this.mCrossPlatformValidator = new CrossPlatformValidator(GooglePlayTangle.Data(), Application.identifier);
                }
            }
            // Bản cũ chỉ catch NotImplementedException. Changelog 5.2.0 nói rõ ctor có thể
            // throw InvalidPublicKeyException -> exception bay ra khỏi InitIapServices
            // -> ConnectToStore() không bao giờ chạy.
            catch (Exception exception)
            {
                this.mCrossPlatformValidator = null;
                this.iapLogWrapped.LogConsole($"Receipt validator unavailable: {exception.Message}");
            }
#endif
        }

        private bool IsGooglePlay()
        {
            return Application.platform == RuntimePlatform.Android &&
                   DefaultStoreHelper.GetDefaultStoreName() == GooglePlay.Name;
        }

        /// <summary>
        /// Trả về false CHỈ KHI validate được và receipt sai. Không validate được -> true.
        /// Bản cũ trả void: receipt sai vẫn log rồi grant content bình thường.
        /// </summary>
        public bool IsReceiptValid(IOrderInfo orderInfo)
        {
            if (orderInfo == null) return false;

#if IAP_GOOGLE_RECEIPT_VALIDATION
            if (this.mCrossPlatformValidator == null || !this.IsGooglePlay()) return true;

            try
            {
                // v5: dùng Order.Info.Receipt, không dùng Product.receipt nữa.
                var receipts = this.mCrossPlatformValidator.Validate(orderInfo.Receipt);

                this.iapLogWrapped.LogConsole("Validated Receipt. Contents:");

                foreach (var receipt in receipts)
                {
                    this.iapLogWrapped.LogReceiptValidation(receipt);
                }

                return true;
            }
            catch (IAPSecurityException exception)
            {
                this.iapLogWrapped.LogConsole($"Invalid receipt, not unlocking content: {exception.Message}");

                return false;
            }
#else
            // Không có local validation: Apple do StoreKit 2 tự lo, Google nên verify server-side.
            // Payload để gửi lên server: orderInfo.Receipt
            return true;
#endif
        }

        #endregion

        #region Entitlement

        public void CacheOrders(Orders existingOrders)
        {
            if (existingOrders == null) return;

            foreach (var order in existingOrders.ConfirmedOrders) this.CacheOrder(order.Info);

            // Bản cũ bỏ qua PendingOrders -> non-consumable đang chờ bị coi là chưa sở hữu.
            foreach (var order in existingOrders.PendingOrders) this.CacheOrder(order.Info);
        }

        public void CacheOrder(IOrderInfo orderInfo)
        {
            if (orderInfo == null || string.IsNullOrEmpty(orderInfo.Receipt)) return;

            foreach (var purchased in orderInfo.PurchasedProductInfo)
            {
                this.CachedOrders[purchased.productId] = orderInfo;

                if (this.ownedProducts.Contains(purchased.productId)) continue;
                this.ownedProducts.Add(purchased.productId);
                this.iapLogWrapped.LogConsole($"Owned product: {purchased.productId}");
            }
        }

        // Hàm này giờ CHỈ query, không còn side-effect ghi vào ownedProducts như bản cũ
        // (bản cũ vừa query vừa mutate, lại bị gọi 2 lần liên tiếp trong callback).
        public bool IsReceiptAvailable(Orders existingOrders)
        {
            return existingOrders != null &&
                   (existingOrders.ConfirmedOrders.Any(o => !string.IsNullOrEmpty(o.Info.Receipt)) ||
                    existingOrders.PendingOrders.Any(o => !string.IsNullOrEmpty(o.Info.Receipt)));
        }

        public bool IsProductOwned(string productId)
        {
            return !string.IsNullOrEmpty(productId) && this.ownedProducts.Contains(productId);
        }

        /// <summary>
        /// CẢNH BÁO: local validation KHÔNG đọc được expiryTimeMillis của Google sub,
        /// và StoreKit 2 không expose expiry cho client. Hàm này chỉ trả lời
        /// "store có ghi nhận order cho product này" — KHÔNG phải "sub còn hạn".
        /// Muốn đúng thì verify server-side (Google Play Developer API / App Store Server API).
        /// </summary>
        public bool IsSubscriptionActive(string productId)
        {
            if (!this.CachedOrders.TryGetValue(productId, out var orderInfo)) return false;
            if (string.IsNullOrEmpty(orderInfo.Receipt)) return false;

            // Bản cũ gọi thẳng mCrossPlatformValidator.Validate() không check null
            // -> NRE trong Editor và trên mọi build không phải Google Play.
            return this.IsReceiptValid(orderInfo);
        }

        public void FetchExistingPurchases()
        {
            if (this.mPurchasingService == null) return;
            this.mPurchasingService.FetchPurchases();
        }

        #endregion

        #region Purchase

        public void BuyProductID(string productId, Action<string> onComplete = null, Action<string> onFailed = null)
        {
            if (!this.isInitedIap || !this.isStoreConnected)
            {
                this.FailPurchase(productId, "IAP not ready.", onFailed);

                return;
            }

            var product = this.FindProduct(productId);

            if (product == null)
            {
                // Bản cũ Fire(OnStartDoingIAPSignal) TRƯỚC khi check product rồi thoát
                // mà không Fire signal fail -> loading spinner treo mãi.
                this.FailPurchase(productId, $"Product with ID {productId} not found.", onFailed);

                return;
            }

            this.signalBus.Fire(new OnStartDoingIAPSignal());
            this.OnCompletePurchase = onComplete;
            this.mPurchasingService.PurchaseProduct(product);
        }

        private void FailPurchase(string productId, string message, Action<string> onFailed)
        {
            this.iapLogWrapped.LogConsole(message);
            this.signalBus.Fire(new OnIAPPurchaseFailedSignal(productId, message));
            onFailed?.Invoke(message);
        }

        // Bản cũ set OnCompletePurchase = null NGAY TRONG vòng lặp cart
        // -> cart nhiều item chỉ item đầu chạy callback.
        public void InvokeCompletePurchase(string productId)
        {
            var callback = this.OnCompletePurchase;
            this.OnCompletePurchase = null;
            callback?.Invoke(productId);
        }

        public void ClearCompletePurchase() { this.OnCompletePurchase = null; }

        public void RestorePurchases(Action onComplete)
        {
            this.signalBus.Fire<OnStartDoingIAPSignal>();

            this.mPurchasingService.RestoreTransactions((success, error) =>
            {
                if (success)
                {
                    this.FetchExistingPurchases();
                }
                else
                {
                    // Bản cũ chỉ có `if (b)`: fail thì im lặng, onComplete không bao giờ chạy -> UI treo.
                    this.iapLogWrapped.LogConsole($"RestoreTransactions failed: {error}");
                    this.signalBus.Fire(new OnIAPPurchaseFailedSignal(string.Empty, error ?? "Restore failed"));
                }

                onComplete?.Invoke();
            });
        }

        public void ConfirmOrderIfAutomatic(PendingOrder order)
        {
            if (this.ShouldConfirmOrderAutomatically(order))
            {
                this.mPurchasingService.ConfirmPurchase(order);
            }
        }

        // Bản cũ trả true nếu CÓ BẤT KỲ item nào consumable -> auto-confirm luôn
        // cả non-consumable nằm cùng cart. Giờ: tất cả item phải là consumable.
        private bool ShouldConfirmOrderAutomatically(PendingOrder order)
        {
            var items = order.CartOrdered.Items().ToList();

            if (items.Count == 0) return false;

            foreach (var cartItem in items)
            {
                if (!this.iapPack.TryGetValue(cartItem.Product.definition.id, out var model)) return false;
                if (model.ProductType != ProductType.Consumable) return false;
            }

            return true;
        }

        // Giữ lại cho tương thích interface IIapServices.
        public void OnPurchaseConfirmed(IOrderInfo orderInfo) { this.CacheOrder(orderInfo); }

        #endregion

        #region Products

        private Product FindProduct(string productId)
        {
            return this.GetFetchedProducts()?.FirstOrDefault(p => p.definition.id == productId);
        }

        private ReadOnlyObservableCollection<Product> GetFetchedProducts() { return this.mProductService?.GetProducts(); }

        public string GetPriceById(string productId, string defaultPrice)
        {
            if (!this.isInitedIap) return defaultPrice;

            // First() -> FirstOrDefault(): khỏi dùng exception làm control flow như bản cũ.
            var price = this.FindProduct(productId)?.metadata?.localizedPriceString;

            return string.IsNullOrWhiteSpace(price) ? defaultPrice : price;
        }

        public ProductData GetProductData(string productId)
        {
            // Bản cũ dùng First() không try/catch -> InvalidOperationException nếu chưa fetch xong.
            var product = this.FindProduct(productId);

            if (product == null)
            {
                this.iapLogWrapped.LogConsole($"GetProductData: product {productId} not fetched.");

                return new ProductData { Id = productId };
            }

            return new ProductData
            {
                Id           = productId,
                Price        = product.metadata.localizedPrice,
                CurrencyCode = product.metadata.isoCurrencyCode
            };
        }

        #endregion
    }
}
#endif
