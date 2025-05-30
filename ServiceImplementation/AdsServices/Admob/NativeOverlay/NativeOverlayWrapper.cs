namespace ServiceImplementation.AdsServices.Admob.NativeOverlay
{
    using System;
    using System.Threading;
    using Core.AdsServices.Signals;
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using Core.AnalyticServices.Signal;
    using GameFoundation.Scripts.Utilities.LogService;
    using GameFoundation.Signals;
    using ServiceImplementation.Configs;
    using ServiceImplementation.Configs.Ads;
    using UnityEngine;
    using UnityEngine.Scripting;
    #if ADMOB
    using Core.AdsServices;
    using Cysharp.Threading.Tasks;
    using GoogleMobileAds.Api;
    #endif

    public class NativeOverlayWrapper
    {
        #region Inject

        private readonly ILogService        logService;
        private readonly ThirdPartiesConfig thirdPartiesConfig;
        private readonly IAnalyticServices  analyticServices;
        private readonly SignalBus          signalBus;

        [Preserve]
        public NativeOverlayWrapper(
            ILogService        logService,
            ThirdPartiesConfig thirdPartiesConfig,
            IAnalyticServices  analyticServices,
            SignalBus          signalBus
        )
        {
            this.logService         = logService;
            this.thirdPartiesConfig = thirdPartiesConfig;
            this.analyticServices   = analyticServices;
            this.signalBus          = signalBus;
        }

        #endregion

        private AdMobSettings adMobSettings => this.thirdPartiesConfig.AdSettings.AdMob;
        private string        adUnitId      => this.adMobSettings.NativeOverlayAdIds.DefaultValue;

        #if ADMOB

        private NativeOverlayStyleConfig config => this.adMobSettings.NativeOverlayStyleConfig;
        private NativeOverlayAd          nativeOverlayAd;
        private bool                     isAdRendered;
        private CancellationTokenSource  showAdsCts;

        private const string AdPlatForm = AdRevenueConstants.ARSourceAdMob;

        public bool IsAdLoaded { get; private set; }
        public bool AdShowing  { get; private set; }

        /// <summary>
        /// Define our native ad-advanced options.
        /// </summary>
        private NativeAdOptions Option = new NativeAdOptions
        {
            AdChoicesPlacement = AdChoicesPlacement.TopRightCorner,
            MediaAspectRatio   = MediaAspectRatio.Square,
        };

        public void ShowAd(AdViewPosition adViewPosition)
        {
            if (this.nativeOverlayAd == null)
            {
                this.LoadAd();
            }
            this.logService.Log("oneLog: NativeOverlayWrapper, ShowAd");
            UniTask.WhenAll(UniTask.WaitUntil(() => this.IsAdLoaded))
                .AttachExternalCancellation((this.showAdsCts = new()).Token)
                .ContinueWith(() =>
                {
                    if (this.isAdRendered)
                    {
                        this.nativeOverlayAd.SetTemplatePosition(this.GetAdPosition(adViewPosition));
                    }
                    else
                    {
                        this.RenderAd(this.GetAdPosition(adViewPosition));
                    }
                    this.logService.Log("oneLog: NativeOverlayWrapper, Showing Native Overlay ad.");
                    this.nativeOverlayAd.Show();
                    this.AdShowing = true;
                }).Forget();
        }

        public void HideAd()
        {
            this.ResetShowAdsCts();
            if (this.nativeOverlayAd == null) return;
            if (!this.IsAdLoaded) return;
            if (!this.AdShowing) return;
            this.logService.Log("oneLog: NativeOverlayWrapper, Hiding Native Overlay ad.");
            this.nativeOverlayAd.Hide();
            this.AdShowing = false;
        }

        /// <summary>
        /// Loads the ad.
        /// </summary>
        private void LoadAd()
        {
            // Clean up the old ad before loading a new one.
            if (this.nativeOverlayAd != null)
            {
                this.DestroyAd();
            }

            this.logService.Log("oneLog: NativeOverlayWrapper, Loading native overlay ad");

            // Create our request used to load the ad.
            var adRequest = new AdRequest();

            // Send the request to load the ad.
            NativeOverlayAd.Load(this.adUnitId, adRequest, this.Option,
                (ad, error) =>
                {
                    // If the operation failed for a reason.
                    if (error != null)
                    {
                        this.logService.Error("oneLog: NativeOverlayWrapper, Native Overlay ad failed to load an ad with error : " + error);
                        return;
                    }
                    // If the operation failed for unknown reasons.
                    // This is an unexpected error, please report this bug if it happens.
                    if (ad == null)
                    {
                        this.logService.Error("oneLog: NativeOverlayWrapper, Unexpected error: Native Overlay ad load event fired with null ad and null error.");
                        return;
                    }

                    // The operation completed successfully.
                    this.logService.Log("oneLog: NativeOverlayWrapper, Native Overlay ad loaded with response : " + ad.GetResponseInfo());
                    this.nativeOverlayAd = ad;
                    this.SubscribeEvent();
                    this.IsAdLoaded = true;
                });
        }

        private void RenderAd(AdPosition adPosition)
        {
            if (this.nativeOverlayAd == null) return;
            this.logService.Log("oneLog: NativeOverlayWrapper, render config style");

            var style = new NativeTemplateStyle
            {
                TemplateId          = this.config.Template,
                MainBackgroundColor = this.config.MainBackgroundColor,
                CallToActionText = new()
                {
                    BackgroundColor = this.config.BackgroundColor,
                    TextColor       = this.config.TextColor,
                    FontSize        = this.config.FontSize,
                    Style           = this.config.Style,
                },
            };

            this.nativeOverlayAd.RenderTemplate(style, adPosition);
            this.isAdRendered = true;
        }

        /// <summary>
        /// Destroys the ad.
        /// When you are finished with the ad, make sure to call the Destroy()
        /// method before dropping your reference to it.
        /// </summary>
        public void DestroyAd()
        {
            this.ResetShowAdsCts();
            if (this.nativeOverlayAd == null) return;
            this.IsAdLoaded = false;
            this.AdShowing  = false;
            this.UnsubscribeEvent();
            this.logService.Log("oneLog: NativeOverlayWrapper, destroying Native Overlay ad.");
            this.nativeOverlayAd.Destroy();
            this.nativeOverlayAd = null;
            this.isAdRendered    = false;
        }

        private AdPosition GetAdPosition(AdViewPosition adViewPosition)
        {
            return adViewPosition switch
            {
                AdViewPosition.Top        => AdPosition.Top,
                AdViewPosition.Bottom     => AdPosition.Bottom,
                AdViewPosition.TopLeft    => AdPosition.TopLeft,
                AdViewPosition.TopRight   => AdPosition.TopRight,
                AdViewPosition.BottomLeft => AdPosition.BottomLeft,
                AdViewPosition.Center     => AdPosition.Center,
                _                         => throw new ArgumentOutOfRangeException(nameof(adViewPosition), adViewPosition, null)
            };
        }

        private void ResetShowAdsCts()
        {
            this.showAdsCts?.Cancel();
            this.showAdsCts?.Dispose();
            this.showAdsCts = null;
        }

        #region Events

        private void SubscribeEvent()
        {
            this.nativeOverlayAd.OnAdPaid                    += this.OnAdPaid;
            this.nativeOverlayAd.OnAdImpressionRecorded      += this.OnAdImpressionRecord;
            this.nativeOverlayAd.OnAdClicked                 += this.OnAdClicked;
            this.nativeOverlayAd.OnAdFullScreenContentOpened += this.OnAdFullScreenContentOpened;
            this.nativeOverlayAd.OnAdFullScreenContentClosed += this.OnAdFullScreenContentClosed;
        }

        private void UnsubscribeEvent()
        {
            this.nativeOverlayAd.OnAdPaid                    -= this.OnAdPaid;
            this.nativeOverlayAd.OnAdImpressionRecorded      -= this.OnAdImpressionRecord;
            this.nativeOverlayAd.OnAdClicked                 -= this.OnAdClicked;
            this.nativeOverlayAd.OnAdFullScreenContentOpened -= this.OnAdFullScreenContentOpened;
            this.nativeOverlayAd.OnAdFullScreenContentClosed -= this.OnAdFullScreenContentClosed;
        }

        // Raised when the ad is estimated to have earned money.
        private void OnAdPaid(AdValue args)
        {
            var adsRevenueEvent = new AdsRevenueEvent
            {
                AdsRevenueSourceId = AdPlatForm,
                AdUnit             = this.adUnitId,
                AdFormat           = AdFormatConstants.NativeOverlay,
                AdNetwork          = "AdMob",
                Revenue            = args.Value / 1e6,
                Currency           = "USD",
            };

            this.analyticServices.Track(adsRevenueEvent);
            this.signalBus.Fire(new AdRevenueSignal(adsRevenueEvent));
        }

        private void OnAdClicked()
        {
            this.logService.Log("oneLog: NativeOverlayWrapper, Clicked native overlay ad");
            var adRevenueEvent = new AdInfo(AdPlatForm, this.adUnitId, AdFormatConstants.NativeOverlay);
            this.signalBus.Fire(new NativeOverlayClickedSignal("", adRevenueEvent));
        }

        private void OnAdImpressionRecord()
        {
            this.logService.Log("oneLog: NativeOverlayWrapper, Recorded ad impression");
        }

        private void OnAdFullScreenContentOpened()
        {
            this.logService.Log("oneLog: NativeOverlayWrapper, Closed native overlay ad");
            var adRevenueEvent = new AdInfo(AdPlatForm, this.adUnitId, AdFormatConstants.NativeOverlay);
            this.signalBus.Fire(new AppOpenFullScreenContentOpenedSignal("", adRevenueEvent));
        }

        private void OnAdFullScreenContentClosed()
        {
            this.logService.Log("oneLog: NativeOverlayWrapper, Closed native overlay ad");
            var adRevenueEvent = new AdInfo(AdPlatForm, this.adUnitId, AdFormatConstants.NativeOverlay);
            this.signalBus.Fire(new AppOpenFullScreenContentClosedSignal("", adRevenueEvent));
        }

        #endregion
        #endif
    }
}