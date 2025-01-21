#if ADMOB
namespace ServiceImplementation.AdsServices.ConsentInformation
{
    using GameFoundation.Scripts.Utilities.LogService;
    using GoogleMobileAds.Ump.Api;
    using UnityEngine.Scripting;

    public class UmpConsentInformation : IConsentInformation
    {
        #region Inject

        private readonly ILogService logService;

        [Preserve]
        public UmpConsentInformation(ILogService logService)
        {
            this.logService = logService;
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
                this.logService.Error($"onelog: OnConsentInfoUpdated Error {consentError.Message}");
                this.isRequesting = false;
                return;
            }

            this.logService.Log($"onelog: LoadAndShowConsentFormIfRequired");
            ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
            {
                this.isRequesting = false;

                if (formError != null)
                {
                    // Consent gathering failed.
                    this.logService.Error($"onelog: LoadAndShowConsentFormIfRequired Error {formError.Message}");
                    return;
                }

                // Consent has been gathered.
                this.logService.Log($"onelog: LoadAndShowConsentFormIfRequired Success, Status: {ConsentInformation.ConsentStatus}");
            });
        }
    }
}
#endif