#if THEONE_IAP
namespace ServiceImplementation.IAPServices
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Core.AdsServices;
    using GameFoundation.Signals;
    using Newtonsoft.Json;
    using ServiceImplementation.IAPServices.Signals;
    using TheOne.Logging;
    using Unity.Services.Core;
    using Unity.Services.Core.Environments;
    using UnityEngine;
    using UnityEngine.Purchasing;
    using UnityEngine.Purchasing.Extension;
    using UnityEngine.Purchasing.Security;
    using UnityEngine.Scripting;
    using ILogger = TheOne.Logging.ILogger;

    public class UnityIapServices : IIapServices, IDetailedStoreListener
    {
        private Action<string, int> onPurchaseComplete;
        private Action<string>      onPurchaseFailed;
        private IStoreController    mStoreController;
        private IExtensionProvider  mStoreExtensionProvider;

        #region inject

        private readonly ILogger                      logger;
        private readonly SignalBus                    signalBus;
        private readonly IAdServices                  adServices;
        private          Dictionary<string, IAPModel> iapPacks;

        #endregion

        [Preserve]
        public UnityIapServices(ILoggerManager loggerManager, SignalBus signalBus)
        {
            this.logger    = loggerManager.GetLogger(this);
            this.signalBus = signalBus;
        }

        public async void InitIapServices(Dictionary<string, IAPModel> iapPack, string environment = "production")
        {
            if (this.mStoreController != null) return;
            this.iapPacks = iapPack;

            // Begin to configure our connection to Purchasing
            try
            {
                var options = new InitializationOptions()
                    .SetEnvironmentName(environment);

                await UnityServices.InitializeAsync(options);
            }
            catch (Exception exception)
            {
                // An error occurred during services initialization.
                this.logger.Info($"init failed {exception.Message}");
            }

            this.InitializePurchasing();
        }

        private bool IsInitialized => this.mStoreController != null && this.mStoreExtensionProvider != null;

        private void InitializePurchasing()
        {
            if (this.IsInitialized)
            {
                return;
            }

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            this.AddAllProduct(builder);
            UnityPurchasing.Initialize(this, builder);
        }

        private void AddAllProduct(ConfigurationBuilder builder)
        {
            for (var i = 0; i < this.iapPacks.Count; i++)
            {
                var current = this.iapPacks.ElementAt(i);
                builder.AddProduct(current.Value.Id, ConvertToUnityProductType(current.Value.ProductType));
            }
        }

        private static UnityEngine.Purchasing.ProductType ConvertToUnityProductType(ProductType productType)
        {
            return productType switch
            {
                ProductType.Consumable    => UnityEngine.Purchasing.ProductType.Consumable,
                ProductType.Subscription  => UnityEngine.Purchasing.ProductType.Subscription,
                ProductType.NonConsumable => UnityEngine.Purchasing.ProductType.NonConsumable,
                _                         => UnityEngine.Purchasing.ProductType.Consumable,
            };
        }

        public string GetPriceById(string id, string defaultPrice = "")
        {
            var s = defaultPrice;

            if (!this.IsInitialized) return s;

            try
            {
                s = this.mStoreController.products.WithID(id).metadata.localizedPriceString;

                if (string.IsNullOrWhiteSpace(s))
                {
                    s = defaultPrice;
                }
            }
            catch (Exception e)
            {
                this.logger.Error($"{e.Message}");
            }

            return s;
        }

        public void BuyProductID(string productId, Action<string, int> onComplete, Action<string> onFailed = null)
        {
            if (this.IsInitialized)
            {
                this.signalBus.Fire<OnStartDoingIAPSignal>();

                var product = this.mStoreController.products.WithID(productId);

                if (product is { availableToPurchase: true })
                {
                    this.logger.Info($"Purchasing product asychronously: '{product.definition.id}'");

                    this.onPurchaseComplete = onComplete;
                    this.onPurchaseFailed   = onFailed;
                    this.mStoreController.InitiatePurchase(product);
                }
                else
                {
                    onFailed?.Invoke(productId);
                    this.logger.Info("FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            }
            else
            {
                this.InitializePurchasing();
                onFailed?.Invoke(productId);
                this.logger.Info("FAIL. Not initialized.");
            }
        }

        // Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google.
        // Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
        public void RestorePurchases(Action onComplete = null, Action onFailed = null)
        {
            #if FAKE_RESTORE_PURCHASE
            foreach (var iapPack in this.iapPacks)
            {
                this.signalBus.Fire(new UnityIAPOnRestorePurchaseCompleteSignal(iapPack.Value.Id));
            }

            onComplete?.Invoke();

            return;

            #endif

            // If Purchasing has not yet been set up ...
            if (!this.IsInitialized)
            {
                // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
                this.logger.Info("FAIL. Not initialized.");
                onFailed?.Invoke();
                return;
            }

            // If we are running on an Apple device ...
            if (Application.platform is RuntimePlatform.IPhonePlayer or RuntimePlatform.OSXPlayer)
            {
                this.signalBus.Fire<OnStartDoingIAPSignal>();

                // ... begin restoring purchases
                this.logger.Info("started ...");

                // Fetch the Apple store-specific subsystem.
                var apple = this.mStoreExtensionProvider.GetExtension<IAppleExtensions>();

                // Begin the asynchronous process of restoring purchases. Expect a confirmation response in
                // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
                apple.RestoreTransactions((result, _) =>
                {
                    // The first phase of restoration. If no more responses are received on ProcessPurchase then
                    // no purchases are available to be restored.
                    this.logger.Info("continuing: " + result + ". If no further messages, no purchases available to restore.");

                    if (!result)
                    {
                        onFailed?.Invoke();
                        return;
                    }

                    foreach (var iapPack in this.iapPacks)
                    {
                        if (!this.IsProductOwned(iapPack.Value.Id)) continue;
                        this.signalBus.Fire(new OnRestorePurchaseCompleteSignal(iapPack.Value.Id, 1));
                    }

                    onComplete?.Invoke();
                });
            }
            // Otherwise ...
            else
            {
                // We are not running on an Apple device. No work is necessary to restore purchases.
                this.logger.Info("FAIL. Not supported on this platform. Current = " + Application.platform);
                onFailed?.Invoke();
            }
        }

        public bool IsProductOwned(string productId)
        {
            if (!this.IsInitialized) return false;

            if (string.IsNullOrEmpty(productId)) return false;

            var pd = this.mStoreController.products.WithID(productId);

            if (!pd.hasReceipt) return false;
            // presume validity if not validate receipt.
            #if !UNITY_EDITOR
            var isValid = this.ValidateReceipt(pd.receipt, out var purchaseReceipts);

            return isValid;
            #endif
            return true;
        }

        public ProductData GetProductData(string productId)
        {
            var product = this.mStoreController.products.WithID(productId);

            return new ProductData()
            {
                Id           = productId,
                Price        = product.metadata.localizedPrice,
                CurrencyCode = product.metadata.isoCurrencyCode
            };
        }

        //check Valid product
        private bool ValidateReceipt(string receipt, out IPurchaseReceipt[] purchaseReceipts, bool logReceiptContent = true)
        {
            // default the out parameter to an empty array
            purchaseReceipts = Array.Empty<IPurchaseReceipt>();

            // Does the receipt has some content?
            if (string.IsNullOrEmpty(receipt))
            {
                this.logger.Info("receipt is null or empty.");

                return false;
            }

            var isValidReceipt = true; // presume validity for platforms with no receipt validation.
            // Unity IAP's receipt validation is only available for Apple app stores and Google Play store.
            #if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_TVOS

            byte[] googlePlayTangleData = null;
            byte[] appleTangleData      = null;

            // Here we populate the secret keys for each platform.
            // Note that the code is disabled in the editor for it to not stop the EM editor code (due to ClassNotFound error)
            // from recreating the dummy AppleTangle and GoogleTangle classes if they were inadvertently removed.

            // #if UNITY_ANDROID && !UNITY_EDITOR
            // googlePlayTangleData = GooglePlayTangle.Data();
            // #endif

            #if (UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_TVOS) && !UNITY_EDITOR
            appleTangleData = AppleTangle.Data();
            #endif

            // Prepare the validator with the secrets we prepared in the Editor obfuscation window.
            var validator = new CrossPlatformValidator(googlePlayTangleData, appleTangleData, Application.identifier);

            try
            {
                // On Google Play, result has a single product ID.
                // On Apple stores, receipts contain multiple products.
                var result = validator.Validate(receipt);

                // If the validation is successful, the result won't be null.
                if (result == null)
                {
                    isValidReceipt = false;
                }
                else
                {
                    purchaseReceipts = result;

                    // For informational purposes, we list the receipt(s)
                    if (logReceiptContent)
                    {
                        this.logger.Info("Receipt contents:");

                        foreach (var productReceipt in result)
                        {
                            if (productReceipt == null) continue;
                            this.logger.Info(productReceipt.productID);
                            this.logger.Info(productReceipt.purchaseDate.ToString(CultureInfo.InvariantCulture));
                            this.logger.Info(productReceipt.transactionID);
                        }
                    }
                }
            }
            catch (IAPSecurityException)
            {
                isValidReceipt = false;
            }
            #endif

            return isValidReceipt;
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            this.logger.Info("PASS");
            this.mStoreController        = controller;
            this.mStoreExtensionProvider = extensions;
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message) { }

        [Obsolete]
        public void OnInitializeFailed(InitializationFailureReason error) { }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var productId = args.purchasedProduct.definition.id;
            var receipt   = args.purchasedProduct.receipt;
            var quantity  = this.GetPurchaseQuantityFromReceipt(receipt);

            this.logger.Info($"{productId} quantity: {quantity}");

            if (this.onPurchaseComplete == null)
            {
                this.signalBus.Fire(new OnRestorePurchaseCompleteSignal(productId, quantity));
            }
            else
            {
                this.signalBus.Fire(new OnIAPPurchaseSuccessSignal(this.GetProductData(productId), quantity));
            }

            this.onPurchaseComplete?.Invoke(productId, quantity);
            this.onPurchaseComplete = null;

            return PurchaseProcessingResult.Complete;
        }

        private int GetPurchaseQuantityFromReceipt(string receipt)
        {
            #if UNITY_IOS
            return 1;
            #endif

            try
            {
                var googlePlayReceipt       = JsonConvert.DeserializeObject<GooglePlayReceipt>(receipt);
                var playReceiptPlayload     = JsonConvert.DeserializeObject<GooglePlayReceiptPlayload>(googlePlayReceipt.Payload);
                var playReceiptPlayloadJson = JsonConvert.DeserializeObject<GooglePlayReceiptPayloadJson>(playReceiptPlayload.json);

                return playReceiptPlayloadJson.quantity;
            }
            catch (Exception e)
            {
                this.logger.Error($"Fail {e.Message}");
                return 1; // Default to 1 if quantity is not available or parsing fails
            }
        }

        #region Google Play Receipt Quantity

        [Preserve]
        public record GooglePlayReceipt
        {
            [Preserve] public string Payload { get; set; }
        }

        [Preserve]
        public class GooglePlayReceiptPlayload
        {
            [Preserve] public string json { get; set; }
        }

        [Preserve]
        public class GooglePlayReceiptPayloadJson
        {
            [Preserve] public int quantity { get; set; }
        }

        #endregion

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            var productId = product.definition.id;
            this.onPurchaseFailed?.Invoke(productId);
            this.onPurchaseFailed = null;
            this.signalBus.Fire(new OnIAPPurchaseFailedSignal(productId, failureDescription.reason.ToString()));
            this.logger.Info($"FAIL. Product: '{productId}', Reason: {failureDescription.reason}, Message: {failureDescription.message}");
        }

        [Obsolete]
        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) { }
    }
}
#endif