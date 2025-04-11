namespace ServiceImplementation.Configs.Ads
{
    using System.Collections.Generic;

    public abstract class AdNetworkSettings
    {
        public abstract Dictionary<AdPlacement, CrossPlatformValue> CustomBannerAdIds { get; set; }

        public abstract Dictionary<AdPlacement, CrossPlatformValue> CustomInterstitialAdIds { get; set; }

        public abstract Dictionary<AdPlacement, CrossPlatformValue> CustomRewardedAdIds { get; set; }
    }
}