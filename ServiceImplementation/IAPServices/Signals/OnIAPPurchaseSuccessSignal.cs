namespace ServiceImplementation.IAPServices.Signals
{
#if THEONE_IAP
    using UnityEngine.Purchasing;
#endif

    public class OnIAPPurchaseSuccessSignal
    {
        public string ProductId { get; set; }
#if THEONE_IAP
        public Product PurchasedProduct { get; set; }
#endif
    }
}