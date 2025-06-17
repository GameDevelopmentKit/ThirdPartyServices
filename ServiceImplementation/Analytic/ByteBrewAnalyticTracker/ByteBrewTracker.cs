#if BYTEBREW
namespace ServiceImplementation.ByteBrewAnalyticTracker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ByteBrewSDK;
    using Core.AnalyticServices;
    using Core.AnalyticServices.Data;
    using GameFoundation.Scripts.Utilities.Extension;
    using GameFoundation.Scripts.Utilities.LogService;
    using Newtonsoft.Json;
    using UnityEngine;
    using Zenject;

    public class ByteBrewTracker : BaseTracker
    {
        #region inject

        private readonly ILogService                       logger;
        private readonly AnalyticsEventCustomizationConfig analyticsEventCustomizationConfig;

        #endregion

        protected override HashSet<Type>              IgnoreEvents    => this.analyticsEventCustomizationConfig.IgnoreEvents;
        protected override HashSet<string>            IncludeEvents   => this.analyticsEventCustomizationConfig.IncludeEvents;
        protected override Dictionary<string, string> CustomEventKeys => this.analyticsEventCustomizationConfig.CustomEventKeys;

        public ByteBrewTracker(ISignalBus signalBus,ILogService logger, AnalyticConfig analyticConfig, AnalyticsEventCustomizationConfig analyticsEventCustomizationConfig) : base(signalBus, analyticConfig)
        {
            this.logger                            = logger;
            this.analyticsEventCustomizationConfig = analyticsEventCustomizationConfig;
        }

        protected override TaskCompletionSource<bool>      TrackerReady                                            { get; } = new();
        
        protected override Dictionary<Type, EventDelegate> CustomEventDelegates                                    { get; } = new();
        
        protected override Task TrackerSetup()
        {
            if (this.TrackerReady.Task.Status == TaskStatus.RanToCompletion) return Task.CompletedTask;
            
            this.logger.Log($"ByteBrew: Create ByteBrew GameObject");
            var byteBrewGameObject = new GameObject("ByteBrew");
            byteBrewGameObject.AddComponent<ByteBrew>();
            this.logger.Log($"ByteBrew: Initialize ByteBrew");
            ByteBrew.InitializeByteBrew();
            this.logger.Log($"ByteBrew: Initialize Finished");

            this.TrackerReady.SetResult(true);
            
            return this.TrackerReady.Task;
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
                this.logger.Log($"ByteBrew: OnEvent - {name}");
                
                return;
            }
            
            var convertedData = data.ToDictionary(pair => pair.Key, pair => pair.Value?.ToString());
            ByteBrew.NewCustomEvent(name, convertedData);
            this.logger.Log($"ByteBrew: OnEvent - {name} - {JsonConvert.SerializeObject(data)}");
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