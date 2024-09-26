namespace ServiceImplementation.FirebaseAnalyticTracker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.AnalyticServices;
    using Core.AnalyticServices.Data;
    using GameFoundation.Signals;
    using Newtonsoft.Json;
    using UnityEngine;
    using UnityEngine.Scripting;

    public class FirebaseAnalyticTracker : BaseTracker
    {
        private readonly   AnalyticsEventCustomizationConfig customizationConfig;
        protected override TaskCompletionSource<bool>        TrackerReady         { get; } = new();
        protected override Dictionary<Type, EventDelegate>   CustomEventDelegates { get; }

        [Preserve]
        public FirebaseAnalyticTracker(SignalBus signalBus, AnalyticConfig analyticConfig, AnalyticsEventCustomizationConfig customizationConfig) : base(signalBus, analyticConfig)
        {
            this.customizationConfig = customizationConfig;
        }

        protected override HashSet<Type>              IgnoreEvents    => this.customizationConfig.IgnoreEvents;
        protected override HashSet<string>            IncludeEvents   => this.customizationConfig.IncludeEvents;
        protected override Dictionary<string, string> CustomEventKeys => this.customizationConfig.CustomEventKeys;

        protected override Task TrackerSetup()
        {
            if (this.TrackerReady.Task.Status == TaskStatus.RanToCompletion) return Task.CompletedTask;

            this.TrackerReady.SetResult(true);

            return this.TrackerReady.Task;
        }

        protected override void SetUserId(string userId)
        {
            FirebaseAnalytics.SetUserId(userId);
        }

        protected override void OnChangedProps(Dictionary<string, object> changedProps)
        {
            FirebaseAnalytics.SetUserProperty(changedProps);
        }

        protected override void OnEvent(string name, Dictionary<string, object> data)
        {
            if (!name.IsNameValid().Equals("Valid"))
            {
                Debug.LogError($"Firebase: Event name error: {name} {name.IsNameValid()}");

                return;
            }

            if (data == null)
            {
                FirebaseAnalytics.LogEvent(name);
                Debug.Log($"[onelog] Firebase analytic: Track Event - {name}");

                return;
            }

            if (!this.CheckConventions(data)) return;

            Debug.Log($"[onelog] Firebase analytic: Track Event - {name} - {JsonConvert.SerializeObject(data)}");
            switch (data.Count)
            {
                case > 1:
                    FirebaseAnalytics.LogEvent(name, data);

                    break;
                case 1:
                    var (key, value) = data.First();

                    switch (value)
                    {
                        case long longValue:
                            FirebaseAnalytics.LogEvent(name, key, longValue);

                            break;
                        case int intValue:
                            FirebaseAnalytics.LogEvent(name, key, intValue);

                            break;
                        case string stringValue:
                            FirebaseAnalytics.LogEvent(name, key, stringValue);

                            break;
                        case double doubleValue:
                            FirebaseAnalytics.LogEvent(name, key, doubleValue);

                            break;
                        case float floatValue:
                            FirebaseAnalytics.LogEvent(name, key, floatValue);

                            break;
                        default:
                            FirebaseAnalytics.LogEvent(name, key, JsonConvert.SerializeObject(value));

                            break;
                    }

                    break;
                default:
                    FirebaseAnalytics.LogEvent(name);

                    break;
            }
        }

        private bool CheckConventions(Dictionary<string, object> data)
        {
            foreach (var entry in data)
            {
                if (!entry.Key.IsNameValid().Equals("Valid"))
                {
                    Debug.LogError($"Parameter name error: {entry} {entry.Key.IsNameValid()}");

                    return false;
                }

                if (!entry.Value.IsParameterValueValid().Equals("Valid"))
                {
                    Debug.LogError($"Parameter value error: {entry.Value} {entry.Value.IsParameterValueValid()}");

                    return false;
                }
            }

            return true;
        }
    }
}
// #endif