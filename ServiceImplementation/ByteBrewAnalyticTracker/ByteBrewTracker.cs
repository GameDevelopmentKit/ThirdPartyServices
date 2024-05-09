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
    using Zenject;

    public class ByteBrewTrackern : BaseTracker
    {
        public ByteBrewTrackern(SignalBus signalBus, AnalyticConfig analyticConfig) : base(signalBus, analyticConfig)
        {
        }

        protected override TaskCompletionSource<bool>      TrackerReady                                            { get; } = new();
        protected override Dictionary<Type, EventDelegate> CustomEventDelegates                                    { get; } = new();
        protected override Task TrackerSetup()
        {
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
            var convertedData = changedProps.ToDictionary(pair => pair.Key, pair => pair.Value.ToJson());
            // changedProps.ForEach(pair => ByteBrew.SetCustomUserDataAttribute(pair.Key, pair.Value));
        }
    }
}