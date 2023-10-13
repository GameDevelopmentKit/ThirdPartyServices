namespace Core.AdsServices
{
    #if ADMOB
    using GoogleMobileAds.Api;
    #endif

    public enum AdViewPosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        CenterLeft,
        Centered,
        CenterRight,
        BottomLeft,
        BottomCenter,
        BottomRight,
    }

    public static class AdViewPositionExtensions
    {
        #if ADMOB
        public static AdPosition ToAdMobAdPosition(this AdViewPosition adViewPosition)
        {
            return adViewPosition switch
            {
                AdViewPosition.TopLeft      => AdPosition.TopLeft,
                AdViewPosition.TopCenter    => AdPosition.Top,
                AdViewPosition.TopRight     => AdPosition.TopRight,
                AdViewPosition.CenterLeft   => AdPosition.Center,
                AdViewPosition.Centered     => AdPosition.Center,
                AdViewPosition.CenterRight  => AdPosition.Center,
                AdViewPosition.BottomLeft   => AdPosition.BottomLeft,
                AdViewPosition.BottomCenter => AdPosition.Bottom,
                AdViewPosition.BottomRight  => AdPosition.BottomRight,
                _                           => AdPosition.Center,
            };
        }
        #endif
    }
}