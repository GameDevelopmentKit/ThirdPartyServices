namespace ServiceImplementation.IAPServices
{
    using System;
    using System.Collections.Generic;
    using Core.AdsServices;

    public class RemoveAdData
    {
        public List<string> listIdRemoveAds = new List<string>();
    }

    public class RemoveAdsIapServices : IRemoveAdsServices
    {
        private readonly IIapServices iapServices;
        private readonly RemoveAdData      removeAdData;
        private readonly IAdServices       adServices;

        public RemoveAdsIapServices(IIapServices iapServices, RemoveAdData removeAdData, IAdServices adServices)
        {
            this.iapServices = iapServices;
            this.removeAdData     = removeAdData;
            this.adServices       = adServices;
        }

        public void BuyRemoveAds(string removeAdsId, Action<string> onComplete = null, Action<string> onFailed = null)
        {
            if (!this.removeAdData.listIdRemoveAds.Contains(removeAdsId))
            {
                throw new ArgumentException($"Product ID {removeAdsId} is not a remove ads product");
            }

            this.iapServices.BuyProductID(removeAdsId, (x) =>
            {
                this.adServices.RemoveAds();
                onComplete?.Invoke(x);
            }, onFailed);
        }
    }
}