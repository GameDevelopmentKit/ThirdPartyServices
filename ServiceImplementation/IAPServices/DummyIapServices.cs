namespace ServiceImplementation.IAPServices
{
    using System;
    using System.Collections.Generic;

    public class DummyIapServices : IIapServices
    {
        public void InitIapServices(Dictionary<string, IAPModel> iapPack, string environment = "production") { }

        public void BuyProductID(string productId, Action<string> onComplete = null, Action<string> onFailed = null) { onComplete?.Invoke(productId); }

        public string      GetPriceById(string productId, string defaultPrice) { return $"$2.99"; }
        public void        RestorePurchases(Action onComplete)                 { onComplete?.Invoke(); }
        public bool        IsProductOwned(string productId)                    { return true; }
        public ProductData GetProductData(string productId)                    { return new ProductData(); }
    }
}