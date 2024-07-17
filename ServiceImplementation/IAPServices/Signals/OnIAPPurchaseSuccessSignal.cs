namespace ServiceImplementation.IAPServices.Signals
{
    public class OnIAPPurchaseSuccessSignal
    {
        public ProductData Product { get; }

        public OnIAPPurchaseSuccessSignal(ProductData product)
        {
            this.Product = product;
        }
    }
}