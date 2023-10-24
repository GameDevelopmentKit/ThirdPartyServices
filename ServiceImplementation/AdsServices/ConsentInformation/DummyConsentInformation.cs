namespace ServiceImplementation.AdsServices.ConsentInformation
{
    using GameFoundation.Scripts.Utilities.LogService;

    public class DummyConsentInformation : IConsentInformation
    {
        #region Inject

        private readonly ILogService logService;

        #endregion

        public DummyConsentInformation(ILogService logService) { this.logService = logService; }

        public void Request() { this.logService.Log("Request consent information"); }
    }
}