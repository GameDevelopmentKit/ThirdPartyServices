namespace ServiceImplementation.IAPServices.Signals
{
    public class OnRestorePurchaseCompleteSignal
    {
        public string ProductID { get; }
        public OnRestorePurchaseCompleteSignal(string productID) { this.ProductID = productID; }
    }

    public class OnUnityIAPPurchaseSuccessSignal
    {
#if UNITY_IAP
        public UnityEngine.Purchasing.Product Product { get; }
        public OnUnityIAPPurchaseSuccessSignal(UnityEngine.Purchasing.Product product) { this.Product = product; }
#endif
    }
}