namespace ServiceImplementation.IAPServices
{
    using System;
    using System.Collections.Generic;

    public interface IUnityIapServices
    {
        void   InitIapServices(Dictionary<string, IAPModel> iapPack, string environment = "production");
        void   BuyProductID(string productId, Action<string> onComplete = null, Action<string> onFailed = null);
        string GetPriceById(string productId);
        void   RestorePurchases(Action onComplete);
    }
}