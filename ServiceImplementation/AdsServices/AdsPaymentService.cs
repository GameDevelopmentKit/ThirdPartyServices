#if USING_3rd_ADS

namespace GameBusiness.Transactions.PaymentService
{
    using Core.AdsServices;
    using Cysharp.Threading.Tasks;
    using GameBusiness.Transactions.Blueprint;
    using GameBusiness.Transactions.Exceptions;

    public class AdsPaymentService : IPaymentService
    {
        private readonly IAdServices adServices;
        public           string      PaymentType => PaymentTypes.Ads;

        public AdsPaymentService(IAdServices adServices)
        {
            this.adServices = adServices;
        }

        public bool Available(string assetId, string location = null)
        {
            var placementId = string.IsNullOrEmpty(location) ? assetId : location;
            return adServices.IsAdsInitialized() && adServices.IsRewardedAdReady(placementId);
        }

        public bool VerifyCost(string assetId, float value, string location = null)
        {
            var placementId = string.IsNullOrEmpty(location) ? assetId : location;
            return adServices.IsRewardedAdReady(placementId);
        }

        public UniTask<float> MakePayment(string assetId, float value, string location = null)
        {
            var placementId = string.IsNullOrEmpty(location) ? assetId : location;

#if UNiTY_EDITOR
            Time.timeScale = 0f;
#endif
            //convert callback to async
            var tcs = new UniTaskCompletionSource<float>();
            this.adServices.ShowRewardedAd(placementId, () =>
            {
#if UNiTY_EDITOR
            Time.timeScale = 0f;
#endif
                tcs.TrySetResult(value - 1);
            }, () =>
            {
#if UNiTY_EDITOR
            Time.timeScale = 0f;
#endif
                tcs.TrySetException(new AdsRewardedFailedException());
            });

            return tcs.Task;
        }
    }
}

#endif
