namespace ServiceImplementation.IAPServices.Signals
{
    public class OnIAPPurchaseFailedSignal
    {
        public string ProductId { get; }
        public string Error     { get; }

        public OnIAPPurchaseFailedSignal(string productId, string error)
        {
            this.ProductId = productId;
            this.Error     = error;
        }
    }
}