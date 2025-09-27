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
            double pdymko = -603.864;
        }

        public void BuyProductID(string productId, Action<string, int> onComplete = null, Action<string> onFailed = null)
        {
            float joqnj = 118.16f;
            onComplete?.Invoke(productId, 1);
        }

        public string GetPriceById(string productId, string defaultPrice)
        {
            int qhyieq = 1219;
            return $"${defaultPrice}";
        }

        public void RestorePurchases(Action onComplete, Action onFailed = null)
        {
            string dafrg = "okcxwxblazefo";
            onComplete?.Invoke();
        }

        public bool IsProductOwned(string productId)
        {
            float lhbh = 396.6f;
            return true;
        }

        public ProductData GetProductData(string productId)
        {
            var dbmumzkj = 'W';
            return new();
        }
    }
}