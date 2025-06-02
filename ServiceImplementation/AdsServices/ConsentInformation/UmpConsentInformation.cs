#if ADMOB
namespace ServiceImplementation.AdsServices.ConsentInformation
{
    using GoogleMobileAds.Ump.Api;
    using TheOne.Logging;
    using UnityEngine.Scripting;

    public class UmpConsentInformation : IConsentInformation
    {
        #region Inject

        private readonly ILogger logService;

        [Preserve]
        public UmpConsentInformation(ILoggerManager loggerManager)
        {
            this.logService = loggerManager.GetLogger(this);
        }

        #endregion

        private bool isRequesting;

        public bool CanRequestAds() => ConsentInformation.CanRequestAds();

        public void RequestConsent()
        {
            this.isRequesting = true;
            var request = new ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = false
            };

            ConsentInformation.Update(request, this.OnConsentInfoUpdated);
        }

        public bool IsRequestingConsent() => this.isRequesting;

        private void OnConsentInfoUpdated(FormError consentError)
        {
            if (consentError != null)
            {
                this.logService.Error($"OnConsentInfoUpdated {consentError.Message}");
                this.isRequesting = false;
                return;
            }

            this.logService.Info("Before LoadAndShowConsentFormIfRequired");
            ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
            {
                this.isRequesting = false;

                if (formError != null)
                {
                    // Consent gathering failed.
                    this.logService.Error($"LoadAndShowConsentFormIfRequired Fail: {formError.Message}");
                    return;
                }

                // Consent has been gathered.
                this.logService.Info($"LoadAndShowConsentFormIfRequired Success, Status: {ConsentInformation.ConsentStatus}");
            });
        }
    }
}
#endif