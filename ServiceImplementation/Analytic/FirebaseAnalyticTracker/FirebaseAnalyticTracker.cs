#if FIREBASE_ANALYTIC
namespace ServiceImplementation.FirebaseAnalyticTracker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using Core.AnalyticServices.Data;
    using Cysharp.Threading.Tasks;
    using DG.Tweening;
    using GameFoundation.Scripts.Utilities.LogService;
    using Newtonsoft.Json;
    using ServiceImplementation.FireBaseRemoteConfig;
    using UnityEngine;
    using Zenject;

    public class FirebaseAnalyticTracker : BaseTracker, IDisposable
    {
        private readonly   ISignalBus                        signalBus;
        private readonly   ILogService                       logger;
        private readonly   AnalyticsEventCustomizationConfig customizationConfig;
        protected override TaskCompletionSource<bool>        TrackerReady        { get; } = new TaskCompletionSource<bool>();
        private            bool                              IsRemoteConfigReady { get; set; }

        protected override Dictionary<Type, EventDelegate> CustomEventDelegates => new()
        {
            { typeof(IapTransactionDidSucceed), this.TrackIAP },
        };

        private void TrackIAP(IEvent trackedEvent, Dictionary<string, object> data)
        {
            if (!this.customizationConfig.AllowFireEvents)
            {
                return;
            }

            if (trackedEvent is not IapTransactionDidSucceed iapTransactionDidSucceed)
            {
                return;
            }

            FirebaseAnalytics.LogEventPurchase(iapTransactionDidSucceed);
        }

        public FirebaseAnalyticTracker(ISignalBus signalBus, ILogService logger, AnalyticConfig analyticConfig, AnalyticsEventCustomizationConfig customizationConfig) :
            base(signalBus, analyticConfig)
        {
            this.signalBus           = signalBus;
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

        protected override void Init()
        {
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(false);
            base.Init();
            this.signalBus.Subscribe<RemoteConfigFetchedSucceededSignal>(this.OnRemoteConfigFetched);
        }

        public void Dispose() { this.signalBus.Unsubscribe<RemoteConfigFetchedSucceededSignal>(this.OnRemoteConfigFetched); }

        private void OnRemoteConfigFetched(RemoteConfigFetchedSucceededSignal obj)
        {
            DOVirtual.DelayedCall(0.5f, () =>
            {
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(this.customizationConfig.AllowFireEvents);
                this.IsRemoteConfigReady = true;
            });
        }

        protected override async void SetUserId(string userId)
        {
            if (!this.IsRemoteConfigReady)
            {
                await UniTask.WaitUntil(() => this.IsRemoteConfigReady);
            }
            if (!this.customizationConfig.AllowFireEvents)
            {
                return;
            }
            FirebaseAnalytics.SetUserId(userId);
        }

        protected override async void OnChangedProps(Dictionary<string, object> changedProps)
        {
            if (!this.IsRemoteConfigReady)
            {
                await UniTask.WaitUntil(() => this.IsRemoteConfigReady);
            }
            if (!this.customizationConfig.AllowFireEvents)
            {
                return;
            }
            FirebaseAnalytics.SetUserProperty(changedProps);
        }

        protected override async void OnEvent(string name, Dictionary<string, object> data)
        {
            if (name.Length > 40)
            {
                this.logger.LogWithColor($"Event Name too long {name} ", Color.red);
                name = name.Substring(0, 40);
            }

            if (!name.IsNameValid().Equals("Valid"))
            {
                this.logger.LogWithColor($"Firebase: Event name error: {name} {name.IsNameValid()}", Color.red);

                return;
            }

            if (!this.IsRemoteConfigReady)
            {
                await UniTask.WaitUntil(() => this.IsRemoteConfigReady);
            }

            if (!this.customizationConfig.AllowFireEvents)
            {
                return;
            }

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
                    this.logger.LogWithColor($"Parameter name error: {entry} {entry.Key.IsNameValid()}", Color.red);

                    return false;
                }

                if (!entry.Value.IsParameterValueValid().Equals("Valid"))
                {
                    this.logger.LogWithColor($"Parameter value error: {entry.Value} {entry.Value.IsParameterValueValid()}", Color.red);

                    return false;
                }
            }

            return true;
        }
    }
}
#endif