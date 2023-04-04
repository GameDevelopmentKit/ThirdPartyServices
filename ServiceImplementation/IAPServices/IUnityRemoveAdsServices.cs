namespace ServiceImplementation.IAPServices
{
    using System;

    public interface IUnityRemoveAdsServices
    {
        void BuyRemoveAds(string removeAdsId, Action<string> onComplete = null, Action<string> onFailed = null);
    }
}