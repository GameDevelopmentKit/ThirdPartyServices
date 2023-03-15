#if APPSFLYER

namespace ServiceImplementation.AppsflyerAnalyticTracker
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using AppsFlyerSDK;
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using Core.AnalyticServices.Data;
    using GameFoundation.Scripts.Utilities.Extension;
    using UnityEngine;
    using Zenject;

    public class AppsflyerTracker : BaseTracker
    {
        protected override TaskCompletionSource<bool> TrackerReady { get; } = new();

        protected override Dictionary<Type, EventDelegate> CustomEventDelegates => new()
        {
            { typeof(IapTransactionDidSucceed), TrackIAP },
            { typeof(AdsRevenueEvent), this.TrackAdsRevenue }
        };

        public AppsflyerTracker(SignalBus signalBus, AnalyticConfig analyticConfig) : base(signalBus, analyticConfig) { }


        protected override Task TrackerSetup()
        {
            if (this.TrackerReady.Task.Status == TaskStatus.RanToCompletion) return Task.CompletedTask;

            Debug.Log($"setting up appsflyer tracker");

            var apiId  = this.analyticConfig.AppsflyerAppId;
            var devKey = this.analyticConfig.AppsflyerDevKey;

#if UNITY_IOS || UNITY_STANDALONE_OSX
            if (string.IsNullOrEmpty(apiId))
            {
                Debug.LogError("Appsflyer can't be initialized, Appsflyer ApiKey not found");
                this.TrackerReady.SetResult(false);
                return this.TrackerReady.Task;
            }
#endif
            AppsFlyer.setIsDebug(this.analyticConfig.AppsflyerIsDebug);
            AppsFlyer.initSDK(devKey, apiId);
            AppsFlyer.startSDK();
            AppsFlyerAdRevenue.start();

            this.TrackerReady.SetResult(true);
            return this.TrackerReady.Task;
        }
        protected override void SetUserId(string userId) { AppsFlyer.setCustomerUserId(userId); }

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

        private void TrackIAP(IEvent trackedEvent, Dictionary<string, object> data)
        {
            if (trackedEvent is not IapTransactionDidSucceed iapTransaction)
            {
                Debug.LogError("trackedEvent in TrackIAP is not of correct type");
                return;
            }

            var eventValues = new Dictionary<string, string>
            {
                { AFInAppEvents.CURRENCY, iapTransaction.CurrencyCode },
                { AFInAppEvents.REVENUE, iapTransaction.Price.ToString(CultureInfo.InvariantCulture) },
                { AFInAppEvents.PRICE, iapTransaction.PriceSku }
            };
            AppsFlyer.sendEvent(AFInAppEvents.PURCHASE, eventValues);
        }


        private void TrackAdsRevenue(IEvent trackedEvent, Dictionary<string, object> data)
        {
            if (trackedEvent is not AdsRevenueEvent adsRevenueEvent)
            {
                Debug.LogError("trackedEvent in AdsRevenue is not of correct type");
                return;
            }

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add(AFAdRevenueEvent.AD_UNIT, adsRevenueEvent.AdUnit);
            dic.Add(AFAdRevenueEvent.PLACEMENT, adsRevenueEvent.Placement);
            AppsFlyerAdRevenueMediationNetworkType mediationNetworkType = adsRevenueEvent.AdsRevenueSourceId switch
            {
                AdRevenueConstants.ARSourceAppLovinMAX => AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeApplovinMax,
                AdRevenueConstants.ARSourceIronSource => AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeIronSource,
                AdRevenueConstants.ARSourceAdMob => AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeGoogleAdMob,
                AdRevenueConstants.ARSourceUnity => AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeUnity,
                _ => AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeCustomMediation
            };

            AppsFlyerAdRevenue.logAdRevenue(adsRevenueEvent.AdNetwork, mediationNetworkType, adsRevenueEvent.Revenue, adsRevenueEvent.Currency, dic);
        }
    }
}
#endif