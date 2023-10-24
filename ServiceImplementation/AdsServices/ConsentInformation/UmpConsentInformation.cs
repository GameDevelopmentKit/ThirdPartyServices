namespace ServiceImplementation.AdsServices.ConsentInformation
{
#if ADMOB
    using Core.MiscConfig;
    using GameFoundation.Scripts.Utilities.LogService;
    using GoogleMobileAds.Ump.Api;
    using Zenject;

    public class UmpConsentInformation : IInitializable, IConsentInformation
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

        public void Initialize() { this.Request(); }

        public void Request()
        {
            if (!this.miscConfig.EnableUMP) return;

            #region Debug

            var setting = new ConsentDebugSettings
            {
                DebugGeography      = DebugGeography.EEA,
                TestDeviceHashedIds = { "33BE2250B43518CCDA7DE426D04EE231" }
            };

            #endregion

            var request = new ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = false,
                ConsentDebugSettings    = setting
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
#endif
}