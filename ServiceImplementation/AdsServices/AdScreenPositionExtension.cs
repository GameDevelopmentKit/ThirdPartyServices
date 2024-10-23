namespace ServiceImplementation.AdsServices
{
    using Core.AdsServices;
    using UnityEngine;

    public static class AdScreenPositionExtension
    {
        private const int MREC_WIDTH  = 300;
        private const int MREC_HEIGHT = 250;
        
        public static AdScreenPosition ApexToUnityCoordinateSystem(this AdScreenPosition adScreenPosition)
        {
            return new AdScreenPosition(adScreenPosition.x, Mathf.Abs(adScreenPosition.y - Screen.safeArea.height));
        }
        
        public static AdScreenPosition VectorToUnityCoordinateSystem(this AdScreenPosition adScreenPosition)
        {
            return new AdScreenPosition(adScreenPosition.x, - adScreenPosition.y);
        }

        #if APPLOVIN
        public static AdScreenPosition ToApplovinPosition(this AdScreenPosition adScreenPosition)
        {
            var density    = MaxSdkUtils.GetScreenDensity();
            var connerPosX = adScreenPosition.x - MREC_WIDTH  * density * (adScreenPosition.x / Screen.safeArea.width);
            var connerPosY = adScreenPosition.y - MREC_HEIGHT * density * (adScreenPosition.y / Screen.safeArea.height);

            return new AdScreenPosition((connerPosX / density), (connerPosY / density));
        }
        #endif

        #if ADMOB
        public static AdScreenPosition ToAdmobPosition(this AdScreenPosition adScreenPosition)
        {
            var dpW = ToDp(Screen.width);
            var dpH = ToDp(Screen.height);

            var connerPosX = dpW * (adScreenPosition.x / Screen.width) - MREC_WIDTH * (Screen.dpi / 160f) * (adScreenPosition.x / Screen.width) * dpW / Screen.width;

            var connerPosY = dpH * (adScreenPosition.y / Screen.height) - MREC_HEIGHT * (Screen.dpi / 160f) * (adScreenPosition.y / Screen.height) * dpH / Screen.height;

            return new AdScreenPosition(connerPosX, connerPosY);
        }

        public static float ToDp(float pixel) { return pixel * 160f / Screen.dpi; }
        #endif
    }
}