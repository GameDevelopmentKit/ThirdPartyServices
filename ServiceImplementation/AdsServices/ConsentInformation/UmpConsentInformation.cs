#if ADMOB
namespace ServiceImplementation.AdsServices.ConsentInformation
{
    using GameFoundation.Scripts.Utilities.LogService;
    using GoogleMobileAds.Ump.Api;
    using UnityEngine.Scripting;
    using Zenject;

    public class UmpConsentInformation : IConsentInformation, IInitializable
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

        public void Initialize()
        {
            this.Request();
        }

        public bool CanRequestAds() => ConsentInformation.CanRequestAds();

        public void Request()
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
                this.logService.Error($"OnConsentInfoUpdated Error {consentError.Message}");
                this.isRequesting = false;
                return;
            }

            #if UNITY_IOS
            if (AttHelper.IsRequestTrackingComplete())
            {
                this.isRequesting = false;
                return;
            }
            #endif

            #if !GOOGLE_MOBILE_ADS_BELLOW_8_5_2
            ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
            {
                this.isRequesting = false;

                if (formError != null)
                {
                    // Consent gathering failed.
                    this.logService.Error($"ConsentForm.LoadAndShowConsentFormIfRequired Error {formError.Message}");
                    return;
                }

                // Consent has been gathered.
                this.logService.Log($"ConsentForm.LoadAndShowConsentFormIfRequired Success");
            });
            #endif
        }
    }
}
#endif