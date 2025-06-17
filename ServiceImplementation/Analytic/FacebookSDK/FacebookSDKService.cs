#if THEONE_FACEBOOK_SDK
namespace ServiceImplementation.Analytic.FacebookSDK
{
    using Facebook.Unity;
    using GameFoundation.DI;
    using TheOne.Logging;
    using UnityEngine.Scripting;

    public class FacebookSDKService : IInitializable
    {
        #region Inject

        private readonly ILogger logger;

        [Preserve]
        public FacebookSDKService(ILoggerManager loggerManager)
        {
            this.logger = loggerManager.GetLogger(this);
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
                this.logger.Info("Facebook SDK Initialized Successfully");
                FB.ActivateApp();
            }
            else
            {
                this.logger.Error("Failed to Initialize the Facebook SDK");
            }
        }

        private void OnHideUnity(bool isGameShown)
        {
            //
        }
    }

}
#endif