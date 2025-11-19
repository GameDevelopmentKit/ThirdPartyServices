#if ADJUST
namespace ServiceImplementation.AdjustAnalyticTracker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AdjustSdk;
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using Core.AnalyticServices.Data;
    using Core.AnalyticServices.Signal;
    using GameFoundation.Scripts.Utilities.LogService;
    using UnityEngine.Scripting;
    using Zenject;

    public class AdjustTracker : BaseTracker
    {
        private readonly ISignalBus                        signalBus;
        private readonly AnalyticConfig                    analyticConfig;
        private readonly ILogService                       logger;
        private readonly AnalyticsEventCustomizationConfig analyticsEventCustomizationConfig;
        private          AdjustMono                        adjustMono;

        [Preserve]
        public AdjustTracker(
            ISignalBus signalBus,
            AnalyticConfig analyticConfig,
            ILogService logger,
            AnalyticsEventCustomizationConfig analyticsEventCustomizationConfig
        ) : base(signalBus, analyticConfig)
        {
            this.signalBus                         = signalBus;
            this.analyticConfig                    = analyticConfig;
            this.logger                            = logger;
            this.analyticsEventCustomizationConfig = analyticsEventCustomizationConfig;

            if (analyticsEventCustomizationConfig.CustomEventKeys.Count == 0)
            {
                foreach (var item in analyticConfig.AdjustMappingEvents)
                {
                    analyticsEventCustomizationConfig.CustomEventKeys.Add(item.EventName, item.EventToken);
                }
            }

            this.eventTokens = this.analyticsEventCustomizationConfig.CustomEventKeys.Values.ToHashSet();
            this.adjustMono  = AdjustMono.Create();
        }

        protected override HashSet<Type>              IgnoreEvents    => this.analyticsEventCustomizationConfig.IgnoreEvents;
        protected override HashSet<string>            IncludeEvents   => this.analyticsEventCustomizationConfig.IncludeEvents;
        protected override Dictionary<string, string> CustomEventKeys => this.analyticsEventCustomizationConfig.CustomEventKeys;
        protected override TaskCompletionSource<bool> TrackerReady    { get; } = new();

        private readonly HashSet<string> eventTokens;

        // flow by source: https://dev.adjust.com/en/sdk/unity/integrations/admob
        private readonly Dictionary<string, string> adRevenueSourceMapping = new()
        {
            { AdRevenueConstants.ARSourceAppLovinMAX, "applovin_max_sdk" },
            { AdRevenueConstants.ARSourceMopub, "mopub" },
            { AdRevenueConstants.ARSourceAdMob, "admob_sdk" },
            { AdRevenueConstants.ARSourceYandex, "yandex_sdk" },
            { AdRevenueConstants.ARSourceIronSource, "ironsource_sdk" },
            { AdRevenueConstants.ARSourceAdmost, "admost_sdk" },
            { AdRevenueConstants.ARSourceUnity, "unity_sdk" },
            { AdRevenueConstants.ARSourceHeliumChartboost, "helium_chartboost_sdk" },
            { AdRevenueConstants.ARSourcePublisher, "publisher_sdk" },
            { AdRevenueConstants.ARSourceImmersiveAds, "immersive_ads_sdk" },
            { AdRevenueConstants.ARSourceGadsmeAds, "gadsme_ads" },
        };

        protected override Dictionary<Type, EventDelegate> CustomEventDelegates => new()
        {
            { typeof(IapTransactionDidSucceed), this.TrackIAP },
            { typeof(AdsRevenueEvent), this.TrackAdsRevenue }
        };

        protected override void OnChangedProps(Dictionary<string, object> changedProps) { }

        protected override void OnEvent(string eventToken, Dictionary<string, object> data)
        {
            // Dont fire event that haven't defined token yet
            if (!this.eventTokens.Contains(eventToken)) return;

            var adjustEvent = new AdjustEvent(eventToken);

            var eventDataString = "";

            if (data != null)
            {
                foreach (var (key, value) in data)
                {
                    if (key == null || value == null) continue;
                    adjustEvent.AddCallbackParameter(key, value.ToString());
                }

                eventDataString = string.Join(", ", data.Select(x => $"{x.Key}: {x.Value}"));
            }

            this.logger.Log($"{eventToken} with data: {eventDataString}");

            Adjust.TrackEvent(adjustEvent);
        }

        protected override Task TrackerSetup()
        {
            if (this.TrackerReady.Task.Status == TaskStatus.RanToCompletion) return Task.CompletedTask;

            this.logger.Log("setting up adjust tracker");

            var appToken = this.analyticConfig.AdjustAppToken;

#if MMP_DEBUG && !PRODUCTION
            var environment = AdjustEnvironment.Sandbox;
#else
            var environment = AdjustEnvironment.Production;
#endif

#if UNITY_IOS || UNITY_STANDALONE_OSX
            if (string.IsNullOrEmpty(appToken))
            {
                this.logger.Error("Adjust can't be initialized, Adjust AppToken not found");
                this.TrackerReady.SetResult(false);
                return this.TrackerReady.Task;
            }
#endif

            var adjustConfig = new AdjustConfig(appToken, environment);
            adjustConfig.AttConsentWaitingInterval        = 120;
            adjustConfig.IsCostDataInAttributionEnabled   = true;
            adjustConfig.IsSendingInBackgroundEnabled     = true;
            adjustConfig.IsDeferredDeeplinkOpeningEnabled = true;
            adjustConfig.AttributionChangedDelegate       = this.OnAttributionChanged;
            adjustConfig.DeferredDeeplinkDelegate         = this.adjustMono.OnDeepLinking;
#if MMP_DEBUG && !PRODUCTION
            adjustConfig.LogLevel = AdjustLogLevel.Verbose;
#endif
            Adjust.InitSdk(adjustConfig);
            this.TrackerReady.SetResult(true);

            return this.TrackerReady.Task;
        }

        // Handle attribution callback
        private void OnAttributionChanged(AdjustAttribution attributionData)
        {
            if (attributionData != null)
            {
                this.logger.Log("Attribution Data Received:");
                // Log key attribution data
                this.logger.Log($"Network: {attributionData.Network}");
                this.logger.Log($"Campaign: {attributionData.Campaign}");
                this.logger.Log($"Ad Group: {attributionData.Adgroup}");
                this.logger.Log($"Creative: {attributionData.Creative}");
                this.logger.Log($"Click Label: {attributionData.ClickLabel}");
                this.logger.Log($"Tracker Token: {attributionData.TrackerToken}");
                this.logger.Log($"Tracker Name: {attributionData.TrackerName}");

                // Log all key-value pairs to a dictionary
                var dataDictionary = new Dictionary<string, object>
                {
                    { "Network", attributionData.Network },
                    { "Campaign", attributionData.Campaign },
                    { "AdGroup", attributionData.Adgroup },
                    { "Creative", attributionData.Creative },
                    { "ClickLabel", attributionData.ClickLabel },
                    { "TrackerToken", attributionData.TrackerToken },
                    { "TrackerName", attributionData.TrackerName }
                };

                this.signalBus.Fire(new AttributionChangedSignal(dataDictionary));
            }
            else
            {
                this.logger.Warning("Attribution data is null.");
            }
        }

        protected override void SetUserId(string userId) { }

        private void TrackIAP(IEvent trackedevent, Dictionary<string, object> data)
        {
            if (trackedevent is not IapTransactionDidSucceed iapTransaction)
            {
                this.logger.Error("trackedEvent in TrackIAP is not of correct type");

                return;
            }

            var adjustEvent = new AdjustEvent(this.analyticConfig.AdjustPurchaseToken);
            adjustEvent.TransactionId = iapTransaction.TransactionId;
            adjustEvent.SetRevenue(iapTransaction.Revenue, iapTransaction.CurrencyCode);
            Adjust.TrackEvent(adjustEvent);
        }

        private void TrackAdsRevenue(IEvent trackedEvent, Dictionary<string, object> data)
        {
            if (trackedEvent is not AdsRevenueEvent adsRevenueEvent)
            {
                this.logger.Error("trackedEvent in AdsRevenue is not of correct type");

                return;
            }

            var adjustRevenue = new AdjustAdRevenue(this.adRevenueSourceMapping[adsRevenueEvent.AdsRevenueSourceId]);
            adjustRevenue.SetRevenue(adsRevenueEvent.Revenue, adsRevenueEvent.Currency);
            adjustRevenue.AdRevenueNetwork   = adsRevenueEvent.AdNetwork;
            adjustRevenue.AdRevenueUnit      = adsRevenueEvent.AdUnit;
            adjustRevenue.AdRevenuePlacement = adsRevenueEvent.Placement;
            Adjust.TrackAdRevenue(adjustRevenue);

            var item = this.analyticConfig.AdjustMappingEvents.FirstOrDefault(x => x.EventName.Equals(nameof(AdsRevenueEvent)));

            if (item != null)
            {
                this.OnEvent(item.EventToken, data);
            }

            this.logger.Log(
                $"OnEvent Ad Revenue : {adsRevenueEvent.AdUnit} - {adsRevenueEvent.AdFormat} - {adsRevenueEvent.AdNetwork} - {adsRevenueEvent.Placement} - {adsRevenueEvent.Currency} - {adsRevenueEvent.Revenue}");
        }
    }
}
#endif