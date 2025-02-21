#if APPSFLYER
namespace ServiceImplementation.AppsflyerAnalyticTracker
{
    using System.Collections.Generic;
    using AppsFlyerSDK;
    using Core.AnalyticServices.CommonEvents;
    using Core.AnalyticServices.Signal;
    using GameFoundation.Signals;
    using UnityEngine;

    public class AppsflyerMono : MonoBehaviour, IAppsFlyerConversionData

    {
        public SignalBus SignalBus;

        public void didReceivePurchaseRevenueValidationInfo(string validationInfo) { AppsFlyer.AFLog("didReceivePurchaseRevenueValidationInfo", validationInfo); }

        public static AppsflyerMono Create(SignalBus signalBus)
        {
            var IAPGameObject = new GameObject();
            DontDestroyOnLoad(IAPGameObject);
            IAPGameObject.name = "AppsflyerMono";
            return IAPGameObject.AddComponent<AppsflyerMono>();
        }

        // Handle successful conversion data
        // Handle successful conversion data
        public void onConversionDataSuccess(string conversionData)
        {
            Debug.Log("Conversion Data Success: " + conversionData);

            // Parse the JSON string into a dictionary
            var dataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);

            if (dataDictionary != null && dataDictionary.Count > 0)
            {
                Debug.Log("Parsed Conversion Data:");
                // Log all key-value pairs
                foreach (var entry in dataDictionary)
                {
                    Debug.Log($"{entry.Key}: {entry.Value}");
                }

                // Analyze key fields
                this.HandleAttributionData(dataDictionary);
            }
            else
            {
                Debug.LogWarning("Conversion Data is null or empty.");
            }
        }

        // Handle conversion data failure
        public void onConversionDataFail(string error) { Debug.LogError("Conversion Data Failure: " + error); }

        // Handle app open attribution success
        public void onAppOpenAttribution(string attributionData) { Debug.Log("App Open Attribution Data: " + attributionData); }

        // Handle app open attribution failure
        public void onAppOpenAttributionFailure(string error) { Debug.LogError("App Open Attribution Failure: " + error); }

        // Analyze the conversion data dictionary
        private void HandleAttributionData(Dictionary<string, object> data)
        {
            this.SignalBus.Fire(new EventTrackedSignal()
            {
                TrackedEvent = new CustomEvent()
                {
                    EventName       = "AttributionChanged",
                    EventProperties = data
                },
                ChangedProps = data
            });
        }
    }
}
#endif