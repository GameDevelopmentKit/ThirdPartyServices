#if GADSME
namespace ServiceImplementation.AdsServices.Gads
{
    using System;
    using Core.AdsServices.Native;
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using Core.AnalyticServices.Signal;
    using Gadsme;
    using GameFoundation.DI;
    using GameFoundation.Signals;
    using ServiceImplementation.Configs;
    using ServiceImplementation.Configs.Ads;
    using TheOne.Logging;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.Scripting;
    using ILogger = TheOne.Logging.ILogger;

    public class GadsmeWrapper : IInitializable, INativeAdsService
    {
        public string AdPlatform => AdRevenueConstants.ARSourceGadsmeAds;

        #region Inject

        private readonly SignalBus          signalBus;
        private readonly IAnalyticServices  analyticServices;
        private readonly ILogger            logger;
        private readonly ThirdPartiesConfig thirdPartiesConfig;
        private readonly AdServicesConfig   adServicesConfig;

        [Preserve]
        public GadsmeWrapper(
            SignalBus          signalBus,
            IAnalyticServices  analyticServices,
            ILoggerManager     loggerManager,
            ThirdPartiesConfig thirdPartiesConfig,
            AdServicesConfig   adServicesConfig
        )
        {
            this.signalBus          = signalBus;
            this.analyticServices   = analyticServices;
            this.logger             = loggerManager.GetLogger(this);
            this.thirdPartiesConfig = thirdPartiesConfig;
            this.adServicesConfig   = adServicesConfig;
        }

        #endregion

        public void Initialize()
        {
            // Placement events
            GadsmeEvents.PlacementLoadedEvent   += this.OnPlacementLoaded;
            GadsmeEvents.PlacementVisibleEvent  += this.OnPlacementVisible;
            GadsmeEvents.PlacementViewableEvent += this.OnPlacementViewable;
            GadsmeEvents.PlacementClickedEvent  += this.OnPlacementClicked;
            GadsmeEvents.PlacementFailedEvent   += this.OnPlacementFailed;

            // Ad content events
            GadsmeEvents.AdContentLoadedEvent   += this.OnAdContentLoaded;
            GadsmeEvents.AdContentVisibleEvent  += this.OnAdContentVisible;
            GadsmeEvents.AdContentViewableEvent += this.OnAdContentViewable;
            GadsmeEvents.AdContentClickedEvent  += this.OnAdContentClicked;
            GadsmeEvents.AdContentFailedEvent   += this.OnAdContentFailed;

            // Audio ad events
            GadsmeEvents.LoadAudioAdEvent                  += this.OnLoadAudioAd;
            GadsmeEvents.CancelAudioAdEvent                += this.OnCancelAudioAd;
            GadsmeEvents.AudioAdReadyToPlayEvent           += this.OnAudioAdReadyToPlay;
            GadsmeEvents.PlayAudioAdEvent                  += this.OnPlayAudioAd;
            GadsmeEvents.AudioAdIncompletePlaythroughEvent += this.OnAudioAdIncompletePlaythrough;
            GadsmeEvents.FinishAudioAdEvent                += this.OnFinishAudioAd;

            GadsmeEvents.ImpressionEvent += OnImpression;

            GadsmeSDK.Init();
            this.logger.Info("Initialize SDK");

            var isForceSandBox = false;
            #if THEONE_ADS_DEBUG
            isForceSandBox = true;
            #endif

            GadsmePreferences.P16.forceSandbox = isForceSandBox;
        }

        private void OnImpression(GadsmeImpressionData obj)
        {
            var data = obj;
            this.logger.Info($"{data}");

            if (data is null) return;
            try
            {
                var adsRevenueEvent = new AdsRevenueEvent()
                {
                    AdsRevenueSourceId = AdRevenueConstants.ARSourceGadsmeAds,
                    Revenue            = data.netRevenue,
                    Currency           = data.currency,
                    Placement          = data.placementId,
                    AdNetwork          = "Gadsme",
                    AdFormat           = data.adFormat.GetName(),
                    AdUnit             = data.lineItemType
                };

                this.analyticServices.Track(adsRevenueEvent);
                this.signalBus.Fire(new AdRevenueSignal(adsRevenueEvent));
            }
            catch
            {
                this.logger.Error($"Failed to parse impression data: {data}");
            }
        }

        #region Events

        private void OnPlacementLoaded(GadsmePlacement placement)
        {
            this.logger.Info("Placement Loaded (" + placement.placementId + ")");
        }

        private void OnPlacementVisible(GadsmePlacement placement)
        {
            this.logger.Info("Placement Visible (" + placement.placementId + ")");
        }

        private void OnPlacementViewable(GadsmePlacement placement)
        {
            this.logger.Info("Placement Viewable (" + placement.placementId + ")");
        }

        private void OnPlacementClicked(GadsmePlacement placement)
        {
            this.logger.Info("Placement Clicked (" + placement.placementId + ")");
        }

        private void OnPlacementFailed(GadsmePlacement placement)
        {
            this.logger.Info("Placement Failed (" + placement.placementId + ")");
        }

        private void OnAdContentLoaded(GadsmeAdContentInfo info)
        {
            this.logger.Info("Ad Content Loaded (" + info.adFormat.GetName() + ", " + info.adChannelNumber + ", " + info.lineItemType + ")");
        }

        private void OnAdContentVisible(GadsmeAdContentInfo info)
        {
            this.logger.Info("Ad Content Visible (" + info.adFormat.GetName() + ", " + info.adChannelNumber + ", " + info.lineItemType + ")");
        }

        private void OnAdContentViewable(GadsmeAdContentInfo info)
        {
            this.logger.Info("Ad Content Viewable (" + info.adFormat.GetName() + ", " + info.adChannelNumber + ", " + info.lineItemType + ")");
        }

        private void OnAdContentClicked(GadsmeAdContentInfo info)
        {
            this.logger.Info("Ad Content Clicked (" + info.adFormat.GetName() + ", " + info.adChannelNumber + ", " + info.lineItemType + ")");
        }

        private void OnAdContentFailed(GadsmeAdContentInfo info)
        {
            this.logger.Info("Ad Content Failed (" + info.adFormat.GetName() + ", " + info.adChannelNumber + ", " + info.lineItemType + ")");
        }

        private void OnLoadAudioAd(GadsmeAudioAdInfo info)
        {
            this.logger.Info("Audio: load audio ad " + info);
        }

        private void OnCancelAudioAd(GadsmeAudioAdInfo info, GadsmeCancelAudioAdReason reason)
        {
            this.logger.Info("Audio: cancel audio ad " + info + " (reason: " + reason + ")");
        }

        private void OnAudioAdReadyToPlay(GadsmeAudioAdInfo info)
        {
            this.logger.Info("Audio: audio ad ready to play " + info);
        }

        private void OnPlayAudioAd(GadsmeAudioAdInfo info)
        {
            this.logger.Info("Audio: play audio ad " + info);
        }

        private void OnAudioAdIncompletePlaythrough(GadsmeAudioAdInfo info)
        {
            this.logger.Info("Audio: incomplete playthrough " + info);
        }

        private void OnFinishAudioAd(GadsmeAudioAdInfo info, bool complete)
        {
            this.logger.Info("Audio: finish audio ad " + info + " (complete: " + complete + ")");
        }

        #endregion

        /// <summary>
        /// Globally enable or disable the default placement interaction implementation.
        /// Default value is true.
        /// </summary>
        /// <param name="isInteractionsEnabled">Set to true to enable interactions, or false to disable them.</param>
        public static void SetInteractionsEnabled(bool isInteractionsEnabled)
        {
            GadsmeSDK.SetInteractionsEnabled(isInteractionsEnabled);
        }

        /// <summary>
        /// Handles placement interactions when the default interaction implementation is disabled.
        /// Expected to be called from a custom Update() method.
        /// </summary>
        public static void HandlePlacementInteractions()
        {
            GadsmeSDK.HandlePlacementInteractions();
        }

        /// <summary>
        /// Checks if the given RaycastResult should be handled by Gadsme.
        /// </summary>
        /// <param name="result">The RaycastResult to check.</param>
        /// <returns>True if Gadsme should handle this result, otherwise false.</returns>
        public static bool IsPlacementResult(RaycastResult result)
        {
            return GadsmeSDK.IsPlacementResult(result);
        }

        /// <summary>
        /// Explicitly refresh ads. This will only affect ads that don't auto-refresh automatically.
        /// </summary>
        public static void RefreshStaticAds()
        {
            GadsmeSDK.RefreshStaticAds();
        }

        /// <summary>
        /// Programmatically configure the maximum number of concurrent requests for loading ad content.
        /// Accepted values: 1 to 5 (inclusive).
        /// </summary>
        /// <param name="maxConcurrentRequests">Maximum number of concurrent requests.</param>
        public static void SetMaxConcurrentRequests(int maxConcurrentRequests)
        {
            GadsmeSDK.SetMaxConcurrentRequests(maxConcurrentRequests);
        }

        /// <summary>
        /// Programmatically configure the maximum number of active ad contents.
        /// Accepted values: 1 to 15 (inclusive).
        /// </summary>
        /// <param name="maxActiveAdContents">Maximum number of active ad contents.</param>
        public static void SetMaxActiveAdContents(int maxActiveAdContents)
        {
            GadsmeSDK.SetMaxActiveAdContents(maxActiveAdContents);
        }

        /// <summary>
        /// Set a custom EventSystem object for Gadsme SDK to handle clicks on placements.
        /// </summary>
        /// <param name="eventSystem">The custom EventSystem object.</param>
        public static void SetEventSystem(EventSystem eventSystem)
        {
            GadsmeSDK.SetEventSystem(eventSystem);
        }

        /// <summary>
        /// Preload an audio advertisement. Call StartAudioAd() to play it once it's ready.
        /// </summary>
        public static void PreloadAudioAd()
        {
            GadsmeSDK.PreloadAudioAd();
        }

        /// <summary>
        /// Start playing an audio advertisement.
        /// If an ad was preloaded, it will use the preloaded content. Otherwise, it loads and plays the ad.
        /// </summary>
        public static void StartAudioAd()
        {
            GadsmeSDK.StartAudioAd();
        }

        /// <summary>
        /// Preload a rewarded audio advertisement.
        /// </summary>
        public static void PreloadRewardedAudioAd()
        {
            GadsmeSDK.PreloadRewardedAudioAd();
        }

        /// <summary>
        /// Start playing a rewarded audio advertisement.
        /// If an ad was preloaded, it will use the preloaded content. Otherwise, it loads and plays the ad.
        /// </summary>
        /// <param name="onComplete">Callback invoked when the ad finishes playing.
        /// Boolean indicates whether the ad was completed successfully.</param>
        public static void StartRewardedAudioAd(System.Action<bool> onComplete)
        {
            GadsmeSDK.StartRewardedAudioAd(onComplete);
        }

        /// <summary>
        /// Stop any currently playing audio advertisement.
        /// Stopping an ad before it finishes may affect revenue and mark its playthrough as incomplete.
        /// </summary>
        public static void StopAudioAd()
        {
            GadsmeSDK.StopAudioAd();
        }

        /// <summary>
        /// Will select MainCamera, if not set, will pick the one with the highest depth
        /// </summary>
        public void ChangeMainCamera(Camera newCamera)
        {
            GadsmeSDK.SetMainCamera(newCamera);
        }
    }
}
#endif