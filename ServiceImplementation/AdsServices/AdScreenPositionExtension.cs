namespace ServiceImplementation.AdsServices
{
    using Core.AdsServices;
    using UnityEngine;

    public static class AdScreenPositionExtension
    {
        private const int MREC_WIDTH  = 300;
        private const int MREC_HEIGHT = 250;

        public static AdScreenPosition CanvasToUnityCoordinateSystem(this AdScreenPosition adScreenPosition)
        {
            return new AdScreenPosition(adScreenPosition.x, Mathf.Abs(adScreenPosition.y - Screen.safeArea.height));
        }

        public static AdScreenPosition FlipY(this AdScreenPosition adScreenPosition)
        {
            return new AdScreenPosition(PixelToDp(adScreenPosition.x), -PixelToDp(adScreenPosition.y));
        }

        #if APPLOVIN
        public static AdScreenPosition ToApplovinPosition(this AdScreenPosition adScreenPosition)
        {
            // calculate in canvas coordinate system
            var density = MaxSdkUtils.GetScreenDensity();
            var connerPosX = adScreenPosition.x - MREC_WIDTH  * density * (adScreenPosition.x / Screen.safeArea.width);
            var connerPosY = adScreenPosition.y - MREC_HEIGHT * density * (adScreenPosition.y / Screen.safeArea.height);

            return new AdScreenPosition((connerPosX / density), (connerPosY / density));
        }
        #endif

        #if ADMOB
        public static AdScreenPosition ToAdmobPosition(this AdScreenPosition adScreenPosition)
        {
            // Calculate in canvas coordinate system
            var dpW = PixelToDp(Screen.width);
            var dpH = PixelToDp(Screen.height);

            float x = adScreenPosition.x;
            float y = adScreenPosition.y;

            // Adjust for iOS coordinate system (top-left to bottom-left)
            #if UNITY_IOS
            y = Screen.height - y;
            #endif

            // Calculate adjusted position considering device scaling
            float scaleFactor = 1f;
            #if UNITY_IOS
            scaleFactor = GetIOSScaleFactor();
            #endif

            var connerPosX = dpW * (x / Screen.width) - MREC_WIDTH * (x / Screen.width) * scaleFactor;
            var connerPosY = dpH * (y / Screen.height) - MREC_HEIGHT * (y / Screen.height) * scaleFactor;

            return new AdScreenPosition(connerPosX, connerPosY);
        }

        public static float PixelToDp(float pixel)
        {
            #if UNITY_IOS
            // Adjust for iOS physical DPI calculation
            float scaleFactor = GetIOSScaleFactor();
            return pixel * 160f / (Screen.dpi * scaleFactor);
            #else
            return pixel * 160f / Screen.dpi;
            #endif
        }

        private static float GetIOSScaleFactor()
        {
            // This should be replaced with actual scale factor detection
            // For iPad Gen 9, scale factor is 2.0f. This is a simplified approach.
            return Screen.width >= 2000 ? 2.0f : 1.0f; // Heuristic for iPad detection
        }
        #endif
    }
}