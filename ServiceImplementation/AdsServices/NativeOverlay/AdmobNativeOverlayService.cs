#if ADMOB
namespace ServiceImplementation.AdsServices.NativeOverlay
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Core.AdsServices;
    using Core.AdsServices.Signals;
    using Core.AnalyticServices;
    using Core.AnalyticServices.CommonEvents;
    using Core.AnalyticServices.Signal;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Signals;
    using GoogleMobileAds.Api;
    using ServiceImplementation.Configs;
    using ServiceImplementation.Configs.Ads;
    using TheOne.Logging;
    using UnityEngine.Scripting;

    public class AdmobNativeOverlayService : INativeOverlayService
    {
        private readonly ILogger            logger;
        private readonly SignalBus          signalBus;
        private readonly ThirdPartiesConfig thirdPartiesConfig;
        private readonly IAnalyticServices  analyticServices;

        [Preserve]
        public AdmobNativeOverlayService(
            ILoggerManager     loggerManager,
            SignalBus          signalBus,
            ThirdPartiesConfig thirdPartiesConfig,
            IAnalyticServices  analyticServices
        )
        {
            this.logger             = loggerManager.GetLogger(this);
            this.signalBus          = signalBus;
            this.thirdPartiesConfig = thirdPartiesConfig;
            this.analyticServices   = analyticServices;
        }

        private AdMobSettings            adMobSettings => this.thirdPartiesConfig.AdSettings.AdMob;
        private NativeOverlayStyleConfig config        => this.adMobSettings.NativeOverlayStyleConfig;
        private CancellationTokenSource  showAdsCts;

        private readonly Dictionary<string, NativeOverlayAdValue> adUnitIdToNativeOverlayAd = new();

        private const string AdPlatForm = AdRevenueConstants.ARSourceAdMob;

        /// <summary>
        /// Define our native ad-advanced options.
        /// </summary>
        private NativeAdOptions Option = new NativeAdOptions
        {
            AdChoicesPlacement = AdChoicesPlacement.TopRightCorner,
            MediaAspectRatio   = MediaAspectRatio.Square,
        };

        public void LoadAd(string placement)
        {
            var adUnitId = this.AdUnitId(placement);
            if (!this.adUnitIdToNativeOverlayAd.ContainsKey(adUnitId))
            {
                this.adUnitIdToNativeOverlayAd.Add(adUnitId, new()
                {
                    IsRendered      = false,
                    IsLoaded        = false,
                    NativeOverlayAd = null,
                });
            }

            if (this.adUnitIdToNativeOverlayAd[adUnitId].IsLoaded) return;

            this.logger.Info($"Loading native overlay ad placement {placement}");

            // Create our request used to load the ad.
            var adRequest = new AdRequest();

            // Send the request to load the ad.
            NativeOverlayAd.Load(adUnitId, adRequest, this.Option,
                (ad, error) =>
                {
                    // If the operation failed for a reason.
                    if (error != null)
                    {
                        this.adUnitIdToNativeOverlayAd[adUnitId].IsLoaded = false;
                        this.logger.Error("Native Overlay ad failed to load an ad with error : " + error);
                        return;
                    }
                    // If the operation failed for unknown reasons.
                    // This is an unexpected error, please report this bug if it happens.
                    if (ad == null)
                    {
                        this.adUnitIdToNativeOverlayAd[adUnitId].IsLoaded = false;
                        this.logger.Error("Unexpected error: Native Overlay ad load event fired with null ad and null error.");
                        return;
                    }

                    // The operation completed successfully.
                    this.logger.Info("Native Overlay ad loaded with response : " + ad.GetResponseInfo());
                    this.adUnitIdToNativeOverlayAd[adUnitId].NativeOverlayAd = ad;
                    this.adUnitIdToNativeOverlayAd[adUnitId].IsLoaded        = true;
                    this.SubscribeEvent(adUnitId);
                });
        }

        public bool IsAdReady(string placement)
        {
            return this.adUnitIdToNativeOverlayAd.TryGetValue(this.AdUnitId(placement), out var value) && value.IsLoaded;
        }

        public void ShowAd(string placement, AdViewPosition adViewPosition)
        {
            if (!this.adUnitIdToNativeOverlayAd.TryGetValue(this.AdUnitId(placement), out _))
            {
                this.LoadAd(placement);
            }
            this.logger.Info($"ShowAd placement {placement}, position {adViewPosition}");
            UniTask.WhenAll(UniTask.WaitUntil(() => this.adUnitIdToNativeOverlayAd[this.AdUnitId(placement)].IsLoaded))
                .AttachExternalCancellation((this.showAdsCts = new()).Token)
                .ContinueWith(() =>
                {
                    var value = this.adUnitIdToNativeOverlayAd[this.AdUnitId(placement)];
                    if (this.adUnitIdToNativeOverlayAd[this.AdUnitId(placement)].IsRendered)
                    {
                        value.NativeOverlayAd.SetTemplatePosition(this.GetAdPosition(adViewPosition));
                    }
                    else
                    {
                        this.RenderAd(placement, this.GetAdPosition(adViewPosition));
                    }
                    this.logger.Info("Showing Native Overlay ad.");
                    value.NativeOverlayAd.Show();
                    value.Showing = true;
                }).Forget();
        }

        public void HideAd(string placement)
        {
            this.ResetShowAdsCts();
            var ad = this.adUnitIdToNativeOverlayAd[this.AdUnitId(placement)];
            if (ad.NativeOverlayAd == null) return;
            if (!ad.IsLoaded) return;
            if (!ad.Showing) return;
            this.logger.Info("Hiding Native Overlay ad.");
            this.adUnitIdToNativeOverlayAd[this.AdUnitId(placement)].NativeOverlayAd.Hide();
            ad.Showing = false;
        }

        public void DestroyAd(string placement)
        {
            var adUnitId = this.AdUnitId(placement);
            this.ResetShowAdsCts();
            if (this.adUnitIdToNativeOverlayAd[adUnitId] == null) return;
            this.UnsubscribeEvent(adUnitId);
            this.logger.Info("destroying Native Overlay ad.");
            this.adUnitIdToNativeOverlayAd[adUnitId].NativeOverlayAd.Destroy();
            this.adUnitIdToNativeOverlayAd.Remove(adUnitId);
        }

        public void DestroyAll()
        {
            foreach (var ad in this.adUnitIdToNativeOverlayAd)
            {
                ad.Value.NativeOverlayAd.Destroy();
                this.UnsubscribeEvent(ad.Key);
            }
            this.adUnitIdToNativeOverlayAd.Clear();
        }

        private void RenderAd(string placement, AdPosition adPosition)
        {
            var adUnitId = this.AdUnitId(placement);
            if (this.adUnitIdToNativeOverlayAd[adUnitId] == null) return;
            this.logger.Info("render config style");

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

            this.adUnitIdToNativeOverlayAd[adUnitId].NativeOverlayAd.RenderTemplate(style, adPosition);
            this.adUnitIdToNativeOverlayAd[adUnitId].IsRendered = true;
        }

        private string AdUnitId(string placement)
        {
            if (!this.adMobSettings.NativeOverlayAdIds.ContainsKey(AdPlacement.PlacementWithName(placement)))
            {
                this.logger.Error($"dictionary is missing placement {placement}.");
                return string.Empty;
            }
            return this.adMobSettings.NativeOverlayAdIds[AdPlacement.PlacementWithName(placement)].DefaultValue;
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

        private void SubscribeEvent(string adUnitId)
        {
            var nativeOverlayAd = this.adUnitIdToNativeOverlayAd[adUnitId].NativeOverlayAd;
            nativeOverlayAd.OnAdPaid                    += this.OnAdPaid;
            nativeOverlayAd.OnAdImpressionRecorded      += this.OnAdImpressionRecord;
            nativeOverlayAd.OnAdClicked                 += this.OnAdClicked;
            nativeOverlayAd.OnAdFullScreenContentOpened += this.OnAdFullScreenContentOpened;
            nativeOverlayAd.OnAdFullScreenContentClosed += this.OnAdFullScreenContentClosed;
        }

        private void UnsubscribeEvent(string adUnitId)
        {
            var nativeOverlayAd = this.adUnitIdToNativeOverlayAd[adUnitId].NativeOverlayAd;
            nativeOverlayAd.OnAdPaid                    -= this.OnAdPaid;
            nativeOverlayAd.OnAdImpressionRecorded      -= this.OnAdImpressionRecord;
            nativeOverlayAd.OnAdClicked                 -= this.OnAdClicked;
            nativeOverlayAd.OnAdFullScreenContentOpened -= this.OnAdFullScreenContentOpened;
            nativeOverlayAd.OnAdFullScreenContentClosed -= this.OnAdFullScreenContentClosed;
        }

        // Raised when the ad is estimated to have earned money.
        private void OnAdPaid(AdValue args)
        {
            var adsRevenueEvent = new AdsRevenueEvent
            {
                AdsRevenueSourceId = AdPlatForm,
                AdUnit             = this.adMobSettings.NativeOverlayAdIds.First().Value.DefaultValue,
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
            this.logger.Info("Clicked native overlay ad");
            var adRevenueEvent = new AdInfo(AdPlatForm, this.adMobSettings.NativeOverlayAdIds.First().Value.DefaultValue, AdFormatConstants.NativeOverlay);
            this.signalBus.Fire(new NativeOverlayClickedSignal("", adRevenueEvent));
        }

        private void OnAdImpressionRecord()
        {
            this.logger.Info("Recorded ad impression");
        }

        private void OnAdFullScreenContentOpened()
        {
            this.logger.Info("Closed native overlay ad");
            var adRevenueEvent = new AdInfo(AdPlatForm, this.adMobSettings.NativeOverlayAdIds.First().Value.DefaultValue, AdFormatConstants.NativeOverlay);
            this.signalBus.Fire(new AppOpenFullScreenContentOpenedSignal("", adRevenueEvent));
        }

        private void OnAdFullScreenContentClosed()
        {
            this.logger.Info("Closed native overlay ad");
            var adRevenueEvent = new AdInfo(AdPlatForm, this.adMobSettings.NativeOverlayAdIds.First().Value.DefaultValue, AdFormatConstants.NativeOverlay);
            this.signalBus.Fire(new AppOpenFullScreenContentClosedSignal("", adRevenueEvent));
        }

        #endregion
    }

    public class NativeOverlayAdValue
    {
        public bool            IsRendered;
        public bool            IsLoaded;
        public bool            Showing;
        public NativeOverlayAd NativeOverlayAd;
    }
}
#endif