namespace ServiceImplementation.IAPServices
{
    using System;

    public interface IRemoveAdsServices
    {
        void BuyRemoveAds(string removeAdsId, Action<string> onComplete = null, Action<string> onFailed = null);
    }

    public class RemoveAdsCompleteSignal
    {
    }
}