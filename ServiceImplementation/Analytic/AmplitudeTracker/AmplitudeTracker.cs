#if AMPLITUDE

namespace ServiceImplementation.AmplitudeTracker
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using Core.AnalyticServices.Data;
    using GameFoundation.Scripts.Utilities.LogService;
    using UnityEngine;
    using Zenject;

    /// <summary>
    /// 
    /// </summary>
    public sealed class AmplitudeTracker : BaseTracker
    {
        private readonly   ILogService                       logger;
        private readonly   AnalyticsEventCustomizationConfig customizationConfig;
        protected override TaskCompletionSource<bool>        TrackerReady { get; } = new();

        protected override Dictionary<Type, EventDelegate> CustomEventDelegates => new Dictionary<Type, EventDelegate>
        {
            { typeof(IapTransactionDidSucceed), TrackIAP }
        };


        public AmplitudeTracker(ILogService logger, SignalBus signalBus, AnalyticConfig analyticConfig, AnalyticsEventCustomizationConfig customizationConfig) : base(signalBus, analyticConfig)
        {
            this.logger              = logger;
            this.customizationConfig = customizationConfig;
        }

        protected override Task TrackerSetup()
        {
            if (this.TrackerReady.Task.Status == TaskStatus.RanToCompletion) return Task.CompletedTask;

            Debug.Log($"setting up amplitude tracker");
            // SerializeEcon = false;

            var apiKey = this.analyticConfig.AmplitudeApiKey;

            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.LogError("Amplitude can't be initialized, AmplitudeApiKey not found");
                this.TrackerReady.SetResult(false);
                return this.TrackerReady.Task;
            }
            else
            {
                Amplitude amplitude = Amplitude.getInstance();
                amplitude.logging = this.analyticConfig.AmplitudeLogging;
                amplitude.trackSessionEvents(this.analyticConfig.AmplitudeTrackSessionEvents);
                amplitude.init(apiKey);
                
                this.TrackerReady.SetResult(true);
                return this.TrackerReady.Task;
            }
        }

        protected override void SetUserId(string userId) { Amplitude.Instance.setUserId(userId); }
        
        protected override void OnChangedProps(Dictionary<string, object> changedProps)
            => Amplitude.Instance.setUserProperties(changedProps);

        protected override void OnEvent(string name, Dictionary<string, object> data)
        {
            Debug.Log($"event {name} will send to amplitude");
            Amplitude.Instance.logEvent(name, data);
        }

        private void TrackIAP(IEvent trackedEvent, Dictionary<string, object> data)
        {
            if (!(trackedEvent is IapTransactionDidSucceed iapTransaction))
            {
                Debug.LogError("trackedEvent in TrackIAP is not of correct type");
                return;
            }

            var sku     = iapTransaction.PriceSku;
            var rev     = iapTransaction.Price;
            var iso     = iapTransaction.CurrencyCode;
            var receipt = iapTransaction.Receipt;
            var tid     = iapTransaction.TransactionId;

            void OnUsdRev(double usdRev)
            {
#if UNITY_ANDROID && STORE_GOOGLEPLAY
                try {
                    var payload = JsonUtility.FromJson<AndroidReceiptPayload>(receipt);
                    Amplitude.Instance.logRevenue(sku, 1, usdRev, payload.json, payload.signature, iso, null);
                }
                catch {
                    Amplitude.Instance.logRevenue(sku, 1, usdRev, receipt, tid, iso, null);
                }
#else
                Amplitude.Instance.logRevenue(sku, 1, usdRev, receipt, tid, iso, null);
#endif

                Amplitude.Instance.addUserProperty("total_spend", usdRev);
                Amplitude.Instance.addUserProperty("total_purchases", 1);
            }

            OnUsdRev(rev);
        }

        private void HandleCOPPA(bool age13OrAbove)
        {
            if (age13OrAbove)
                Amplitude.Instance.disableCoppaControl();
            else
                Amplitude.Instance.enableCoppaControl();
        }
    }
}
#endif