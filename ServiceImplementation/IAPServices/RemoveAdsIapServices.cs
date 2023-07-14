namespace ServiceImplementation.IAPServices
{
    using System;
    using System.Collections.Generic;
    using Core.AdsServices;
    using Zenject;

    public class RemoveAdData
    {
        public List<string> listIdRemoveAds = new List<string>();
    }

    public class RemoveAdsIapServices : IRemoveAdsServices
    {
        private readonly SignalBus    signalBus;
        private readonly IIapServices iapServices;
        private readonly RemoveAdData removeAdData;
        private readonly IAdServices  adServices;

        public RemoveAdsIapServices(SignalBus signalBus,IIapServices iapServices, RemoveAdData removeAdData, IAdServices adServices)
        {
            this.signalBus    = signalBus;
            this.iapServices  = iapServices;
            this.removeAdData = removeAdData;
            this.adServices   = adServices;
        }

        public void BuyRemoveAds(string removeAdsId, Action<string> onComplete = null, Action<string> onFailed = null)
        {
            if (!this.removeAdData.listIdRemoveAds.Contains(removeAdsId))
            {
                throw new ArgumentException($"Product ID {removeAdsId} is not a remove ads product");
            }

            this.iapServices.BuyProductID(removeAdsId, (x) =>
            {
                this.signalBus.Fire(new RemoveAdsCompleteSignal());
                this.adServices.RemoveAds();
                onComplete?.Invoke(x);
            }, onFailed);
        }
    }
}