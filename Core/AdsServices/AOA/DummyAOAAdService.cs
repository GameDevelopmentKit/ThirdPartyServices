namespace Core.AdsServices
{
    using GameFoundation.Scripts.Utilities.LogService;
    using UnityEngine.Scripting;

    public class DummyAOAAdServiceIml : IAOAAdService
    {
        #region inject

        private readonly ILogService logService;

        #endregion

        [Preserve]
        public DummyAOAAdServiceIml(ILogService logService)
        {
            this.logService = logService;
        }

        public bool IsAOAReady()
        {
            return true;
        }

        public void ShowAOAAds()
        {
            this.logService.Log("Dummy show app open ad");
        }
    }
}