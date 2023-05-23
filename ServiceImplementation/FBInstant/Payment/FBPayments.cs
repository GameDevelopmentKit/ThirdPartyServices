namespace ServiceImplementation.FBInstant.Payment
{
    using System;
    using System.Runtime.InteropServices;
    using Newtonsoft.Json;

    public class FBPayments
    {
        public static bool isOnReadyOk = false;

        public bool isSupportPayments()
        {
            return FBInstant.getSupportedAPIs().Contains("payments.purchaseAsync") && isOnReadyOk;
        }

        /// <summary>
        /// callback after FBInstant.payments.onReady
        /// </summary>
        public static Action payments_onReady_Callback = null;

        [DllImport("__Internal")]
        public static extern void payments_onReady();

        /// <summary>
        /// callback after getCatalogAsync
        /// </summary>
        public static Action<FBError, Product[]> getCatalogAsync_Callback = null;

        [DllImport("__Internal")]
        public static extern void payments_getCatalogAsync();

        /// <summary>
        /// callback after purchaseAsync
        /// </summary>
        public static Action<FBError, Purchase> purchaseAsync_Callback = null;

        [DllImport("__Internal")]
        public static extern void payments_purchaseAsync(string purchaseConfig);

        /// <summary>
        /// callback after getPurchasesAsync
        /// </summary>
        public static Action<FBError, Purchase[]> getPurchasesAsync_Callback = null;

        [DllImport("__Internal")]
        public static extern void payments_getPurchasesAsync();

        /// <summary>
        /// callback after consumePurchaseAsync
        /// </summary>
        public static Action<FBError> consumePurchaseAsync_Callback = null;

        [DllImport("__Internal")]
        public static extern void payments_consumePurchaseAsync(string purchaseToken);


        public void onReady(Action cb)
        {
            payments_onReady_Callback = cb;
            payments_onReady();
        }

        public void getCatalogAsync(Action<FBError, Product[]> cb)
        {
            getCatalogAsync_Callback = cb;
            payments_getCatalogAsync();
        }

        public void purchaseAsync(PurchaseConfig purchaseConfig, Action<FBError, Purchase> cb)
        {
            purchaseAsync_Callback = cb;
            payments_purchaseAsync(JsonConvert.SerializeObject(purchaseConfig));
        }

        public void getPurchasesAsync(Action<FBError, Purchase[]> cb)
        {
            getPurchasesAsync_Callback = cb;
            payments_getPurchasesAsync();
        }

        public void consumePurchaseAsync(string purchaseToken, Action<FBError> cb)
        {
            consumePurchaseAsync_Callback = cb;
            payments_consumePurchaseAsync(purchaseToken);
        }
    }
}