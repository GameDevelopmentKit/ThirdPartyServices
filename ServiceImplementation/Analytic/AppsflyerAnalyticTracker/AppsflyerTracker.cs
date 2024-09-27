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
    using GameFoundation.Signals;
    using UnityEngine.Scripting;
    using Newtonsoft.Json;
    using TheOne.Logging;
#if THEONE_IAP
    using AppsFlyerConnector;
#endif

    public class AppsflyerTracker : BaseTracker
    {
        private readonly   AnalyticsEventCustomizationConfig customizationConfig;
        protected override TaskCompletionSource<bool>        TrackerReady { get; } = new();

        protected override Dictionary<Type, EventDelegate> CustomEventDelegates => new()
        {
            { typeof(IapTransactionDidSucceed), this.TrackIAP },
            { typeof(AdsRevenueEvent), this.TrackAdsRevenue }
        };

        [Preserve]
        public AppsflyerTracker(SignalBus                         signalBus,
                                AnalyticConfig                    analyticConfig,
                                ILoggerManager                    loggerManager,
                                AnalyticsEventCustomizationConfig customizationConfig)
            : base(signalBus, analyticConfig, loggerManager)
        {
            this.customizationConfig = customizationConfig;

            if (customizationConfig.CustomEventKeys.Count == 0)
            {
                this.Logger.Error("Appsflyer: CustomEventKeys is empty, please Init in your ProjectInstaller");
            }
        }

        protected override HashSet<Type>              IgnoreEvents    => this.customizationConfig.IgnoreEvents;
        protected override HashSet<string>            IncludeEvents   => this.customizationConfig.IncludeEvents;
        protected override Dictionary<string, string> CustomEventKeys => this.customizationConfig.CustomEventKeys;

        protected override Task TrackerSetup()
        {
            if (this.TrackerReady.Task.Status == TaskStatus.RanToCompletion) return Task.CompletedTask;

            this.Logger.Info("Appsflyer: Setting up appsflyer tracker");

            var apiId = this.analyticConfig.AppsflyerAppId;
            var devKey = this.analyticConfig.AppsflyerDevKey;

            if (string.IsNullOrEmpty(apiId))
            {
                throw new Exception("Appsflyer analytic tracker: Appsflyer can't be initialized, Appsflyer AppId not found");
            }

            if (string.IsNullOrEmpty(devKey))
            {
                throw new Exception("Appsflyer analytic tracker: Appsflyer can't be initialized, Appsflyer DevKey not found");
            }

#if UNITY_IOS || UNITY_STANDALONE_OSX
            if (string.IsNullOrEmpty(apiId))
            {
                this.Logger.Error("Appsflyer: Appsflyer can't be initialized, Appsflyer ApiKey not found");
                this.TrackerReady.SetResult(false);
                return this.TrackerReady.Task;
            }
#endif
            AppsFlyer.initSDK(devKey, apiId);
#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyer.waitForATTUserAuthorizationWithTimeoutInterval(60);
#endif
#if THEONE_MMP_DEBUG && !PRODUCTION
            AppsFlyer.setIsDebug(true);
#endif

            //IAP Revenue connector
#if THEONE_IAP
            AppsFlyerPurchaseConnector.init(AppsflyerMono.Create(), Store.GOOGLE);
#if THEONE_MMP_DEBUG && !PRODUCTION
            AppsFlyerPurchaseConnector.setIsSandbox(true);
#endif
            AppsFlyerPurchaseConnector.setAutoLogPurchaseRevenue(AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsAutoRenewableSubscriptions, AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsInAppPurchases);
            AppsFlyerPurchaseConnector.build();
            AppsFlyerPurchaseConnector.startObservingTransactions();
#endif

            //Start SDK
            AppsFlyer.startSDK();

            this.TrackerReady.SetResult(true);

            return this.TrackerReady.Task;
        }

        protected override void SetUserId(string userId)
        {
            AppsFlyer.setCustomerUserId(userId);
        }

        protected override void OnChangedProps(Dictionary<string, object> changedProps)
        {
            var convertedData = changedProps.ToDictionary(pair => pair.Key, pair => pair.Value?.ToString());
            AppsFlyer.setAdditionalData(convertedData);
        }

        protected override void OnEvent(string name, Dictionary<string, object> data)
        {
            this.Logger.Info($"Appsflyer: Track Event - {name} - {JsonConvert.SerializeObject(data)}");
            var convertedData = data == null ? new Dictionary<string, string>() : data.ToDictionary(pair => pair.Key, pair => pair.Value?.ToString());
            AppsFlyer.sendEvent(name, convertedData);
        }

        //we don't need it anymore because we use AppsFlyer Purchase Connector instead
        //new update: appsflyer connector still not work, we need to use this method to track IAP
        private void TrackIAP(IEvent trackedEvent, Dictionary<string, object> data)
        {
            if (trackedEvent is not IapTransactionDidSucceed iapTransaction)
            {
                this.Logger.Error("Appsflyer: TrackedEvent in TrackIAP is not of correct type");

                return;
            }

            var eventValues = new Dictionary<string, string>
            {
                { AFInAppEvents.CURRENCY, iapTransaction.CurrencyCode },
                { AFInAppEvents.PRICE, iapTransaction.Price.ToString(CultureInfo.InvariantCulture) },
                { AFInAppEvents.PURCHASE, iapTransaction.Price.ToString(CultureInfo.InvariantCulture) },
                { AFInAppEvents.REVENUE, iapTransaction.Price.ToString(CultureInfo.InvariantCulture) },
                { AFInAppEvents.CONTENT_ID, iapTransaction.OfferSku }
            };

            AppsFlyer.sendEvent(AFInAppEvents.PURCHASE, eventValues);
        }

        private void TrackAdsRevenue(IEvent trackedEvent, Dictionary<string, object> data)
        {
            if (trackedEvent is not AdsRevenueEvent adsRevenueEvent)
            {
                this.Logger.Error("Appsflyer: TrackedEvent in AdsRevenue is not of correct type");
                return;
            }

            var parameters = new Dictionary<string, string>
            {
                { AdRevenueScheme.AD_UNIT, adsRevenueEvent.AdUnit },
                { AdRevenueScheme.AD_TYPE, adsRevenueEvent.AdFormat },
                { AdRevenueScheme.PLACEMENT, adsRevenueEvent.Placement },
                { "af_quantity", "1" }
            };

            var mediationNetworkType = adsRevenueEvent.AdsRevenueSourceId switch
            {
                AdRevenueConstants.ARSourceAppLovinMAX => MediationNetwork.ApplovinMax,
                AdRevenueConstants.ARSourceIronSource  => MediationNetwork.IronSource,
                AdRevenueConstants.ARSourceAdMob       => MediationNetwork.GoogleAdMob,
                AdRevenueConstants.ARSourceUnity       => MediationNetwork.Unity,
                AdRevenueConstants.ARSourceYandex      => MediationNetwork.Yandex,
                _                                      => MediationNetwork.Custom
            };

            var logRevenue = new AFAdRevenueData(adsRevenueEvent.AdNetwork, mediationNetworkType, adsRevenueEvent.Currency, adsRevenueEvent.Revenue);
            AppsFlyer.logAdRevenue(logRevenue, parameters);
        }
    }
}
#endif