#if THEONE_IAP
namespace ServiceImplementation.IAPServices
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Core.AdsServices;
    using GameFoundation.Scripts.Utilities.LogService;
    using ServiceImplementation.IAPServices.Signals;
    using Unity.Services.Core;
    using Unity.Services.Core.Environments;
    using UnityEngine;
    using UnityEngine.Purchasing;
    using UnityEngine.Purchasing.Security;
    using Zenject;

    public class UnityIapServices : IIapServices, IStoreListener
    {
        private Action<string>     onPurchaseComplete, onPurchaseFailed;
        private IStoreController   mStoreController;
        private IExtensionProvider mStoreExtensionProvider;

        #region inject

        private readonly ILogService                  logger;
        private readonly SignalBus                    signalBus;
        private readonly IAdServices                  adServices;
        private          Dictionary<string, IAPModel> iapPacks;

        #endregion

        public UnityIapServices(ILogService log, SignalBus signalBus)
        {
            this.logger        = log;
            this.signalBus     = signalBus;
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
                this.logger.Log($"init failed {exception.Message}");
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
                builder.AddProduct(current.Value.Id, this.ConvertToUnityProductType(current.Value.ProductType));
            }
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
                this.logger.Log($"GetPriceById {e.Message}");
            }

            return s;
        }

        public void BuyProductID(string productId, Action<string> onComplete, Action<string> onFailed = null)
        {
            if (this.IsInitialized)
            {
                this.signalBus.Fire<OnStartDoingIAPSignal>();
                
                var product = this.mStoreController.products.WithID(productId);

                if (product is { availableToPurchase: true })
                {
                    this.logger.Log($"Purchasing product asychronously: '{product.definition.id}'");

                    this.onPurchaseComplete = onComplete;
                    this.mStoreController.InitiatePurchase(product);
                }
                else
                {
                    this.onPurchaseFailed = onFailed;
                    this.logger.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            }
            else
            {
                this.InitializePurchasing();
                this.logger.Log("BuyProductID FAIL. Not initialized.");
            }
        }

        // Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
        // Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
        public void RestorePurchases(Action onComplete = null)
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
                this.logger.Log("RestorePurchases FAIL. Not initialized.");

                return;
            }

            // If we are running on an Apple device ... 
            if (Application.platform is RuntimePlatform.IPhonePlayer or RuntimePlatform.OSXPlayer)
            {
                this.signalBus.Fire<OnStartDoingIAPSignal>();

                // ... begin restoring purchases
                this.logger.Log("RestorePurchases started ...");

                // Fetch the Apple store-specific subsystem.
                var apple = this.mStoreExtensionProvider.GetExtension<IAppleExtensions>();

                // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
                // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
                apple.RestoreTransactions((result, _) =>
                {
                    // The first phase of restoration. If no more responses are received on ProcessPurchase then 
                    // no purchases are available to be restored.
                    this.logger.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");

                    if (!result) return;

                    foreach (var iapPack in this.iapPacks)
                    {
                        if (!this.IsProductOwned(iapPack.Value.Id)) continue;
                        this.signalBus.Fire(new OnRestorePurchaseCompleteSignal(iapPack.Value.Id));
                    }

                    onComplete?.Invoke();
                });
            }
            // Otherwise ...
            else
            {
                // We are not running on an Apple device. No work is necessary to restore purchases.
                this.logger.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
            }
        }

        public bool IsProductOwned(string productId)
        {
            if (!this.IsInitialized)
                return false;

            if (string.IsNullOrEmpty(productId))
                return false;

            var pd = this.mStoreController.products.WithID(productId);

            if (!pd.hasReceipt) return false;
            // presume validity if not validate receipt.
#if !UNITY_EDITOR
            var isValid = this.ValidateReceipt(pd.receipt, out var purchaseReceipts);

            return isValid;
#endif
            return true;
        }

        //check Valid product
        private bool ValidateReceipt(string receipt, out IPurchaseReceipt[] purchaseReceipts, bool logReceiptContent = true)
        {
            // default the out parameter to an empty array 
            purchaseReceipts = Array.Empty<IPurchaseReceipt>();

            // Does the receipt has some content?
            if (string.IsNullOrEmpty(receipt))
            {
                this.logger.Log("Receipt Validation: receipt is null or empty.");

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
                        this.logger.Log("Receipt contents:");

                        foreach (var productReceipt in result)
                        {
                            if (productReceipt == null) continue;
                            this.logger.Log(productReceipt.productID);
                            this.logger.Log(productReceipt.purchaseDate.ToString(CultureInfo.InvariantCulture));
                            this.logger.Log(productReceipt.transactionID);
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
            this.logger.Log("OnInitialized: PASS");
            this.mStoreController        = controller;
            this.mStoreExtensionProvider = extensions;
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message) { }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
            this.logger.Log("OnInitializeFailed InitializationFailureReason:" + error);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            if (this.onPurchaseComplete == null)
            {
                this.signalBus.Fire(new OnRestorePurchaseCompleteSignal(args.purchasedProduct.definition.id));
            }

            this.onPurchaseComplete?.Invoke(args.purchasedProduct.definition.id);
            this.onPurchaseComplete = null;

            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            this.onPurchaseFailed?.Invoke(product.definition.storeSpecificId);
            this.onPurchaseFailed = null;
            this.logger.Log($"OnPurchaseFailed: FAIL. Product: '{product.definition.storeSpecificId}', PurchaseFailureReason: {failureReason}");
        }
    }
}
#endif