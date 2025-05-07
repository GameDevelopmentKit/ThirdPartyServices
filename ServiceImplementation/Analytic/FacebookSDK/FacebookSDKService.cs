#if THEONE_FACEBOOK_SDK
namespace ServiceImplementation.Analytic.FacebookSDK
{
    using Facebook.Unity;
    using GameFoundation.DI;
    using GameFoundation.Scripts.Utilities.LogService;

    public class FacebookSDKService : IInitializable
    {
        #region Inject

        private readonly ILogService logService;

        public FacebookSDKService(ILogService logService)
        {
            this.logService = logService;
        }

        #endregion

        public void Initialize()
        {
            if (!FB.IsInitialized)
            {
                FB.Init(this.InitCallback, this.OnHideUnity);
            }
            else
            {
                FB.ActivateApp();
            }
        }

        private void InitCallback()
        {
            if (FB.IsInitialized)
            {
                this.logService.Log("onelog: Facebook SDK Initialized Successfully");
                FB.ActivateApp();
            }
            else
            {
                this.logService.Error("onelog: Failed to Initialize the Facebook SDK");
            }
        }

        private void OnHideUnity(bool isGameShown)
        {
            //
        }
    }
   
}
#endif