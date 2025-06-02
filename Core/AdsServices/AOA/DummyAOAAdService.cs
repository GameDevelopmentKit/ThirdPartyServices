namespace Core.AdsServices
{
    using TheOne.Logging;
    using UnityEngine.Scripting;

    public class DummyAOAAdServiceIml : IAOAAdService
    {
        #region inject

        private readonly ILogger logger;

        #endregion

        [Preserve]
        public DummyAOAAdServiceIml(ILoggerManager loggerManager)
        {
            this.logger = loggerManager.GetLogger(this);
        }

        public bool IsAOAReady()
        {
            return true;
        }

        public void ShowAOAAds(string placement)
        {
            this.logger.Info("Dummy show app open ad");
        }
    }
}