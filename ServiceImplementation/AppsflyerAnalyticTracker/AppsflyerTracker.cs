namespace ServiceImplementation.AppsflyerAnalyticTracker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AppsFlyerSDK;
    using Core.AnalyticServices;
    using Core.AnalyticServices.Data;
    using GameFoundation.Scripts.Utilities.Extension;
    using UnityEngine;
    using Zenject;

    public class AppsflyerTracker : BaseTracker
    {
        protected override TaskCompletionSource<bool>      TrackerReady         { get; } = new();
        protected override Dictionary<Type, EventDelegate> CustomEventDelegates { get; }

        public AppsflyerTracker(SignalBus signalBus, IAnalyticServices analyticServices) : base(signalBus, analyticServices) { }


        protected override Task TrackerSetup()
        {
            if (this.TrackerReady.Task.Status == TaskStatus.RanToCompletion) return Task.CompletedTask;
            
            
            Debug.Log($"setting up appsflyer tracker");

            var apiId = "appId";
            var devKey = "devKey";
            
            if (string.IsNullOrEmpty(apiId)) {
                Debug.LogError("Appsflyer can't be initialized, Appsflyer ApiKey not found");
                this.TrackerReady.SetResult(false);
            }
            else {
                AppsFlyer.initSDK(devKey, apiId);
                AppsFlyer.startSDK();

                this.TrackerReady.SetResult(true);
            }
            
            return this.TrackerReady.Task;
        }
        protected override void SetUserId(string userId)
        {
            AppsFlyer.setCustomerUserId(userId);
        }

        protected override void OnChangedProps(Dictionary<string, object> changedProps)
        {
            var convertedData = changedProps.ToDictionary(pair => pair.Key, pair => pair.Value.ToJson());
            AppsFlyer.setAdditionalData(convertedData);
        }
        protected override void OnEvent(string name, Dictionary<string, object> data)
        {
            var convertedData = data.ToDictionary(pair => pair.Key, pair => pair.Value.ToJson());
            AppsFlyer.sendEvent(name, convertedData);
        }
        
        //todo track IAP event
    }
}