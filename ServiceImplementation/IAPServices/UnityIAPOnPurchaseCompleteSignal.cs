namespace ServiceImplementation.IAPServices
{
    public class UnityIAPOnRestorePurchaseCompleteSignal
    {
        public string ProductID { get; }
        public UnityIAPOnRestorePurchaseCompleteSignal(string productID) { this.ProductID = productID; }
    }
}