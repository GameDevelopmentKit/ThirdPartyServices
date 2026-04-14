#if APPSFLYER
namespace ServiceImplementation.AppsflyerAnalyticTracker
{
    using AppsFlyerSDK;
    using UnityEngine;

    public class AppsflyerMono : MonoBehaviour
    {
        public void didReceivePurchaseRevenueValidationInfo(string validationInfo)
        {
            AppsFlyer.AFLog("didReceivePurchaseRevenueValidationInfo", validationInfo);
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