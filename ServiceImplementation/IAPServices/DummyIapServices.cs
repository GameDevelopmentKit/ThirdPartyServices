namespace ServiceImplementation.IAPServices
{
    using System;
    using System.Collections.Generic;
    using Core.AdsServices;
    using Zenject;

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
        private readonly SignalBus    signalBus;
        private readonly RemoveAdData removeAdData;
        private readonly IAdServices  adServices;

        public DummyRemoveAdsIapServices(SignalBus signalBus, RemoveAdData removeAdData, IAdServices adServices)
        {
            this.signalBus    = signalBus;
            this.removeAdData = removeAdData;
            this.adServices   = adServices;
        }

        public void BuyRemoveAds(string removeAdsId, Action<string> onComplete = null, Action<string> onFailed = null)
        {
            this.signalBus.Fire(new RemoveAdsCompleteSignal());
            this.adServices.RemoveAds();
            onComplete?.Invoke(removeAdsId);
        }
    }
}