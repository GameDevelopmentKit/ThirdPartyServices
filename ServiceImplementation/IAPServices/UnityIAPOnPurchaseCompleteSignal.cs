namespace ServiceImplementation.IAPServices
{
    public class UnityIAPOnPurchaseCompleteSignal
    {
        public string ProductID { get; }
        public UnityIAPOnPurchaseCompleteSignal(string productID) { this.ProductID = productID; }
    }
}