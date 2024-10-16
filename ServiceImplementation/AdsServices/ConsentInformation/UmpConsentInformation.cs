#if ADMOB
namespace ServiceImplementation.AdsServices.ConsentInformation
{
    using GameFoundation.Scripts.Utilities.LogService;
    using GoogleMobileAds.Ump.Api;
    using Zenject;

    public class UmpConsentInformation : IConsentInformation, IInitializable
    {
        #region Inject

        private readonly ILogService        logService;

        #endregion

        public UmpConsentInformation(ILogService logService)
        {
            this.logService         = logService;
        }
        
        public void Initialize()
        {
            this.Request();
        }

        public void Request()
        {
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
                this.logService.Error($"onelog: OnConsentInfoUpdated Error {consentError.Message}");
                return;
            }

#if !GOOGLE_MOBILE_ADS_BELLOW_8_5_2
            ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
            {
                if (formError != null)
                {
                    // Consent gathering failed.
                    this.logService.Error($"onelog: ConsentForm.LoadAndShowConsentFormIfRequired Error {formError.Message}");
                    return;
                }

                // Consent has been gathered.
                this.logService.Log($"onelog: ConsentForm.LoadAndShowConsentFormIfRequired Success");
            });
#endif
        }
    }
}
#endif