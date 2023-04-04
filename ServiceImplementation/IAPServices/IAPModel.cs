namespace ServiceImplementation.IAPServices
{
    public class IAPModel
    {
        public string      Id          { get; set; }
        public ProductType ProductType { get; set; }
    }

    public enum ProductType
    {
        Consumable,
        NonConsumable,
        Subscription
    }
}