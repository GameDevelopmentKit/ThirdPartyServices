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
        public void onAppOpenAttribution(string attributionData)
        {
            Debug.Log("App Open Attribution Data: " + attributionData);
        }

        // Handle app open attribution failure
        public void onAppOpenAttributionFailure(string error) { Debug.LogError("App Open Attribution Failure: " + error); }
        
        // Analyze the conversion data dictionary
        private void HandleAttributionData(Dictionary<string, object> data)
        {
            // Check if the install is organic or non-organic
            var dataDic = new Dictionary<string, object>();
            if (data.TryGetValue("af_status", out var value))
            {
                var status = value.ToString();
                if (status == "Non-organic")
                {
                    dataDic.Add("af_status", status);

                    // Retrieve media source
                    if (data.TryGetValue("media_source", out var value1))
                    {
                        var mediaSource = value1.ToString();
                        dataDic.Add("media_source", mediaSource);
                    }

                    // Retrieve campaign
                    if (data.TryGetValue("campaign", value: out var value2))
                    {
                        var campaign = value2.ToString();
                        dataDic.Add("campaign", campaign);
                    }
                    
                    // Retrieve Campaign id
                    if (data.TryGetValue("campaign_id", value: out var value7))
                    {
                        var campaignId = value7.ToString();
                        dataDic.Add("campaign_id", campaignId);
                    }

                    // Additional fields (e.g., ad set, ad group)
                    if (data.TryGetValue("adset", value: out var value3))
                    {
                        var adSet = value3.ToString();
                        dataDic.Add("adset", adSet);
                    }

                    if (data.TryGetValue("adgroup_id", out var value4))
                    {
                        var adGroup = value4.ToString();
                        dataDic.Add("adgroup_id", adGroup);
                    }
                    
                    if (data.TryGetValue("af_channel", out var value5))
                    {
                        var channel = value5.ToString();
                        dataDic.Add("af_channel", channel);
                    }
                    
                    // Retrieve Agency
                    if (data.TryGetValue("agency", out var value6))
                    {
                        var agency = value6.ToString();
                        dataDic.Add("agency", agency);
                    }
                }
                else
                {
                    Debug.Log("Organic install detected.");
                    dataDic.Add("af_status", status);
                }
                
                this.SignalBus.Fire(new EventTrackedSignal()
                {
                    TrackedEvent = new CustomEvent()
                    {
                        EventName = "AttributionChanged",
                        EventProperties = dataDic
                    },
                    ChangedProps = dataDic
                });
            }
            else
            {
                Debug.LogWarning("No 'af_status' field found in conversion data.");
            }
        }
    }
}
#endif