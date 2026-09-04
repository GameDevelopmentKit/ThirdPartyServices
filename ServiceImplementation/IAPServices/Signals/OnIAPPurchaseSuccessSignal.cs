namespace ServiceImplementation.IAPServices.Signals
{
#if IAP|| IAP_5_OR_NEWER
    using UnityEngine.Purchasing;
#endif

    public class OnIAPPurchaseSuccessSignal
    {
        public string ProductId { get; set; }
#if IAP||IAP_5_OR_NEWER
        public Product PurchasedProduct { get; set; }
#endif
    }
}