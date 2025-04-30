namespace ServiceImplementation.IAPServices
{
    using System;
    using System.Collections.Generic;
    using GameFoundation.Scripts.Utilities.LogService;

    public class DummyIapServices : IIapServices
    {
        private readonly ILogService logger;

        public DummyIapServices(ILogService logger) { this.logger = logger; }

        public void InitIapServices(Dictionary<string, IAPModel> iapPack, string environment = "production") { }

        public void BuyProductID(string productId, Action<string> onComplete = null, Action<string> onFailed = null)
        {
            this.logger.Log($"DummyIapServices: BuyProductID {productId}");
            onComplete?.Invoke(productId);
        }

        public string GetPriceById(string productId, string defaultPrice) { return $"$2.99"; }

        public void RestorePurchases(Action onComplete)
        {
            this.logger.Log("DummyIapServices: RestorePurchases");
            onComplete?.Invoke();
        }

        public bool        IsProductOwned(string productId) { return true; }
        public ProductData GetProductData(string productId) { return new ProductData(); }
    }
}