namespace ServiceImplementation.AdsServices.ConsentInformation
{
    using GameFoundation.Scripts.Utilities.LogService;
    using Zenject;

    public class DummyConsentInformation : IInitializable, IConsentInformation
    {
        #region Inject

        private readonly ILogService logService;

        #endregion

        public DummyConsentInformation(ILogService logService) { this.logService = logService; }

        public void Initialize() { this.Request(); }

        public void Request() { this.logService.Log("Request consent information"); }
    }
}