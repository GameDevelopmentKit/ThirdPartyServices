namespace ServiceImplementation.IAPServices
{
    using System;
    using System.Collections.Generic;
    using Core.AdsServices;

    public class DummyIapServices : IIapServices
    {
        public void InitIapServices(Dictionary<string, IAPModel> iapPack, string environment = "production") { }

        public void BuyProductID(string productId, Action<string> onComplete = null, Action<string> onFailed = null) { onComplete?.Invoke(productId); }

        public string GetPriceById(string productId, string defaultPrice) { return $"$2.99"; }
        public void   RestorePurchases(Action onComplete)                 { onComplete?.Invoke(); }
        public bool   IsProductOwned(string productId)                    { return true; }
    }

    public class DummyRemoveAdsIapServices : IRemoveAdsServices
    {
        private readonly RemoveAdData removeAdData;
        private readonly IAdServices  adServices;

        public DummyRemoveAdsIapServices(RemoveAdData removeAdData, IAdServices adServices)
        {
            this.removeAdData = removeAdData;
            this.adServices   = adServices;
        }

        public void BuyRemoveAds(string removeAdsId, Action<string> onComplete = null, Action<string> onFailed = null)
        {
            this.adServices.RemoveAds();
            onComplete?.Invoke(removeAdsId);
        }
    }
}