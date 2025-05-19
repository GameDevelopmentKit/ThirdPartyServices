namespace ServiceImplementation.Configs.Ads
{
    using System;
    using UnityEngine;
    #if ADMOB
    using GoogleMobileAds.Api;
    #endif

    [Serializable]
    public class NativeOverlayStyleConfig
    {
        public string Template            = "medium";
        public Color  MainBackgroundColor = Color.red;
        public Color  BackgroundColor     = Color.green;
        public Color  TextColor           = Color.white;
        public int    FontSize            = 9;
        #if ADMOB
        public NativeTemplateFontStyle Style = NativeTemplateFontStyle.Bold;
        #endif
    }
}