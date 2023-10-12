namespace Core.AdsServices
{
    public enum AdNetwork
    {
        None,
        AdColony,
        AdMob,
        AppLovin,
        AudienceNetwork,
        Chartboost,
        FairBid,
        IronSource,
        TapJoy,
        UnityAds,
        Vungle,
    }

    public enum BannerAdNetwork
    {
        None            = AdNetwork.None,
        AdColony        = AdNetwork.AdColony,
        AdMob           = AdNetwork.AdMob,
        AppLovin        = AdNetwork.AppLovin,
        AudienceNetwork = AdNetwork.AudienceNetwork,
        FairBid         = AdNetwork.FairBid,
        IronSource      = AdNetwork.IronSource,
        UnityAds        = AdNetwork.UnityAds,
        Vungle          = AdNetwork.Vungle,
    }

    public enum InterstitialAdNetwork
    {
        None            = AdNetwork.None,
        AdColony        = AdNetwork.AdColony,
        AdMob           = AdNetwork.AdMob,
        AppLovin        = AdNetwork.AppLovin,
        AudienceNetwork = AdNetwork.AudienceNetwork,
        Chartboost      = AdNetwork.Chartboost,
        FairBid         = AdNetwork.FairBid,
        IronSource      = AdNetwork.IronSource,
        TapJoy          = AdNetwork.TapJoy,
        UnityAds        = AdNetwork.UnityAds,
        Vungle          = AdNetwork.Vungle,
    }

    public enum RewardedAdNetwork
    {
        None            = AdNetwork.None,
        AdColony        = AdNetwork.AdColony,
        AdMob           = AdNetwork.AdMob,
        AppLovin        = AdNetwork.AppLovin,
        AudienceNetwork = AdNetwork.AudienceNetwork,
        Chartboost      = AdNetwork.Chartboost,
        FairBid         = AdNetwork.FairBid,
        IronSource      = AdNetwork.IronSource,
        TapJoy          = AdNetwork.TapJoy,
        UnityAds        = AdNetwork.UnityAds,
        Vungle          = AdNetwork.Vungle,
    }

    public enum RewardedInterstitialAdNetwork
    {
        None  = AdNetwork.None,
        AdMob = AdNetwork.AdMob,
    }

    public enum ConsentStatus
    {
        /// <summary>
        /// Consent status is not set, subject to request from user.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Consent is granted by the user.
        /// </summary>
        Granted,

        /// <summary>
        /// Consent is revoked by the user.
        /// </summary>
        Revoked,
    }
}