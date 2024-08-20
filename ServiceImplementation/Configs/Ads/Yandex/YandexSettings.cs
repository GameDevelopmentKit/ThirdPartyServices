namespace ServiceImplementation.Configs.Ads.Yandex
{
    using System;
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [Serializable]
    public class YandexSettings : AdNetworkSettings
    {
        public override Dictionary<AdPlacement, AdId> CustomBannerAdIds       { get; set; }
        public override Dictionary<AdPlacement, AdId> CustomInterstitialAdIds { get; set; }
        public override Dictionary<AdPlacement, AdId> CustomRewardedAdIds     { get; set; }

        public AdId BannerAdId
        {
#if THEONE_ADS_DEBUG
            get => new ("demo-banner-yandex","demo-banner-yandex");
#else
            get => this.mBannerAdId;
#endif
            set => this.mBannerAdId = value;
        }

        public AdId InterstitialAdId
        {
#if THEONE_ADS_DEBUG
            get => new ("demo-interstitial-yandex","demo-interstitial-yandex");
#else
            get => this.mInterstitialAdId;
#endif
            set => this.mInterstitialAdId = value;
        }

        public AdId RewardedAdId
        {
#if THEONE_ADS_DEBUG
            get => new ("demo-rewarded-yandex","demo-rewarded-yandex");
#else
            get => this.mRewardedAdId;
#endif
            set => this.mRewardedAdId = value;
        }

        public AdId AoaAdId
        {
#if THEONE_ADS_DEBUG
            get => new ("demo-appopenad-yandex","demo-appopenad-yandex");
#else
            get => this.mAoaAdId;
#endif
            set => this.mAoaAdId = value;
        }

        [SerializeField, LabelText("Banner"), BoxGroup("Ads Id")]
        private AdId mBannerAdId;

        [SerializeField, LabelText("Interstitial"), BoxGroup("Ads Id")]
        private AdId mInterstitialAdId;

        [SerializeField, LabelText("Rewarded"), BoxGroup("Ads Id")]
        private AdId mRewardedAdId;

        [SerializeField, LabelText("AOA"), BoxGroup("Ads Id")]
        private AdId mAoaAdId;

        [SerializeField, PropertyOrder(-1)]
        private YandexDashboard dashboard;

        public YandexDashboard Dashboard => this.dashboard;
    }
}