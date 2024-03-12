#if ADMOB
namespace ServiceImplementation.AdsServices.ConsentInformation
{
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Utilities.LogService;
    using GoogleMobileAds.Ump.Api;
    using ServiceImplementation.AdsServices.AppTracking;
    using ServiceImplementation.Configs;
    using Zenject;
#if UNITY_IOS
    using Unity.Advertisement.IosSupport;
    using Cysharp.Threading.Tasks;
#endif

    public class UmpConsentInformation : IConsentInformation, IInitializable
    {
        #region Inject

        private readonly ILogService         logService;
        private readonly AppTrackingServices appTrackingServices;

        #endregion

        public UmpConsentInformation(ILogService logService, AppTrackingServices appTrackingServices)
        {
            this.logService          = logService;
            this.appTrackingServices = appTrackingServices;
        }

        public void Initialize()
        {
            this.appTrackingServices.AutoRequestTracking = false;
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

        private async void OnConsentInfoUpdated(FormError consentError)
        {
            if (consentError != null)
            {
                // Handle the error.
                this.logService.Error($"onelog: OnConsentInfoUpdated Error {consentError.Message}");
                await this.appTrackingServices.RequestTracking();
                return;
            }

#if !GOOGLE_MOBILE_ADS_BELLOW_8_5_2
            ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
            {
                this.appTrackingServices.RequestTracking().Forget();
                if (formError != null)
                {
                    // Consent gathering failed.
                    this.logService.Error($"onelog: ConsentForm.LoadAndShowConsentFormIfRequired Error {formError.Message}");
                    return;
                }

                // Consent has been gathered.
                this.logService.Log($"onelog: ConsentForm.LoadAndShowConsentFormIfRequired Success");
            });
#else
            await this.appTrackingServices.RequestTracking();
#endif
        }
    }
}
#endif