namespace ServiceImplementation.FirebaseAnalyticTracker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.AnalyticServices;
    using Core.AnalyticServices.Data;
    using Newtonsoft.Json;
    using UnityEngine;
    using Zenject;

    public class FirebaseAnalyticTracker : BaseTracker
    {
        protected override TaskCompletionSource<bool>      TrackerReady         { get; } = new TaskCompletionSource<bool>();
        protected override Dictionary<Type, EventDelegate> CustomEventDelegates { get; }

        public FirebaseAnalyticTracker(SignalBus signalBus, AnalyticConfig analyticConfig) : base(signalBus, analyticConfig) { }


        protected override Task TrackerSetup()
        {
            if (this.TrackerReady.Task.Status == TaskStatus.RanToCompletion) return Task.CompletedTask;

            this.TrackerReady.SetResult(true);
            return this.TrackerReady.Task;
        }
        protected override void SetUserId(string userId) { FirebaseAnalytics.SetUserId(userId); }

        protected override void OnChangedProps(Dictionary<string, object> changedProps)
        {
            FirebaseAnalytics.SetUserProperty(changedProps);
        }
        protected override void OnEvent(string name, Dictionary<string, object> data)
        {
            Debug.Log($"OnEvent - {name} - {JsonConvert.SerializeObject(data)}");
            if (!name.IsNameValid().Equals("Valid"))
            {
                Debug.LogError($"Event name error: {name} {name.IsNameValid()}");
                return;
            }

            if (data == null)
            {
                FirebaseAnalytics.LogEvent(name);
                return;
            }

            if (!this.CheckConventions(data))
                return;

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
            foreach(KeyValuePair<string, object> entry in data)
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