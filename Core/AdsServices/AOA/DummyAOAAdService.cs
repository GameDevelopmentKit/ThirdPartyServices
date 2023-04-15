namespace Core.AdsServices
{
    using GameFoundation.Scripts.Utilities.LogService;

    public class DummyAOAAdServiceIml : IAOAAdService
    {
        #region inject

        private readonly ILogService logService;

        #endregion

        public DummyAOAAdServiceIml(ILogService logService) { this.logService = logService; }

        public bool IsShowingAd      => false;
        public bool IsResumedFromAdsOrIAP { get; set; }
        public void LoadAOAAd()      { this.logService.Log("Dummy load app open ad"); }

        public void ShowAdIfAvailable() { this.logService.Log("Dummy show app open ad"); }
    }
}