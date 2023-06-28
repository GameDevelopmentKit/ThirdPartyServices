namespace ServiceImplementation.Configs.Ads
{
    using System.Collections.Generic;

    public abstract class AdNetworkSettings
    {
        public abstract Dictionary<AdPlacement, AdId> CustomBannerAdIds { get; set; }

        
        public abstract Dictionary<AdPlacement, AdId> CustomInterstitialAdIds {get; set; }

       
        public abstract Dictionary<AdPlacement, AdId> CustomRewardedAdIds { get; set; }

    }
}