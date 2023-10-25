#if APPLOVIN && THEONE_APS_ENABLE
namespace ServiceImplementation.AdsServices.AppLovin
{
    using AmazonAds;
    using Core.AdsServices;
    using GameFoundation.Scripts.Utilities.LogService;
    using ServiceImplementation.Configs;
    using ServiceImplementation.Configs.Ads;
    using Zenject;

    public class AmazonApplovinAdsWrapper : AppLovinAdsWrapper
    {
        #region Cache

        // Amazon Cache
        private AmazonApplovinSetting amazonSetting;

        private bool isFirstInterstitialRequest  = true;
        private bool isFirstRewardedVideoRequest = true;
        private bool isFirstMRecRequest          = true;

        private APSBannerAdRequest       bannerAdRequest;
        private APSBannerAdRequest       mRecAdsRequest;
        private APSInterstitialAdRequest interstitialAdRequest;
        private APSVideoAdRequest        rewardedVideoAdRequest;

        #endregion

        private const string AmazonResponseMessage = "amazon_ad_response";
        private const string AmazonErrorMessage    = "amazon_ad_error";

        public AmazonApplovinAdsWrapper(ILogService logService, SignalBus signalBus, AdServicesConfig adServicesConfig,
            ThirdPartiesConfig thirdPartiesConfig)
            : base(logService, signalBus, adServicesConfig, thirdPartiesConfig)
        {
            this.amazonSetting = thirdPartiesConfig.AdSettings.AppLovin.AmazonApplovinSetting;
        }

        public override void Initialize()
        {
            // Amazon
            Amazon.Initialize(this.amazonSetting.AppId);
            Amazon.EnableTesting(this.amazonSetting.EnableTesting);
            Amazon.EnableLogging(this.amazonSetting.EnableLogging);
            Amazon.UseGeoLocation(this.amazonSetting.UseGeoLocation);
            Amazon.SetMRAIDPolicy(this.amazonSetting.MRAIDPolicy);
            Amazon.SetAdNetworkInfo(new AdNetworkInfo(DTBAdNetwork.MAX));
            Amazon.SetMRAIDSupportedVersions(new[] { "1.0", "2.0", "3.0" });

            base.Initialize();
        }

        #region MREC

        protected override void InternalShowMREC(string id, MaxSdkBase.AdViewPosition position)
        {
            var amazonId = this.amazonSetting.AmazonMRecAdId.Id;
            if (this.isFirstMRecRequest && !string.IsNullOrEmpty(amazonId))
            {
                this.isFirstMRecRequest = false;

                this.mRecAdsRequest = new APSBannerAdRequest(300, 250, amazonId);
                this.mRecAdsRequest.onSuccess += response =>
                    {
                        MaxSdk.SetMRecLocalExtraParameter(id, AmazonResponseMessage, response.GetResponse());
                        MaxSdk.UpdateMRecPosition(id, position);
                        MaxSdk.ShowMRec(id);
                    }
                    ;
                this.mRecAdsRequest.onFailedWithError += error =>
                {
                    MaxSdk.SetMRecLocalExtraParameter(id, AmazonErrorMessage, error.GetAdError());
                    MaxSdk.UpdateMRecPosition(id, position);
                    MaxSdk.ShowMRec(id);
                };

                this.mRecAdsRequest.LoadAd();
            }
            else
            {
                MaxSdk.UpdateMRecPosition(id, position);
                MaxSdk.ShowMRec(id);
            }
        }

        #endregion

        #region Banner

        protected override void InternalCreateBanner(string id, BannerAdsPosition position, BannerSize bannerSize)
        {
            var amazonId = this.amazonSetting.AmazonBannerAdId.Id;
            if (!string.IsNullOrEmpty(amazonId))
            {
                this.bannerAdRequest = new APSBannerAdRequest(bannerSize.width, bannerSize.height, amazonId);

                this.bannerAdRequest.onFailedWithError += error =>
                {
                    MaxSdk.SetBannerLocalExtraParameter(id, AmazonErrorMessage, error.GetAdError());
                    this.CreateAdBanner(id, position);
                };

                this.bannerAdRequest.onSuccess += response =>
                {
                    MaxSdk.SetBannerLocalExtraParameter(id, AmazonResponseMessage, response.GetResponse());
                    this.CreateAdBanner(id, position);
                };
                this.bannerAdRequest.LoadAd();
            }
            else
            {
                this.CreateAdBanner(id, position);
            }
        }

        #endregion

        #region Interstitial

        protected override void InternalLoadInterstitialAd(AdPlacement adPlacement)
        {
            if (!this.IsInterstitialPlacementReady(adPlacement.Name, out var id)) return;

            var amazonId = this.amazonSetting.AmazonInterstitialAdId.Id;
            if (this.isFirstInterstitialRequest && !string.IsNullOrEmpty(amazonId))
            {
                this.isFirstInterstitialRequest = false;
                this.interstitialAdRequest      = new APSInterstitialAdRequest(amazonId);

                this.interstitialAdRequest.onSuccess += response =>
                {
                    MaxSdk.SetInterstitialLocalExtraParameter(id, AmazonResponseMessage, response.GetResponse());
                    MaxSdk.LoadInterstitial(id);
                };
                this.interstitialAdRequest.onFailedWithError += error =>
                {
                    MaxSdk.SetInterstitialLocalExtraParameter(id, AmazonErrorMessage, error.GetAdError());
                    MaxSdk.LoadInterstitial(id);
                };

                this.interstitialAdRequest.LoadAd();
            }
            else MaxSdk.LoadInterstitial(id);
        }

        #endregion

        #region Rewarded

        protected override void InternalLoadRewarded(AdPlacement placement)
        {
            if (!this.IsRewardedPlacementReady(placement.Name, out var id)) return;

            var amazonId = this.amazonSetting.AmazonRewardedAdId.Id;
            if (this.isFirstRewardedVideoRequest && !string.IsNullOrEmpty(amazonId))
            {
                this.isFirstRewardedVideoRequest = false;
                this.rewardedVideoAdRequest      = new APSVideoAdRequest(320, 480, amazonId);

                this.rewardedVideoAdRequest.onSuccess += response =>
                {
                    MaxSdk.SetRewardedAdLocalExtraParameter(id, AmazonResponseMessage, response.GetResponse());
                    MaxSdk.LoadRewardedAd(id);
                };

                this.rewardedVideoAdRequest.onFailedWithError += error =>
                {
                    MaxSdk.SetRewardedAdLocalExtraParameter(id, AmazonErrorMessage, error.GetAdError());
                    MaxSdk.LoadRewardedAd(id);
                };

                this.rewardedVideoAdRequest.LoadAd();
            }
            else MaxSdk.LoadRewardedAd(id);
        }

        #endregion
    }
}

#endif