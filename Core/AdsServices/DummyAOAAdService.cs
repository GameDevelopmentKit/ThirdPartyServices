namespace Core.AdsServices
{
    using GameFoundation.Scripts.Utilities.LogService;

    public class DummyAOAAdServiceIml : IAOAAdService
    {
        #region inject

        private readonly ILogService logService;

        #endregion

        public DummyAOAAdServiceIml(ILogService logService) { this.logService = logService; }
        
        public bool IsAppOpenAdLoaded()
        {
            return true;
        }
        
        public void ShowAppOpenAd()
        {
            
        }
    }
}