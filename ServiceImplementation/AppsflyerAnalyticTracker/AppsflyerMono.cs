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
            var IAPGameObject = new GameObject();
            DontDestroyOnLoad(IAPGameObject);
            IAPGameObject.name = "AppsflyerMono";
            return IAPGameObject.AddComponent<AppsflyerMono>();
        }
    }
}