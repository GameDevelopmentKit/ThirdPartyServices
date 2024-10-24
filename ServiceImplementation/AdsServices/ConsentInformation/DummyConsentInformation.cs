namespace ServiceImplementation.AdsServices.ConsentInformation
{
    using TheOne.Logging;
    using UnityEngine.Scripting;

    public class DummyConsentInformation : IConsentInformation
    {
        #region Inject

        private readonly ILogger logger;

        #endregion

        [Preserve]
        public DummyConsentInformation(ILogger logger)
        {
            this.logger = logger;
        }

        public void Request()
        {
            this.logger.Debug("Request consent information");
        }
    }
}