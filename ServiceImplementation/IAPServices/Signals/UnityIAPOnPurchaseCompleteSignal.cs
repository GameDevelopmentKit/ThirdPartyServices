namespace ServiceImplementation.IAPServices.Signals
{
    using UnityEngine.Purchasing;

    public class OnRestorePurchaseCompleteSignal
    {
        public string ProductID { get; }
        public OnRestorePurchaseCompleteSignal(string productID) { this.ProductID = productID; }
    }

    public class OnUnityIAPPurchaseSuccessSignal
    {
        public Product Product { get;}
        public OnUnityIAPPurchaseSuccessSignal(Product product) { this.Product = product; }
    }
}