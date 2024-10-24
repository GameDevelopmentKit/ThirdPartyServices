namespace ServiceImplementation.IAPServices
{
    using System;
    using System.Collections.Generic;
    using Core.AdsServices;
    using Zenject;

    public class DummyIapServices : IIapServices
    {
        public bool IsInitialized { get; } = true;
        public void InitIapServices(Dictionary<string, IAPModel> iapPack, string environment = "production") { }

        public void BuyProductID(string productId, Action<string> onComplete = null, Action<string> onFailed = null) { onComplete?.Invoke(productId); }

        public string      GetPriceById(string productId, string defaultPrice) { return $"$2.99"; }
        public void        RestorePurchases(Action onComplete)                 { onComplete?.Invoke(); }
        public bool        IsProductOwned(string productId)                    { return true; }
        public bool        IsProductAvailable(string productId)                {return true; }
        public ProductData GetProductData(string productId)                    { return new ProductData(); }
    }
}