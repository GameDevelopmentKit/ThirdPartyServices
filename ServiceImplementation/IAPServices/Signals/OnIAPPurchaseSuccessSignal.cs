namespace ServiceImplementation.IAPServices.Signals
{
    using UnityEngine.Purchasing;

    public class OnIAPPurchaseSuccessSignal
    {
        public string ProductId { get; set; }
        public Product PurchasedProduct { get; set; }
    }
}