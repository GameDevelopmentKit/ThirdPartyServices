namespace ServiceImplementation.IAPServices.Signals
{
    public class OnRestorePurchaseCompleteSignal
    {
        public string ProductID { get; }
        public int    Quantity  { get; }

        public OnRestorePurchaseCompleteSignal(string productID, int quantity)
        {
            this.ProductID = productID;
            this.Quantity  = quantity;
        }
    }
}