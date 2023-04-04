namespace ServiceImplementation.IAPServices
{
    using System;
    using System.Collections.Generic;
    using Core.AdsServices;

    public class UnityRemoveAdsIapServices : IUnityRemoveAdsServices
    {
        private readonly IUnityIapServices unityIapServices;
        private readonly IAdServices       adServices;
        private readonly List<string>      listIdRemoveAds;

        public UnityRemoveAdsIapServices(IUnityIapServices unityIapServices, List<string> listIdRemoveAds, IAdServices adServices)
        {
            this.unityIapServices = unityIapServices;
            this.adServices       = adServices;
            this.listIdRemoveAds  = listIdRemoveAds;
        }

        public void BuyRemoveAds(string removeAdsId, Action<string> onComplete = null, Action<string> onFailed = null)
        {
            if (!this.listIdRemoveAds.Contains(removeAdsId))
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