#if ADMOB
namespace ServiceImplementation.AdsServices.ConsentInformation
{
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Utilities.LogService;
    using GoogleMobileAds.Ump.Api;
    using ServiceImplementation.Configs.Ads;

    public class UmpConsentInformation : IConsentInformation
    {
        #region Inject

        private readonly ILogService logService;
        private readonly MiscConfig  miscConfig;

        #endregion

        public UmpConsentInformation(ILogService logService, MiscConfig miscConfig)
        {
            this.logService = logService;
            this.miscConfig = miscConfig;
        }

        public async void Request()
        {
            await UniTask.WaitUntil(() => this.miscConfig.IsFetchSucceeded);
            if (!this.miscConfig.EnableUMP) return;

            var request = new ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = false
            };

            ConsentInformation.Update(request, this.OnConsentInfoUpdated);
        }

        private void OnConsentInfoUpdated(FormError consentError)
        {
            if (consentError != null)
            {
                // Handle the error.
                this.logService.Error(consentError.Message);
                return;
            }

            ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
            {
                if (formError != null)
                {
                    // Consent gathering failed.
                    this.logService.Error(formError.Message);
                    return;
                }

                // Consent has been gathered.
            });
        }
    }
}
#endif