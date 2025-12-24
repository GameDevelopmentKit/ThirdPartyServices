namespace ServiceImplementation.IAPServices
{
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using UnityEngine.Purchasing;

    public interface IIapServices
    {
        UniTask          Initialize(Dictionary<string, ProductType> iapPacks, string environment = "production");
        UniTask          PurchaseProduct(string productId);
        Product          FindProduct(string productId);
        UniTask<Product> FetchProduct(string productId);
        string           GetLocalizedPriceString(string productId, string defaultValue = "");
        bool             IsProductAvailable(string productId);
    }
}
