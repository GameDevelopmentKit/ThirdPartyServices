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
    using GameFoundation.Scripts.Utilities.LogService;
    using Newtonsoft.Json;
    using UnityEngine;
    using Zenject;

    public class AppsflyerTracker : BaseTracker
    {
        private readonly          ILogService                       logger;
        private readonly          AnalyticsEventCustomizationConfig customizationConfig;
        protected override        TaskCompletionSource<bool>        TrackerReady         { get; } = new();
        protected sealed override Dictionary<Type, EventDelegate>   CustomEventDelegates { get; }
        protected override        HashSet<Type>                     IgnoreEvents         => this.customizationConfig.IgnoreEvents;
        protected override        HashSet<string>                   IncludeEvents        => this.customizationConfig.IncludeEvents;
        protected override        Dictionary<string, string>        CustomEventKeys      => this.customizationConfig.CustomEventKeys;


        public AppsflyerTracker(ILogService logger, SignalBus signalBus, AnalyticConfig analyticConfig,
            AnalyticsEventCustomizationConfig customizationConfig,
            AppsflyerPurchaseConnectorReceiptValidator receiptValidator) : base(signalBus, analyticConfig)
        {
            this.logger = logger;
            _ = receiptValidator; // Resolving this dependency subscribes the ROI360 callback bridge before SDK setup.
            CustomEventDelegates = new Dictionary<Type, EventDelegate>
            {
                { typeof(AdsRevenueEvent), this.TrackAdsRevenue }
            };
            
            this.customizationConfig = customizationConfig;

            if (customizationConfig.CustomEventKeys.Count == 0)
            {
                this.logger.Error($"CustomEventKeys is empty, please Init in your ProjectInstaller");
            }
        }

        protected override Task TrackerSetup()
        {
            Debug.Log($"[AppsflyerTracker] TrackerSetup entered, TrackerReady status: {this.TrackerReady.Task.Status}");
            if (this.TrackerReady.Task.Status == TaskStatus.RanToCompletion) return Task.CompletedTask;

            if (this.analyticConfig.debugMode) Debug.Log($"Setting up appsflyer tracker");
            
            if (!analyticConfig.AppsflyerIsEnableRoi360)
            {
                CustomEventDelegates.Add( typeof(IapTransactionDidSucceed), this.TrackIAP );
            }

            var apiId  = this.analyticConfig.AppsflyerAppId;
            var devKey = this.analyticConfig.AppsflyerDevKey;

            if (string.IsNullOrEmpty(devKey))
            {
                throw new Exception("Appsflyer can't be initialized, Appsflyer DevKey not found");
            }

#if UNITY_IOS || UNITY_STANDALONE_OSX
            if (string.IsNullOrEmpty(apiId))
            {
                Debug.LogError("Appsflyer can't be initialized, Appsflyer ApiKey not found");
                this.TrackerReady.SetResult(false);
                return this.TrackerReady.Task;
            }
#endif
            AppsFlyer.initSDK(devKey, apiId);
#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyer.waitForATTUserAuthorizationWithTimeoutInterval(60);
#endif
            AppsFlyer.setIsDebug(this.analyticConfig.AppsflyerIsDebug);

            this.ConfigurePurchaseConnector();

            //Start SDK
            AppsFlyer.startSDK();

            this.TrackerReady.SetResult(true);

            return this.TrackerReady.Task;
        }

        protected override void SetUserId(string userId)
        {
            if (this.analyticConfig.debugMode) Debug.Log($"Appsflyer: SetUserId - {userId}");
            AppsFlyer.setCustomerUserId(userId);
        }

        protected override void OnChangedProps(Dictionary<string, object> changedProps)
        {
            var convertedData = changedProps.ToDictionary(pair => pair.Key, pair => pair.Value?.ToString());
            if (this.analyticConfig.debugMode) Debug.Log($"Appsflyer: OnChangedProps - {JsonConvert.SerializeObject(convertedData)}");
            AppsFlyer.setAdditionalData(convertedData);
        }

        protected override void OnEvent(string name, Dictionary<string, object> data)
        {
            var convertedData = data == null ? new Dictionary<string, string>() : data.ToDictionary(pair => pair.Key, pair => pair.Value?.ToString());

            if (this.analyticConfig.debugMode)
            {
                Debug.Log(data == null
                              ? $"Appsflyer: OnEvent - {name}"
                              : $"Appsflyer: OnEvent - {name} - {JsonConvert.SerializeObject(convertedData)}");
            }

            AppsFlyer.sendEvent(name, convertedData);
        }

        //we don't need it anymore because we use AppsFlyer Purchase Connector instead
        //new update: appsflyer connector still not work, we need to use this method to track IAP
        private void TrackIAP(IEvent trackedEvent, Dictionary<string, object> data)
        {
            if (trackedEvent is not IapTransactionDidSucceed iapTransaction)
            {
                Debug.LogError("trackedEvent in TrackIAP is not of correct type");

                return;
            }

            var eventValues = new Dictionary<string, string>
            {
                { AFInAppEvents.CONTENT_ID, iapTransaction.OfferSku },
                { AFInAppEvents.CURRENCY, iapTransaction.CurrencyCode },
                { AFInAppEvents.PRICE, iapTransaction.Price.ToString(CultureInfo.InvariantCulture) },
                { AFInAppEvents.QUANTITY, iapTransaction.Quantity.ToString() },
                { AFInAppEvents.REVENUE, iapTransaction.Revenue.ToString(CultureInfo.InvariantCulture) },
            };

            if (this.analyticConfig.debugMode) Debug.Log($"Appsflyer: TrackIAP - {AFInAppEvents.PURCHASE} - {JsonConvert.SerializeObject(eventValues)}");

            AppsFlyer.sendEvent(AFInAppEvents.PURCHASE, eventValues);
        }

        private void TrackAdsRevenue(IEvent trackedEvent, Dictionary<string, object> data)
        {
            if (trackedEvent is not AdsRevenueEvent adsRevenueEvent)
            {
                Debug.LogError("trackedEvent in AdsRevenue is not of correct type");

                return;
            }

            MediationNetwork mediationNetworkType = adsRevenueEvent.AdsRevenueSourceId switch
            {
                AdRevenueConstants.ARSourceAppLovinMAX => MediationNetwork.ApplovinMax,
                AdRevenueConstants.ARSourceIronSource => MediationNetwork.IronSource,
                AdRevenueConstants.ARSourceAdMob => MediationNetwork.GoogleAdMob,
                AdRevenueConstants.ARSourceUnity => MediationNetwork.Unity,
                _ => MediationNetwork.Custom
            };

            Dictionary<string, string> additionalParams = new Dictionary<string, string>();
            additionalParams.Add(AdRevenueScheme.AD_UNIT, adsRevenueEvent.AdUnit);
            additionalParams.Add(AdRevenueScheme.AD_TYPE, adsRevenueEvent.AdFormat);
            additionalParams.Add(AdRevenueScheme.PLACEMENT, adsRevenueEvent.Placement);
            var logRevenue = new AFAdRevenueData(adsRevenueEvent.AdNetwork, mediationNetworkType, adsRevenueEvent.Currency, adsRevenueEvent.Revenue);

            if (this.analyticConfig.debugMode) Debug.Log($"Appsflyer: TrackAdsRevenue - network:{adsRevenueEvent.AdNetwork} mediation:{mediationNetworkType} currency:{adsRevenueEvent.Currency} revenue:{adsRevenueEvent.Revenue} - {JsonConvert.SerializeObject(additionalParams)}");

            AppsFlyer.logAdRevenue(logRevenue, additionalParams);
        }

        private void ConfigurePurchaseConnector()
        {
            if (!analyticConfig.AppsflyerIsEnableRoi360) return;

            //IAP Revenue connector
            AppsFlyerPurchaseConnector.init(AppsflyerMono.Create(), Store.GOOGLE);


            // Set sandbox mode for testing
            AppsFlyerPurchaseConnector.setIsSandbox(this.analyticConfig.AppsflyerIapSandbox);

            // Configure StoreKit version (iOS only) - SK1 is the default
            AppsFlyerPurchaseConnector.setStoreKitVersion(StoreKitVersion.SK2);

            // Enable automatic logging for subscriptions and in-app purchases
            AppsFlyerPurchaseConnector.setAutoLogPurchaseRevenue(
                AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsAutoRenewableSubscriptions,
                AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsInAppPurchases
            );

            AppsFlyerPurchaseConnector.setPurchaseRevenueValidationListeners(true);

            AppsFlyerPurchaseConnector.build();
            AppsFlyerPurchaseConnector.startObservingTransactions();
        }
    }
}
#endif
