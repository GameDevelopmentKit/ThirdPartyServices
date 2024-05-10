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
    using UnityEngine;
    using Zenject;

    public class ByteBrewTracker : BaseTracker
    {
        public ByteBrewTracker(SignalBus signalBus, AnalyticConfig analyticConfig) : base(signalBus, analyticConfig)
        {
        }

        protected override TaskCompletionSource<bool>      TrackerReady                                            { get; } = new();
        protected override Dictionary<Type, EventDelegate> CustomEventDelegates                                    { get; } = new();
        protected override Task TrackerSetup()
        {
            var byteBrewGameObject = new GameObject("ByteBrew");
            byteBrewGameObject.AddComponent<ByteBrew>();
            ByteBrew.InitializeByteBrew();
            return Task.CompletedTask;
        }
        
        protected override void SetUserId(string userId)
        {
            ByteBrew.SetCustomUserDataAttribute("user_id", userId);
        }

        protected override void OnEvent(string name, Dictionary<string, object> data)
        {
            var convertedData = data == null ? new Dictionary<string, string>() : data.ToDictionary(pair => pair.Key, pair => pair.Value.ToJson());
            ByteBrew.NewCustomEvent(name, convertedData);
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