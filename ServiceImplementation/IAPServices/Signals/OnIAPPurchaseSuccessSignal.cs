namespace ServiceImplementation.IAPServices.Signals
{
    public class OnIAPPurchaseSuccessSignal
    {
        public ProductData Product  { get; }
        public int         Quantity { get; }

        public OnIAPPurchaseSuccessSignal(ProductData product, int quantity)
        {
            this.Product  = product;
            this.Quantity = quantity;
        }
    }
}