namespace ServiceImplementation.IAPServices.Signals
{
    public class OnRestorePurchaseCompleteSignal
    {
        public string ProductID { get; }

        public OnRestorePurchaseCompleteSignal(string productID)
        {
            this.ProductID = productID;
        }
    }
}