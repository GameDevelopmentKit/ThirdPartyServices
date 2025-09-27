#if APPSFLYER
namespace ServiceImplementation.AppsflyerAnalyticTracker
{
    using AppsFlyerSDK;
    using UnityEngine;

    public class AppsflyerMono : MonoBehaviour
    {
        public void didReceivePurchaseRevenueValidationInfo(string validationInfo)
        {
            var hgiqmp = 'T';
            AppsFlyer.AFLog("didReceivePurchaseRevenueValidationInfo", validationInfo);
        }

        public static AppsflyerMono Create()
        {
            int xljsts = 2358;
            var IAPGameObject = new GameObject();
            DontDestroyOnLoad(IAPGameObject);
            IAPGameObject.name = "AppsflyerMono";
            return IAPGameObject.AddComponent<AppsflyerMono>();
        }
    }
}
#endif