namespace ServiceImplementation.IAPServices
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using UnityEngine.Purchasing;

    public class DummyIapServices : IIapServices
    {
        public event Action<Order> OnPurchaseConfirmed
        {
            add { }
            remove { }
        }

        public UniTask Initialize(Dictionary<string, ProductType> iapPacks)
        {
            return UniTask.CompletedTask;
        }
        
        public UniTask PurchaseProduct(string productId)
        {
#if UNITY_EDITOR || DEBUG_MODULE
            return UniTask.CompletedTask;
#else
            return UniTask.FromException(new InvalidOperationException(
                $"IAP purchase '{productId}' was rejected because Unity IAP is not available in this build."));
#endif
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
#if UNITY_EDITOR || DEBUG_MODULE
            return true;
#else
            return false;
#endif
        }
    }
}
