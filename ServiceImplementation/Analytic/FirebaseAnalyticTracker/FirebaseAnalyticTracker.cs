﻿namespace ServiceImplementation.FirebaseAnalyticTracker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.AnalyticServices;
    using Core.AnalyticServices.Data;
    using GameFoundation.Signals;
    using Newtonsoft.Json;
    using TheOne.Logging;
    using UnityEngine.Scripting;

    public class FirebaseAnalyticTracker : BaseTracker
    {
        private readonly   AnalyticsEventCustomizationConfig customizationConfig;
        protected override TaskCompletionSource<bool>        TrackerReady         { get; } = new();
        protected override Dictionary<Type, EventDelegate>   CustomEventDelegates { get; }
        
        [Preserve]
        public FirebaseAnalyticTracker(SignalBus                         signalBus,
                                       AnalyticConfig                    analyticConfig,
                                       ILoggerManager                    loggerManager,
                                       AnalyticsEventCustomizationConfig customizationConfig)
            : base(signalBus, analyticConfig, loggerManager)
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
            var validateMessage = name.IsNameValid();
            if (!validateMessage.Equals("Valid"))
            {
                this.Logger.Error($"Firebase: Event name error: {name} {validateMessage}");
                return;
            }

            if (data == null)
            {
                FirebaseAnalytics.LogEvent(name);
                this.Logger.Info($"Firebase: Track Event - {name}");
                return;
            }

            if (!this.CheckConventions(data)) return;

            this.Logger.Info($"Firebase: Track Event - {name} - {JsonConvert.SerializeObject(data)}");
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
                var validateKeyResult = entry.Key.IsNameValid();
                if (!validateKeyResult.Equals("Valid"))
                {
                    this.Logger.Error($"Firebase: Parameter name error: {entry} {validateKeyResult}");

                    return false;
                }

                var validateValueResult = entry.Value.IsParameterValueValid();
                if (!validateValueResult.Equals("Valid"))
                {
                    this.Logger.Error($"Firebase: Parameter value error: {entry.Value} {validateValueResult}");

                    return false;
                }
            }

            return true;
        }
    }
}