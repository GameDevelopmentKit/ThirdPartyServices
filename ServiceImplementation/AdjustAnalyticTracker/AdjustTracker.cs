#if ADJUST
namespace ServiceImplementation.AdjustAnalyticTracker
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using com.adjust.sdk;
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using Core.AnalyticServices.Data;
    using UnityEngine;
    using Zenject;

    public class AdjustTracker : BaseTracker
    {
        public AdjustTracker(SignalBus signalBus, AnalyticConfig analyticConfig) : base(signalBus, analyticConfig) { }

        protected override TaskCompletionSource<bool> TrackerReady { get; } = new();

        protected override Dictionary<Type, EventDelegate> CustomEventDelegates => new()
        {
            { typeof(IapTransactionDidSucceed), this.TrackIAP },
            { typeof(AdsRevenueEvent), this.TrackAdsRevenue }
        };

        protected override void OnChangedProps(Dictionary<string, object> changedProps) { }

        protected override void OnEvent(string name, Dictionary<string, object> data)
        {
            var adjustEvent = new AdjustEvent(name);
            foreach (var (key, value) in data)
            {
                adjustEvent.addCallbackParameter(key, value.ToString());
            }

            Adjust.trackEvent(adjustEvent);
        }

        protected override Task TrackerSetup()
        {
            if (this.TrackerReady.Task.Status == TaskStatus.RanToCompletion) return Task.CompletedTask;

            Debug.Log("setting up adjust tracker");

            var appToken = this.analyticConfig.AdjustAppToken;
            var environment = this.analyticConfig.AdjustIsDebug ? AdjustEnvironment.Sandbox : AdjustEnvironment.Production;

#if UNITY_IOS || UNITY_STANDALONE_OSX
            if (string.IsNullOrEmpty(appToken))
            {
                Debug.LogError("Adjust can't be initialized, Adjust AppToken not found");
                this.TrackerReady.SetResult(false);
                return this.TrackerReady.Task;
            }
#endif

            var adjustConfig = new AdjustConfig(appToken, environment);
            adjustConfig.setSendInBackground(true);
            Adjust.start(adjustConfig);
            this.TrackerReady.SetResult(true);
            return this.TrackerReady.Task;
        }

        protected override void SetUserId(string userId) { }

        private void TrackIAP(IEvent trackedevent, Dictionary<string, object> data)
        {
            if (trackedevent is not IapTransactionDidSucceed iapTransaction)
            {
                Debug.LogError("trackedEvent in TrackIAP is not of correct type");
                return;
            }

            var adjustEvent = new AdjustEvent(this.analyticConfig.AdjustPurchaseToken);
            adjustEvent.setTransactionId(iapTransaction.TransactionId);
            adjustEvent.setRevenue(iapTransaction.Price, iapTransaction.CurrencyCode);
            Adjust.trackEvent(adjustEvent);
        }

        private void TrackAdsRevenue(IEvent trackedEvent, Dictionary<string, object> data)
        {
            if (trackedEvent is not AdsRevenueEvent adsRevenueEvent)
            {
                Debug.LogError("trackedEvent in AdsRevenue is not of correct type");
                return;
            }

            var adjustRevenue = new AdjustAdRevenue(adsRevenueEvent.AdsRevenueSourceId);
            adjustRevenue.setRevenue(adsRevenueEvent.Revenue, adsRevenueEvent.Currency);
            adjustRevenue.setAdRevenueNetwork(adsRevenueEvent.AdNetwork);
            adjustRevenue.setAdRevenueUnit(adsRevenueEvent.AdUnit);
            adjustRevenue.setAdRevenuePlacement(adsRevenueEvent.Placement);
            Adjust.trackAdRevenue(adjustRevenue);
        }
    }
}
#endif