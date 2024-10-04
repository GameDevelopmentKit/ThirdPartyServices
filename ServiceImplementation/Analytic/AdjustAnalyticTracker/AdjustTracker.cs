#if ADJUST
namespace ServiceImplementation.AdjustAnalyticTracker
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AdjustSdk;
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using Core.AnalyticServices.Data;
    using GameFoundation.Scripts.Utilities.LogService;
    using UnityEngine;
    using GameFoundation.Signals;
    using UnityEngine.Scripting;

    public class AdjustTracker : BaseTracker
    {
        private readonly ILogService                       logger;
        private readonly AnalyticsEventCustomizationConfig analyticsEventCustomizationConfig;

        [Preserve]
        public AdjustTracker(ILogService logger, SignalBus signalBus, AnalyticConfig analyticConfig, AnalyticsEventCustomizationConfig analyticsEventCustomizationConfig) : base(signalBus,
            analyticConfig)
        {
            this.analyticsEventCustomizationConfig = analyticsEventCustomizationConfig;
            this.logger                            = logger;
            if (analyticsEventCustomizationConfig.CustomEventKeys.Count == 0)
            {
                this.logger.Error($"CustomEventKeys is empty, please Init in your ProjectInstaller");
            }
        }

        protected override HashSet<Type>              IgnoreEvents    => this.analyticsEventCustomizationConfig.IgnoreEvents;
        protected override HashSet<string>            IncludeEvents   => this.analyticsEventCustomizationConfig.IncludeEvents;
        protected override Dictionary<string, string> CustomEventKeys => this.analyticsEventCustomizationConfig.CustomEventKeys;
        protected override TaskCompletionSource<bool> TrackerReady    { get; } = new();

        protected override Dictionary<Type, EventDelegate> CustomEventDelegates => new()
        {
            { typeof(IapTransactionDidSucceed), this.TrackIAP },
            { typeof(AdsRevenueEvent), this.TrackAdsRevenue }
        };

        protected override void OnChangedProps(Dictionary<string, object> changedProps) { }

        protected override void OnEvent(string name, Dictionary<string, object> data)
        {
            var adjustEvent = new AdjustEvent(name);

            if (data != null)
            {
                foreach (var (key, value) in data)
                {
                    if (key == null || value == null) continue;
                    adjustEvent.AddCallbackParameter(key, value.ToString());
                }
            }

            Adjust.TrackEvent(adjustEvent);
        }

        protected override Task TrackerSetup()
        {
            if (this.TrackerReady.Task.Status == TaskStatus.RanToCompletion) return Task.CompletedTask;

            Debug.Log("setting up adjust tracker");

            var appToken    = this.analyticConfig.AdjustAppToken;

#if THEONE_MMP_DEBUG && !PRODUCTION
            var environment = AdjustEnvironment.Sandbox;
#else
            var environment = AdjustEnvironment.Production;
#endif


#if UNITY_IOS || UNITY_STANDALONE_OSX
            if (string.IsNullOrEmpty(appToken))
            {
                Debug.LogError("Adjust can't be initialized, Adjust AppToken not found");
                this.TrackerReady.SetResult(false);
                return this.TrackerReady.Task;
            }
#endif

            var adjustConfig = new AdjustConfig(appToken, environment);
            adjustConfig.IsSendingInBackgroundEnabled = true;
            Adjust.InitSdk(adjustConfig);
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
            adjustEvent.TransactionId = iapTransaction.TransactionId;
            adjustEvent.SetRevenue(iapTransaction.Price, iapTransaction.CurrencyCode);
            Adjust.TrackEvent(adjustEvent);
        }

        private void TrackAdsRevenue(IEvent trackedEvent, Dictionary<string, object> data)
        {
            if (trackedEvent is not AdsRevenueEvent adsRevenueEvent)
            {
                Debug.LogError("trackedEvent in AdsRevenue is not of correct type");

                return;
            }

            var adjustRevenue = new AdjustAdRevenue(adsRevenueEvent.AdsRevenueSourceId);
            adjustRevenue.SetRevenue(adsRevenueEvent.Revenue, adsRevenueEvent.Currency);
            adjustRevenue.AdRevenueNetwork = adsRevenueEvent.AdNetwork;
            adjustRevenue.AdRevenueUnit = adsRevenueEvent.AdUnit;
            adjustRevenue.AdRevenuePlacement = adsRevenueEvent.Placement;
            Adjust.TrackAdRevenue(adjustRevenue);
        }
    }
}
#endif