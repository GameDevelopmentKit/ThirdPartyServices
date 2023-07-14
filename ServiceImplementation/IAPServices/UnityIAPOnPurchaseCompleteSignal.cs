namespace ServiceImplementation.IAPServices
{
    public class OnRestorePurchaseCompleteSignal
    {
        public string ProductID { get; }
        public OnRestorePurchaseCompleteSignal(string productID) { this.ProductID = productID; }
    }
}