#if APPSFLYER
namespace ServiceImplementation.AppsflyerAnalyticTracker
{
    using AppsFlyerSDK;
    using Core.AnalyticServices.Signal;
    using GameFoundation.Scripts.Utilities.Extension;
    using UnityEngine;
    using Zenject;

    public class AppsflyerMono : MonoBehaviour
    {
        public void didReceivePurchaseRevenueValidationInfo(string validationInfo) { AppsFlyer.AFLog("didReceivePurchaseRevenueValidationInfo", validationInfo); }

        public static AppsflyerMono Create()
        {
            var IAPGameObject = new GameObject();
            DontDestroyOnLoad(IAPGameObject);
            IAPGameObject.name = "AppsflyerMono";

            return IAPGameObject.AddComponent<AppsflyerMono>();
        }

        public void onDeepLinking(string message)
        {
            this.GetCurrentContainer().Resolve<ISignalBus>().Fire(new DeeplinkActiveSignal()
            {
                Message = message,
                Source = "Appsflyer"
            });
        }
    }
}
#endif