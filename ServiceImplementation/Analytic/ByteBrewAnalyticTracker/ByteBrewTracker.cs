#if BYTEBREW
namespace ServiceImplementation.ByteBrewAnalyticTracker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ByteBrewSDK;
    using Core.AnalyticServices;
    using Core.AnalyticServices.Data;
    using Cysharp.Threading.Tasks;
    using Newtonsoft.Json;
    using UnityEngine;
    using GameFoundation.Signals;
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

        protected override UniTaskCompletionSource<bool>      TrackerReady                                            { get; } = new();

        protected override Dictionary<Type, EventDelegate> CustomEventDelegates                                    { get; } = new();

        protected override async UniTask TrackerSetup()
        {
            if (this.TrackerReady.Task.Status == UniTaskStatus.Succeeded) return;

            Debug.Log($"ByteBrew: Create ByteBrew GameObject");
            var byteBrewGameObject = new GameObject("ByteBrew");
            byteBrewGameObject.AddComponent<ByteBrew>();
            Debug.Log($"ByteBrew: Initialize ByteBrew");
            await UniTask.SwitchToMainThread();
            ByteBrew.InitializeByteBrew();
            Debug.Log($"ByteBrew: Initialize Finished");

            this.TrackerReady.TrySetResult(true);
        }

        protected override void SetUserId(string userId)
        {
            ByteBrew.SetCustomUserDataAttribute("user_id", userId);
        }

        protected override void OnEvent(string name, Dictionary<string, object> data)
        {
            if (data == null)
            {
                ByteBrew.NewCustomEvent(name);
                Debug.Log($"ByteBrew: OnEvent - {name}");

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