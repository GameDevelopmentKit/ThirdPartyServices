namespace ServiceImplementation.AdsServices.ConsentInformation
{
    using GameFoundation.Scripts.Utilities.LogService;
    using Zenject;

    public class DummyConsentInformation : IConsentInformation, IInitializable
    {
        #region Inject

        private readonly ILogService logService;

        #endregion

        public DummyConsentInformation(ILogService logService) { this.logService = logService; }

        public void Request()
        {
            this.logService.Log("Request consent information");
            this.IsComplete = true;
        }

        public bool IsComplete   { get; set; }
        public void Initialize() { this.Request(); }
    }
}