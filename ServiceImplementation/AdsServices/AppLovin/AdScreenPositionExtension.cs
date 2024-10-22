namespace ServiceImplementation.AdsServices.AppLovin
{
    using Core.AdsServices;
    using UnityEngine.Device;

    public static class AdScreenPositionExtension
    {
        private const int MREC_WIDTH  = 300;
        private const int MREC_HEIGHT = 250;
        
        #if APPLOVIN
        public static AdScreenPosition GetMRECPosition(this AdScreenPosition adScreenPosition)
        {
            var density    = MaxSdkUtils.GetScreenDensity();
            var connerPosX = adScreenPosition.x - MREC_WIDTH  * density * (adScreenPosition.x / Screen.safeArea.width);
            var connerPosY = adScreenPosition.y - MREC_HEIGHT * density * (adScreenPosition.y / Screen.safeArea.height);

            return new AdScreenPosition((connerPosX / density), (connerPosY / density));
        }
        #endif
    }
}