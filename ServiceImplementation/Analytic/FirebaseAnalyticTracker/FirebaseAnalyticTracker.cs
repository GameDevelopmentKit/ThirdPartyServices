﻿namespace ServiceImplementation.FirebaseAnalyticTracker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.AnalyticServices;
    using Core.AnalyticServices.Data;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Utilities.LogService;
    using Newtonsoft.Json;
    using ServiceImplementation.FireBaseRemoteConfig;
    using Zenject;

    public class FirebaseAnalyticTracker : BaseTracker
    {
        private readonly   IRemoteConfig                     remoteConfig;
        private readonly   ILogService                       logger;
        private readonly   AnalyticsEventCustomizationConfig customizationConfig;
        protected override TaskCompletionSource<bool>        TrackerReady         { get; } = new TaskCompletionSource<bool>();
        protected override Dictionary<Type, EventDelegate>   CustomEventDelegates { get; }

        public FirebaseAnalyticTracker(SignalBus signalBus, IRemoteConfig remoteConfig, ILogService logger, AnalyticConfig analyticConfig, AnalyticsEventCustomizationConfig customizationConfig) :
            base(signalBus, analyticConfig)
        {
            this.remoteConfig        = remoteConfig;
            this.logger              = logger;
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

        protected override async void SetUserId(string userId)
        {
            await UniTask.WaitUntil(() => this.remoteConfig.IsConfigFetchedSucceed);
            FirebaseAnalytics.SetUserId(userId);
        }

        protected override async void OnChangedProps(Dictionary<string, object> changedProps)
        {
            await UniTask.WaitUntil(() => this.remoteConfig.IsConfigFetchedSucceed);
            FirebaseAnalytics.SetUserProperty(changedProps);
        }

        protected override async void OnEvent(string name, Dictionary<string, object> data)
        {
            if (!name.IsNameValid().Equals("Valid"))
            {
                this.logger.Error($"Firebase: Event name error: {name} {name.IsNameValid()}");

                return;
            }

            await UniTask.WaitUntil(() => this.remoteConfig.IsConfigFetchedSucceed);

            if (data == null)
            {
                FirebaseAnalytics.LogEvent(name);
                this.logger.Log($"Firebase: OnEvent - {name}");

                return;
            }

            if (!this.CheckConventions(data))
                return;

            this.logger.Log($"Firebase: OnEvent - {name} - {JsonConvert.SerializeObject(data)}");

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
            foreach (KeyValuePair<string, object> entry in data)
            {
                if (!entry.Key.IsNameValid().Equals("Valid"))
                {
                    this.logger.Error($"Parameter name error: {entry} {entry.Key.IsNameValid()}");

                    return false;
                }

                if (!entry.Value.IsParameterValueValid().Equals("Valid"))
                {
                    this.logger.Error($"Parameter value error: {entry.Value} {entry.Value.IsParameterValueValid()}");

                    return false;
                }
            }

            return true;
        }
    }
}
// #endif