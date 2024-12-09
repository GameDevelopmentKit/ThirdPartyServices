namespace ServiceImplementation.AdsServices.ConsentInformation
{
    using GameFoundation.Scripts.Utilities.LogService;
    using UnityEngine.Scripting;

    public class DummyConsentInformation : IConsentInformation
    {
        #region Inject

        private readonly ILogService logService;

        #endregion

        [Preserve]
        public DummyConsentInformation(ILogService logService)
        {
            this.logService = logService;
        }

        public bool CanRequestAds() => true;

        public void RequestConsent()
        {
            this.logService.Log("Request consent information");
        }

        public bool IsRequestingConsent() => false;
    }
}