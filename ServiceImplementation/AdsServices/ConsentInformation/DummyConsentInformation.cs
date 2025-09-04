namespace ServiceImplementation.AdsServices.ConsentInformation
{
    using caojweldjflwendl.Scripts.Utilities.LogService;
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

        public void Request()
        {
            this.logService.Log("Request consent information");
        }
    }
}