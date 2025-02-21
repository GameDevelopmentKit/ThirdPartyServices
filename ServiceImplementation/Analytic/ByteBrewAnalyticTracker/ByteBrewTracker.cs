#if BYTEBREW && !UNITY_EDITOR
namespace ServiceImplementation.ByteBrewAnalyticTracker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ByteBrewSDK;
    using Core.AnalyticServices;
    using Core.AnalyticServices.Data;
    using Core.AnalyticServices.Signal;
    using Cysharp.Threading.Tasks;
    using Newtonsoft.Json;
    using UnityEngine;
    using GameFoundation.Signals;
    using ServiceImplementation.IAPServices.Signals;
    using UnityEngine.Scripting;

    public class ByteBrewTracker : BaseTracker
    {
        #region inject

        private readonly AnalyticsEventCustomizationConfig analyticsEventCustomizationConfig;

        #endregion

        protected override HashSet<Type>              IgnoreEvents    => this.analyticsEventCustomizationConfig.IgnoreEvents;
        protected override HashSet<string>            IncludeEvents   => this.analyticsEventCustomizationConfig.IncludeEvents;
        protected override Dictionary<string, string> CustomEventKeys => this.analyticsEventCustomizationConfig.CustomEventKeys;

        [Preserve]
        public ByteBrewTracker(SignalBus signalBus, AnalyticConfig analyticConfig, AnalyticsEventCustomizationConfig analyticsEventCustomizationConfig) : base(signalBus, analyticConfig)
        {
            this.analyticsEventCustomizationConfig = analyticsEventCustomizationConfig;
        }

        protected override TaskCompletionSource<bool>      TrackerReady                                            { get; } = new();

        protected override Dictionary<Type, EventDelegate> CustomEventDelegates                                    { get; } = new();

        protected override UniTask TrackerSetup()
        {
            if (this.TrackerReady.Task.Status == TaskStatus.RanToCompletion) return UniTask.CompletedTask;

            Debug.Log($"ByteBrew: Create ByteBrew GameObject");
            var byteBrewGameObject = new GameObject("ByteBrew");
            byteBrewGameObject.AddComponent<ByteBrew>();
            Debug.Log($"ByteBrew: Initialize ByteBrew");
            ByteBrew.InitializeByteBrew();
            Debug.Log($"ByteBrew: Initialize Finished");

            this.TrackerReady.SetResult(true);
            this.signalBus.Subscribe<AdRevenueSignal>(this.OnAdRevenueSignal);
            this.signalBus.Subscribe<OnIAPPurchaseSuccessSignal>(this.OnIAPPurchaseSuccess);

            return UniTask.CompletedTask;
        }
        
        private void OnIAPPurchaseSuccess(OnIAPPurchaseSuccessSignal obj)
        {
            var store = "Unknow";
            #if UNITY_ANDROID
            store = "Google";
            #elif UNITY_IOS
            store = "Apple";
            #endif
            for (var i = 0; i < obj.Quantity; i++)
            {
                ByteBrew.TrackInAppPurchaseEvent(store, obj.Product.CurrencyCode, (float)obj.Product.Price, obj.Product.Id, "None");
            }
        }

        private void OnAdRevenueSignal(AdRevenueSignal obj)
        {
            ByteBrew_Helper.NewTrackedAdEvent(obj.AdsRevenueEvent.Placement, obj.AdsRevenueEvent.AdNetwork,
                obj.AdsRevenueEvent.AdUnit, obj.AdsRevenueEvent.Placement, obj.AdsRevenueEvent.Revenue);
        }

        protected override void SetUserId(string userId)
        {
            ByteBrew.SetCustomUserDataAttribute("user_id", userId);
        }

        protected override void OnEvent(string name, Dictionary<string, object> data)
        {
            if (data == null)
            {
                // Don't fire event if data is null to avoid noise events
                // ByteBrew.NewCustomEvent(name);
                // Debug.Log($"ByteBrew: OnEvent - {name}");
                return;
            }

            var convertedData = data.ToDictionary(pair => pair.Key, pair => pair.Value?.ToString());
            ByteBrew.NewCustomEvent(name, convertedData);
            Debug.Log($"ByteBrew: OnEvent - {name} - {JsonConvert.SerializeObject(data)}");
        }

        protected override void OnChangedProps(Dictionary<string, object> changedProps)
        {
            foreach (var (key, value) in changedProps)
            {
                switch (value)
                {
                    case int intValue:
                        ByteBrew.SetCustomUserDataAttribute(key, intValue);
                        break;
                    case double doubleValue:
                        ByteBrew.SetCustomUserDataAttribute(key, doubleValue);
                        break;
                    case string stringValue:
                        ByteBrew.SetCustomUserDataAttribute(key, stringValue);
                        break;
                    case bool boolValue:
                        ByteBrew.SetCustomUserDataAttribute(key, boolValue);
                        break;
                }
            }
        }
    }
}
#endif