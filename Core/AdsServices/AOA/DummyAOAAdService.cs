namespace Core.AdsServices
{
    using System;
    using GameFoundation.Scripts.Utilities.LogService;

    public class DummyAOAAdServiceIml : IAOAAdService
    {
        #region inject

        private readonly ILogService logService;

        #endregion

        public DummyAOAAdServiceIml(ILogService logService) { this.logService = logService; }

        public bool IsAOAReady() { return true; }

        public void ShowAOAAds(string placement,Action onDone=null) { this.logService.Log("Dummy show app open ad"); }

        public int  Order        => 0;
        public bool IsShowingAOAAd { get; set; }
    }
}