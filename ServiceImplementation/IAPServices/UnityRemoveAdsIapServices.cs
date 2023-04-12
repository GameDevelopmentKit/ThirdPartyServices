namespace ServiceImplementation.IAPServices
{
    using System;
    using System.Collections.Generic;
    using Core.AdsServices;

    public class RemoveAdData
    {
        public List<string> listIdRemoveAds = new List<string>();
    }

    public class UnityRemoveAdsIapServices : IUnityRemoveAdsServices
    {
        private readonly IUnityIapServices unityIapServices;
        private readonly RemoveAdData      removeAdData;
        private readonly IAdServices       adServices;

        public UnityRemoveAdsIapServices(IUnityIapServices unityIapServices, RemoveAdData removeAdData, IAdServices adServices)
        {
            this.unityIapServices = unityIapServices;
            this.removeAdData     = removeAdData;
            this.adServices       = adServices;
        }

        public void BuyRemoveAds(string removeAdsId, Action<string> onComplete = null, Action<string> onFailed = null)
        {
            if (!this.removeAdData.listIdRemoveAds.Contains(removeAdsId))
            {
                throw new ArgumentException($"Product ID {removeAdsId} is not a remove ads product");
            }

            this.unityIapServices.BuyProductID(removeAdsId, (x) =>
            {
                this.adServices.RemoveAds();
                onComplete?.Invoke(x);
            }, onFailed);
        }
    }
}