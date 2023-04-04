namespace ServiceImplementation.IAPServices
{
    using System;
    using System.Collections.Generic;
    using Core.AdsServices;

    public class DummyUnityIapServices : IUnityIapServices
    {
        public void InitIapServices(Dictionary<string, IAPModel> iapPack, string environment = "production") { }

        public void BuyProductID(string productId, Action<string> onComplete = null, Action<string> onFailed = null) { onComplete?.Invoke(productId); }

        public string GetPriceById(string productId) { return $"0.0"; }
        public void   RestorePurchases()             { }
    }

    public class DummyUnityRemoveAdsIapServices : IUnityRemoveAdsServices
    {
        private readonly List<string> listRemoveAds;
        private readonly IAdServices  adServices;

        public DummyUnityRemoveAdsIapServices(List<string> listRemoveAds, IAdServices adServices)
        {
            this.listRemoveAds = listRemoveAds;
            this.adServices    = adServices;
        }

        public void BuyRemoveAds(string removeAdsId, Action<string> onComplete = null, Action<string> onFailed = null)
        {
            this.adServices.RemoveAds();
            onComplete?.Invoke(removeAdsId);
        }
    }
}