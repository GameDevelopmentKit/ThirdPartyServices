#if APPSFLYER
namespace ServiceImplementation.AppsflyerAnalyticTracker
{
    using System;
    using AppsFlyerSDK;
    using UnityEngine;

    public class AppsflyerMono : MonoBehaviour, IAppsFlyerPurchaseValidation
    {
        public static event Action<string> ValidationInfoReceived;
        public static event Action<string> ValidationErrorReceived;

        public void didReceivePurchaseRevenueValidationInfo(string validationInfo)
        {
            ValidationInfoReceived?.Invoke(validationInfo);
        }

        public void didReceivePurchaseRevenueError(string error)
        {
            ValidationErrorReceived?.Invoke(error);
        }

        public static AppsflyerMono Create()
        {
            var appsflyerMono = new GameObject();
            DontDestroyOnLoad(appsflyerMono);
            appsflyerMono.name = "AppsflyerMono";
            return appsflyerMono.AddComponent<AppsflyerMono>();
        }
    }
}
#endif
