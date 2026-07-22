namespace ServiceImplementation.IAPServices
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using UnityEngine.Purchasing;

    public interface IIapServices
    {
        event Action<Order> OnPurchaseConfirmed;

        UniTask          Initialize(Dictionary<string, ProductType> iapPacks, Func<byte[]> getGooglePublicKey = null, Func<byte[]> getAppleRootCert = null);
        UniTask          PurchaseProduct(string productId);
        Product          FindProduct(string productId);
        UniTask<Product> FetchProduct(string productId);
        string           GetLocalizedPriceString(string productId, string defaultValue = "");
        decimal          GetLocalizedPrice(string productId, decimal defaultValue = 0);
        bool             IsProductAvailable(string productId);
    }
}
