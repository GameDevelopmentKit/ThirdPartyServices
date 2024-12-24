namespace ServiceImplementation.IAPServices
{
    using System;
    using System.Collections.Generic;
    using UnityEngine.Scripting;

    [Preserve]
    public class DummyIapServices : IIapServices
    {
        public void InitIapServices(Dictionary<string, IAPModel> iapPack, string environment = "production")
        {
        }

        public void BuyProductID(string productId, Action<string, int> onComplete = null, Action<string> onFailed = null)
        {
            onComplete?.Invoke(productId, 1);
        }

        public string GetPriceById(string productId, string defaultPrice)
        {
            return $"${defaultPrice}";
        }

        public void RestorePurchases(Action onComplete, Action onFailed = null)
        {
            onComplete?.Invoke();
        }

        public bool IsProductOwned(string productId)
        {
            return true;
        }

        public ProductData GetProductData(string productId)
        {
            return new();
        }
    }
}