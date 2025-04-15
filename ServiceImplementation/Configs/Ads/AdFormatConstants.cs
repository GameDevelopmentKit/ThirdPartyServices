namespace ServiceImplementation
{
    public class AdFormatConstants
    {
        #if IRONSOURCE
        public const string Banner            = "banner";
        public const string Interstitial      = "interstitial";
        public const string Rewarded          = "rewarded_video";
        #else
        public const string Banner            = "BANNER";
        public const string Interstitial      = "INTER";
        public const string Rewarded          = "REWARDED";
        #endif
        public const string AppOpen           = "AOA";
        public const string Native            = "NATIVE";
        public const string MREC              = "MREC";
        public const string CollapsibleBanner = "COLLAPSIBLE_BANNER";
    }
}