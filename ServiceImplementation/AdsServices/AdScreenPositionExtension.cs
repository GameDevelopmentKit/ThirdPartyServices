namespace ServiceImplementation.AdsServices
{
    using Core.AdsServices;
    using UnityEngine;

    public static class AdScreenPositionExtension
    {
        private const int   MREC_WIDTH          = 300;
        private const int   MREC_HEIGHT         = 250;
        private const float ADMOB_CANVAS_HEIGHT = 1080;

        #if APPLOVIN
        public static AdScreenPosition GetMRECPosition(this AdScreenPosition adScreenPosition)
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
            // var admobCanvasWidth = ADMOB_CANVAS_HEIGHT * Screen.width / Screen.height;
            //
            // var connerPosX = (admobCanvasWidth    - MREC_WIDTH)  * (adScreenPosition.x / Screen.width);
            // var connerPosY = (ADMOB_CANVAS_HEIGHT - MREC_HEIGHT) * (adScreenPosition.y / Screen.height);
            //
            // return new AdScreenPosition(connerPosX, connerPosY);
            return adScreenPosition;
        }
        #endif
    }
}