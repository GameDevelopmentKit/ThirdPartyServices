namespace ServiceImplementation.IAPServices
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using UnityEngine.Purchasing;

    public class DummyIapServices : IIapServices
    {

        public UniTask Initialize(Dictionary<string, ProductType> iapPacks, string environment = "production", Func<byte[]> getGooglePublicKey = null, Func<byte[]> getAppleRootCert = null)
        {
            return UniTask.CompletedTask;
        }
        
        public UniTask PurchaseProduct(string productId)
        {
            return UniTask.CompletedTask;
        }
        
        public Product FindProduct(string productId)
        {
            return null;
        }
        
        public UniTask<Product> FetchProduct(string productId)
        {
            return UniTask.FromResult<Product>(null);
        }
        
        public string GetLocalizedPriceString(string productId, string defaultValue = "")
        {
            return defaultValue;
        }

        public decimal GetLocalizedPrice(string productId, decimal defaultValue = 0)
        {
            return defaultValue;
        }

        public bool IsProductAvailable(string productId)
        {
            return true;
        }
    }
}
