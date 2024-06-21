namespace ServiceImplementation.IAPServices.Signals
{
#if IAP
    using UnityEngine.Purchasing;
#endif

    public class OnIAPPurchaseSuccessSignal
    {
        public string ProductId { get; set; }
#if IAP
        public Product PurchasedProduct { get; set; }
#endif
    }
}